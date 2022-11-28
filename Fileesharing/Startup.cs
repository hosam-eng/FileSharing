using Fileesharing.Data;
using Fileesharing.Helpers.Mail;
using Fileesharing.Models;
using Fileesharing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fileesharing
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
			services.AddControllersWithViews()
				.AddViewLocalization(op =>
				{
					op.ResourcesPath = "Resources";
				});
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
			});

			services.AddIdentity<ApplicationUser, IdentityRole>(Options =>
			{
				Options.SignIn.RequireConfirmedEmail = true;
			})
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			services.Configure<DataProtectionTokenProviderOptions>(Options =>
			{
				Options.TokenLifespan = TimeSpan.FromHours(3);
			});

			services.AddLocalization();

			services.AddTransient<IMailHelper, MailHelper>();
			services.AddTransient<IUploadService, UploadService>();
			services.AddAutoMapper(typeof(Startup));


			services.AddAuthentication()
				.AddFacebook(Options =>
				{
					Options.AppId = Configuration["Authentication:Facebook:AppId"];
					Options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
				})
				.AddGoogle(Options =>
				{
					Options.ClientId = Configuration["Authentication:Google:ClientId"];
					Options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}
			app.UseStaticFiles();

			app.UseCookiePolicy(new CookiePolicyOptions()
			{
				MinimumSameSitePolicy = SameSiteMode.Lax
			});

			app.UseAuthentication();
			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseAuthorization();


			var supportedCultures = new[] { "ar-SA", "en-US" };
			app.UseRequestLocalization(n =>
			{
				n.AddSupportedUICultures(supportedCultures);
				n.AddSupportedCultures(supportedCultures);
				n.SetDefaultCulture("en-US");

			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
