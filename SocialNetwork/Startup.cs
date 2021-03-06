using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocialNetwork.Models.Users;
using SocialNetwork.AdditionalClaims;
using SocialNetwork.Classes.Hubs;
using Microsoft.EntityFrameworkCore.Proxies;
using SocialNetwork.Classes.Users.Timeline;

namespace SocialNetwork
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Setting up the database context.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"))
                    .UseLazyLoadingProxies());

            // Setting the default identity model.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            // Setting up the additional claims class for the identity user.
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
                AdditionalUserClaimsPrincipalFactory>();

            // Setting up additional classes
            services.AddScoped<TimelineService>();
            services.AddScoped<Timeline>();

            services.AddControllersWithViews();
            services.AddRazorPages();

            // We want to include SignalR for realtime chat and interaction.
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();

                /*
                 * Hubs
                 */
                endpoints.MapHub<GlobalHub>("/global");
            });
        }
    }
}
