using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StrangersChat2.Models;

namespace StrangersChat2.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //Constructor
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }


    }
}
