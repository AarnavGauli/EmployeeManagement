using System;
using System.ComponentModel.DataAnnotations;

namespace CustomIdentity.ViewModels
{
    public class LeaveDetailVM
    {
        public int Id { get; set; }

        public string? UserName { get; set; }

        [Required(ErrorMessage = "Leave reason is required")]
        [DataType(DataType.MultilineText)]
        public string? LeaveReason { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int LeavesTaken { get; set; }

        public string? LeaveType { get; set; }
        public string? Status { get; set; }
    }
}
