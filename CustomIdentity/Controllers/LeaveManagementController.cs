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
    private readonly EmailService _emailService;


    public LeaveManagementController(ApplicationDbContext context, UserManager<AppUser> userManager, EmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    // Index action to list leave requests
    [Authorize]
    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 7; // Number of items per page

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

        // Calculate total number of items
        int totalItems = await leaveDetailsQuery.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Fetch only the items for the current page
        var leaveDetails = await leaveDetailsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LeaveDetailVM
            {
                Id = l.Id,
                UserName = l.UserName,
                LeaveReason = l.LeaveReason,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                StartTime = l.StartTime,
                EndTime = l.EndTime,
                LeaveType = l.LeaveType,
                Status = l.Status
            }).ToListAsync();

        var approvedLeavesCount = await _context.LeaveDetails
            .CountAsync(l => l.UserName == user!.UserName && l.Status == "Approved");

        ViewBag.ApprovedLeaveCount = approvedLeavesCount;

        // Pass pagination data to the view
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;

        return View(leaveDetails);
    }

    public async Task<IActionResult> List()
    {
        // Group by UserName and sum LeavesTaken
        var leaveTotals = await _context.LeaveDetails
            .Where(ld => ld.Status == "Approved") // Filter by Approved status
            .GroupBy(ld => ld.UserName)
            .Select(g => new
            {
                UserName = g.Key,
                TotalLeavesTaken = g.Count() // Count of approved leave requests per user
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
                LeaveCategory = model.LeaveCategory,
                LeaveType = model.LeaveType,
                Status = "Pending"
            };

            _context.LeaveDetails.Add(leaveDetail);
            await _context.SaveChangesAsync();

            string subject = $"Leave Request Submission from {model.UserName}";
            string message = $"Dear Team,\n\n" +
                         $"Please be informed that {model.UserName} has submitted a leave request with the following details:\n\n" +
                         $"Reason for Leave: {model.LeaveReason}\n" +
                         $"Leave Category: {model.LeaveCategory}\n" +
                         $"Start Date: {model.StartDate.ToShortDateString()}\n" +
                         $"End Date: {model.EndDate.ToShortDateString()}\n" +
                         $"Leave Type: {model.LeaveType}\n";

                        // Add StartTime and EndTime if LeaveType is HalfDay
                        if (model.LeaveType == "Half Day")
                        {
                            message += $"Start Time: {model.StartTime}\n" +
                                       $"End Time: {model.EndTime}\n";
                        }

                        // Closing message
                    message += "\nKindly review and process this request at your earliest convenience.\n\n" +
                               "Thank you,\n" +
                                $"{model.UserName}";

            await _emailService.SendEmailAsync("arnavgauli66@gmail.com", subject, message);

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
