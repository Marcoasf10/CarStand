using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StandCarros.Models;

namespace StandCarros.Data
{
    public class StandCarrosContext : IdentityDbContext<IdentityUser>
    {
        public StandCarrosContext (DbContextOptions<StandCarrosContext> options)
            : base(options)
        {
        }

        public DbSet<StandCarros.Models.Car> Car { get; set; } = default!;
        public DbSet<StandCarros.Models.Photo> Photos { get; set; } 
        
    }
}
