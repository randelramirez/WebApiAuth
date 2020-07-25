using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAuth.Models;

namespace WebApiAuth
{
    public static class StartupHelper
    {
        public static void SeedDataContext(this IApplicationBuilder application)
        {
            using (var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DataContext>();

                context.Database.EnsureCreated();

                var databaseSeeder = new DataSeeder(context);
                if (!context.ToDos.Any())
                {
                    databaseSeeder.SeedTodos();
                }
              
            }
        }

        public static void CreateIdentityDatabase(this IApplicationBuilder application)
        {
            using (var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<WebApiAuthUserContext>();

                context.Database.EnsureCreated();
            }
        }
    }
}
