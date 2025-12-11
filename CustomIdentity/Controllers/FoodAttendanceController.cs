using CustomIdentity.Data;
using CustomIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

[Authorize]
public class FoodAttendanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    
        public FoodAttendanceController(ApplicationDbContext context, UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    private static readonly TimeZoneInfo NepalTz =
    TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");

    // Returns Nepal-local "now"
    private static DateTime NowNepal() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NepalTz);

    // Returns a UTC DateTime that represents 00:00 of the given Nepal-local date
    // (i.e., the “day anchor” you’ll store in the DB).
    private static DateTime NepalDayAnchorUtc(DateTime nepalLocalDate) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(nepalLocalDate.Date, DateTimeKind.Unspecified), NepalTz);

    private (int hour, int minute) GetCutoffFromConfig()
    {
        string submissionTime = _configuration["SparrowSms:SubmissionTime"]!;
        // Format is "mm HH * * ..." → parts[0] = minutes, parts[1] = hours
        var parts = submissionTime.Split(' ');
        int minute = int.Parse(parts[0]);
        int hour = int.Parse(parts[1]);
        return (hour, minute);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "User, Admin")]
    public async Task<IActionResult> SubmitPreference(FoodAttendanceViewModel model)
    {
        var nowNpt = NowNepal();
        var (hour, minute) = GetCutoffFromConfig();
        var cutoffNpt = nowNpt.Date.AddHours(hour).AddMinutes(minute);

        if (nowNpt >= cutoffNpt)
        {
            ModelState.AddModelError("", $"The submission window has closed for today (after {hour:D2}:{minute:D2} NPT).");
            return await SubmitPreference();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        // Non-Veg only on Fridays in Nepal
        if (model.Preference && nowNpt.DayOfWeek != DayOfWeek.Friday)
        {
            ModelState.AddModelError("", "Non-Veg can only be selected on Fridays.");
            return await SubmitPreference();
        }

        // Compute the Nepal day anchor in UTC (this is what we store/query)
        var dayAnchorUtc = NepalDayAnchorUtc(nowNpt);

        // Upsert one record per user per Nepal day
        var existing = await _context.FoodAttendances
            .FirstOrDefaultAsync(fp => fp.UserId == user.Id && fp.Date == dayAnchorUtc);

        var preferenceValue = model.Preference ? "Non-Veg" : "Veg";

        if (existing is null)
        {
            _context.FoodAttendances.Add(new FoodAttendance
            {
                UserId = user.Id,
                UserName = user.Name ?? user.UserName ?? "User",
                Preference = preferenceValue,
                Date = dayAnchorUtc // Kind=Utc, safe for timestamptz
            });
        }
        else
        {
            existing.Preference = preferenceValue;
            _context.FoodAttendances.Update(existing);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Your preference has been recorded for today.";
        return RedirectToAction(nameof(SubmitPreference));
    }



    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    public async Task<IActionResult> SubmitPreference()
    {
        // “Today” in Nepal
        var todayNpt = NowNepal().Date;
        var dayAnchorUtc = NepalDayAnchorUtc(todayNpt);

        // Counts for the Nepal day
        ViewBag.TodayResponsesCount = await _context.FoodAttendances
            .CountAsync(fp => fp.Date == dayAnchorUtc);

        ViewBag.VegCountToday = await _context.FoodAttendances
            .CountAsync(fp => fp.Date == dayAnchorUtc && fp.Preference == "Veg");

        ViewBag.NonVegCountToday = await _context.FoodAttendances
            .CountAsync(fp => fp.Date == dayAnchorUtc && fp.Preference == "Non-Veg");

        var (hour, minute) = GetCutoffFromConfig();
        ViewBag.CronTime = $"{hour:D2}:{minute:D2}";

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