using CustomIdentity.Data;
using CustomIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class FoodAttendanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    
        public FoodAttendanceController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    
    [Authorize(Roles = "User, Admin")]
    public async Task<IActionResult> SubmitPreference(FoodAttendanceViewModel model)
    {
        
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.UtcNow.Date; // Use UTC date to avoid timezone issues

            var UserName = user!.Name;


        // Get existing responses for the current day
        var existingResponses = _context.FoodAttendances
                .Where(fp => fp.UserId == user.Id && fp.Date == today)
                .ToList();

            // Delete existing responses for the current day (optional, if you want to allow multiple responses per day)
            _context.FoodAttendances.RemoveRange(existingResponses);

            // Convert the boolean preference to "Yes" or "No"
            var preferenceValue = model.Preference ? "Non-Veg" : "Veg";

            

        var foodAttendance = new FoodAttendance
            {
                UserId = user.Id,
                UserName = UserName,
                Preference = preferenceValue,
                Date = today
            };

            _context.FoodAttendances.Add(foodAttendance);
            await _context.SaveChangesAsync();

            // Fetch the count of responses for today
            var todayResponsesCount = _context.FoodAttendances
                .Where(fp => fp.Date == today)
                .Count();

            ViewBag.TodayResponsesCount = todayResponsesCount;

        var vegCountToday = _context.FoodAttendances
            .Where(fp => fp.Preference == "Veg" && fp.Date == today)
            .Count();

        ViewBag.VegCountToday = vegCountToday;

        var nonvegCountToday = _context.FoodAttendances
            .Where(fp => fp.Preference == "Non-Veg" && fp.Date == today)
            .Count();

        ViewBag.NonVegCountToday = nonvegCountToday;


        return View();    
        
    }

   

    // GET method to show the checkbox page
    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    public async Task<IActionResult> SubmitPreference()
    {
        var today = DateTime.UtcNow.Date;

        // Fetch the count of responses for today
        var todayResponsesCount = await _context.FoodAttendances
            .Where(fp => fp.Date == today)
            .CountAsync();

        ViewBag.TodayResponsesCount = todayResponsesCount;

        var vegCountToday = _context.FoodAttendances
            .Where(fp => fp.Preference == "Veg" && fp.Date == today)
            .Count();

        ViewBag.VegCountToday = vegCountToday;

        var nonvegCountToday = _context.FoodAttendances
            .Where(fp => fp.Preference == "Non-Veg" && fp.Date == today)
            .Count();

        ViewBag.NonVegCountToday = nonvegCountToday;

        return View();
    }


    [Authorize(Roles = "User, Admin")]
    public async Task<IActionResult> AttendanceSummary()
    {
        var summaries = await _context.FoodAttendances
            .GroupBy(fp => fp.Date.Date) // Group by date 
            .Select(g => new
            {
                Date = g.Key,
                TotalPreferences = g.Count()
            })
            .ToListAsync();

        return Json(summaries);
    }
}