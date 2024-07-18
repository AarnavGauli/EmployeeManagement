using CustomIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomIdentity.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
            {

            }

        public DbSet<FoodAttendance> FoodAttendances { get; set; }

        public DbSet<LeaveDetail> LeaveDetails { get; set; }

        
    }

    
}
