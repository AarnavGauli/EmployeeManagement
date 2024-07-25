using CustomIdentity.Models;

public class LeaveDetail
{
    public int Id { get; set; } // Unique identifier for the leave request
    public string? UserName { get; set; } // Employee's name
    public DateTime StartDate { get; set; } // Start date of leave
    public DateTime EndDate { get; set; } // End date of leave
    public TimeSpan StartTime { get; set; }  // New property
    public TimeSpan EndTime { get; set; }
    public string? LeaveType { get; set; }
    public string? Status { get; set; } // Status of the leave request (e.g., Pending, Approved, Rejected)
    public string? LeaveReason { get; set; }
    public int LeavesTaken { get; set; }
    public int LeavesRemaining { get; set; }
    public string? LeaveCategory { get; set; }

    // Add more properties as needed

    // Optionally, you can add calculated properties or methods here
}
