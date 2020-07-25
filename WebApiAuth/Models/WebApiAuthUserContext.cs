using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiAuth.Models
{
    public class WebApiAuthUserContext : IdentityDbContext<WebApiAuthUser>
    {
        public WebApiAuthUserContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<WebApiAuthUser>(user => user.HasIndex(x => x.Locale).IsUnique(false));

        }
    }
}