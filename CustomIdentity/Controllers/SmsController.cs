﻿using CustomIdentity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SmsController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly SparrowSmsService _smsService;
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public SmsController(SparrowSmsService smsService, IConfiguration configuration, ApplicationDbContext context, EmailService emailService)
    {
        _smsService = smsService;
        _configuration = configuration;
        _context = context;
        _emailService = emailService;
    }

    public async Task SendScheduledSms()
    {
        /*var numbers = await GetMobileNumbers();
        foreach (var number in numbers)
        {*/
            DateTime utcDateTime = DateTime.UtcNow.Date; // Use UTC time
            var totalAttendance = await _context.FoodAttendances.CountAsync(a => a.Date == utcDateTime);
            var vegCount = await _context.FoodAttendances.CountAsync(a => a.Date == utcDateTime && a.Preference == "Veg");
            var nonVegCount = await _context.FoodAttendances.CountAsync(a => a.Date == utcDateTime && a.Preference == "Non-Veg");

            string message = $"Total head count: {totalAttendance}. Veg: {vegCount}, Non-Veg: {nonVegCount}.";

            string token = _configuration["SparrowSms:token"]!;
            string from = _configuration["SparrowSms:from"]!;
            string to = _configuration["SparrowSms:to"]!;
            string Remail = _configuration["EmailSettings:ReceiverEmail"]!;

            string smsResult = await _smsService.SendSms(from, token, to, message);

            if (smsResult == "error")
            {
                await _emailService.SendEmailAsync(Remail, "Sms Not Delivered", "Sms failed to be delivered to KL.");
            }
            else
            {
                await _emailService.SendEmailAsync(Remail, "Sms Delivered", "Sms has been delivered to KL.");
            }
        }
    }

    /*public async Task<List<string>> GetMobileNumbers()
    {
        await Task.Delay(100); // Simulating async database call
        return new List<string> { "9848580066" }; // Replace with actual data fetching logic
    }
}*/
