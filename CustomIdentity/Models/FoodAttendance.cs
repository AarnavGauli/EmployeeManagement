using CustomIdentity.Models;

public class FoodAttendance
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Preference { get; set; }
    public DateTime Date { get; set; }
    public AppUser? User { get; set; }
}

