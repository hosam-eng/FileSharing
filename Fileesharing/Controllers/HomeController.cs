using Fileesharing.Helpers.Mail;
using Fileesharing.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fileesharing.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _db;
		private readonly IMailHelper _mailHelper;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext contecxt, IMailHelper mailHelper)
		{
			_logger = logger;
			_db = contecxt;
			_mailHelper = mailHelper;
		}

		public IActionResult Index()
		{
			var HeighestDownload = _db.Uploads.OrderByDescending(u => u.DownloadCount)
				.Select(u => new UploadViewModel
				{
					Id = u.id,
					OriginalName = u.OriginalName,
					FileName = u.FileName,
					Size = u.size,
					ContentType = u.ContentType,
					UploadDate = u.UploadDate,
					DownloadCount = u.DownloadCount
				}).Take(3);
			ViewBag.Popular = HeighestDownload;
			return View();
		}

		private string userId
		{
			get
			{
				return User.FindFirstValue(ClaimTypes.NameIdentifier);
			}
		}

		public IActionResult Privacy()
		{
			return View();
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpGet]
		public IActionResult Contact()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Contact(ContactUsViewModel model)
		{
			if (ModelState.IsValid)
			{
				await _db.Contacts.AddAsync(new Data.Contact
				{
					Name = model.Name,
					Email = model.Email,
					Subject = model.Subject,
					Message = model.Message,
					UserId = userId
				});
				await _db.SaveChangesAsync();
				TempData["Message"] = "Message has been send successfuly!.";

				StringBuilder sb = new StringBuilder();
				sb.AppendLine("<h1>you have unread message</h1>");
				sb.AppendFormat("Name : {0}", model.Name);
				sb.AppendFormat("Email : {0}", model.Email);
				sb.AppendLine();
				sb.AppendFormat("Subject : {0}", model.Subject);
				sb.AppendFormat("Message : {0}", model.Message);
				_mailHelper.SendMail(new InputEmailMessage
				{
					Subject = "You have - unread message",
					Email = "Microsoft@gmail.com"

				});

				return RedirectToAction("Contact");
			}
			return View(model);
		}
		[HttpGet]
		public IActionResult About()
		{
			return View();
		}

		[HttpGet]
		public IActionResult setCulture(string lang,string returnUrl= null)
		{
			if (!string.IsNullOrEmpty(lang))
			{
				Response.Cookies.Append(
					 CookieRequestCultureProvider.DefaultCookieName,
					 CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
					 new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
					);
			}
			if (!string.IsNullOrEmpty(returnUrl))
			{
				return LocalRedirect(returnUrl);
			}
			return RedirectToAction("Index");
		}

	}
}
