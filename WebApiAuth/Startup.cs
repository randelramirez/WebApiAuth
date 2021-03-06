using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApiAuth.Models;
using WebApiAuth.Services;

namespace WebApiAuth
{
    public class Startup
    {
        const string APPLY_AUTHORIZE_WITH_JwtBearerDefaults_AuthenticationScheme = nameof(APPLY_AUTHORIZE_WITH_JwtBearerDefaults_AuthenticationScheme);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtOptions>(Configuration.GetSection("Tokens"));

            services.AddScoped<IUserService, UserService>();

            #region Option 1: Add Authorize Attribute/Filter globally with Auth Scheme as JWT
            //services.AddControllers(options =>
            //{
            //    //Add [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] globally
            //    options.Filters.Add(new AuthorizeFilter(APPLY_AUTHORIZE_WITH_JwtBearerDefaults_AuthenticationScheme));
            //});
            #endregion

            services.AddControllers();

            services.AddDbContextPool<DataContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString(nameof(DataContext)));
                //options.UseSqlServer(connection, b => b.MigrationsAssembly("Project.Api"))
            });

            services.AddDbContextPool<WebApiAuthUserContext>(options =>
            {
                var connectionString = "Data Source=localhost; Database=WebApiAuthIdentity; User Id=sa;Password=randel1_23;";
                options.UseSqlServer(connectionString);
            });

            services.AddIdentity<WebApiAuthUser, IdentityRole>()
                .AddEntityFrameworkStores<WebApiAuthUserContext>();

            // perhaps move the next 2 lines of code in a local method? We need them only to bind jwtsettings.json to be strongly typed here in ConfigureServices
            var jwtOptions = new JwtOptions();
            Configuration.GetSection("Tokens").Bind(jwtOptions);
            services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {

                    // This specifies which parameters will be used to validate the token
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
                    };
                });

            services.AddAuthorization(options =>
            {
                // Set up to add [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] globally
                // We add it on the middleware (middleware all requests vs filters(attribute) controller or action level only)
                options.AddPolicy(APPLY_AUTHORIZE_WITH_JwtBearerDefaults_AuthenticationScheme, b =>
                {
                    b.RequireAuthenticatedUser();
                    b.AuthenticationSchemes = new List<string> { JwtBearerDefaults.AuthenticationScheme };
                });
            });


            // for api/users/authentication2
            //services.AddAuthentication()
            // .AddJwtBearer(options =>
            //  {
            //      options.Events = new JwtBearerEvents
            //      {
            //          OnTokenValidated = context =>
            //          {
            //              var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<WebApiAuthUser>>();
            //              var userId = context.Principal.Identity.Name;
            //              var user = userManager.FindByIdAsync(userId);
            //              if (user == null)
            //              {
            //                  // return unauthorized if user no longer exists
            //                  context.Fail("Unauthorized");
            //              }
            //              return Task.CompletedTask;
            //          }
            //      };

            //      options.RequireHttpsMetadata = false;
            //      options.SaveToken = true;
            //      options.TokenValidationParameters = new TokenValidationParameters
            //      {
            //          ValidateIssuerSigningKey = true,
            //          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"])),
            //          ValidateIssuer = false,
            //          ValidateAudience = false
            //      };
            //  });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.SeedDataContext();
                app.CreateIdentityDatabase();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                #region  Option 2: add to controllers pipeline
                    .RequireAuthorization(APPLY_AUTHORIZE_WITH_JwtBearerDefaults_AuthenticationScheme);
                #endregion
            });
        }
    }
}
