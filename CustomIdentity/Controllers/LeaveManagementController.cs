using CustomIdentity.Data;
using CustomIdentity.Models;
using CustomIdentity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class LeaveManagementController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    

    public LeaveManagementController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
      
    }

    // Index action to list leave requests
    [Authorize]
    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 10;

        var user = await _userManager.GetUserAsync(User);

        // Check if the user is an admin
        bool isAdmin = User.IsInRole("Admin");

        var leaveDetailsQuery = _context.LeaveDetails.AsQueryable();

        // If not an admin, filter leave requests to show only those of the logged-in user
        if (!isAdmin)
        {
            leaveDetailsQuery = leaveDetailsQuery.Where(l => l.UserName == user!.UserName);
        }

        // Order by StartDate in descending order to show recently added requests on top
        leaveDetailsQuery = leaveDetailsQuery.OrderByDescending(l => l.StartDate);

        var leaveDetails = await leaveDetailsQuery
            .Select(l => new LeaveDetailVM
            {
                Id = l.Id,
                UserName = l.UserName,
                LeaveReason = l.LeaveReason,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                StartTime = l.StartTime,
                EndTime = l.EndTime,
                Status = l.Status
            }).ToListAsync();

        var approvedLeavesCount = await _context.LeaveDetails
        .CountAsync(l => l.UserName == user!.UserName && l.Status == "Approved");

        ViewBag.ApprovedLeaveCount = approvedLeavesCount;

        // Calculate total number of pages
        int totalItems = await leaveDetailsQuery.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Pass data to the view
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;

        return View(leaveDetails);
    }

    public async Task<IActionResult> List()
    {
        // Group by UserName and sum LeavesTaken
        var leaveTotals = await _context.LeaveDetails
            .GroupBy(ld => ld.UserName)
            .Select(g => new
            {
                UserName = g.Key,
                TotalLeavesTaken = g.Sum(ld =>ld.LeavesTaken)
            })
            .ToListAsync();

        // Map to ViewModel
        var leaveDetailVMs = leaveTotals.Select(lt => new LeaveDetailVM
        {
            UserName = lt.UserName,
            LeavesTaken = lt.TotalLeavesTaken
        }).ToList();

        return View(leaveDetailVMs);
    }

    // Get action for the Create page
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        var model = new LeaveDetailVM
        {
            UserName = user!.UserName,
            StartDate = DateTime.UtcNow.Date, // Store in UTC
            EndDate = DateTime.UtcNow.Date
        };
            
        return View(model);
    }

    // Post action to create a new leave request
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(LeaveDetailVM model)
    {
        if (ModelState.IsValid)
        {
            var leaveDetail = new LeaveDetail
            {
                UserName = model.UserName,
                LeaveReason = model.LeaveReason,
                StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc),
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                LeaveType = model.LeaveType,
                Status = "Pending"
            };

            _context.LeaveDetails.Add(leaveDetail);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // Post action to approve a leave request
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var leaveDetail = await _context.LeaveDetails.FindAsync(id);
        if (leaveDetail != null)
        {
            leaveDetail.Status = "Approved";
            // Increment the LeavesTaken count
            leaveDetail.LeavesTaken++;
            _context.Update(leaveDetail);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // Post action to disapprove a leave request
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disapprove(int id)
    {
        var leaveDetail = await _context.LeaveDetails.FindAsync(id);
        if (leaveDetail != null)
        {
            leaveDetail.Status = "Disapproved";
            _context.Update(leaveDetail);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    
}
