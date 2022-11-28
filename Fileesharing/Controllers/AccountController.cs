using AutoMapper;
using Fileesharing.Data;
using Fileesharing.Helpers.Mail;
using Fileesharing.Models;
using Fileesharing.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Common;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Fileesharing.Controllers
{
	public class AccountController : Controller
	{
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly IMapper _mapper;
		private readonly IStringLocalizer<SharedResource> stringLocalizer;
		private readonly IMailHelper mailHelper;

		public AccountController(SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IMapper mapper,
			IStringLocalizer<SharedResource> stringLocalizer,
			IMailHelper mailHelper)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			_mapper = mapper;
			this.stringLocalizer = stringLocalizer;
			this.mailHelper = mailHelper;
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, true, true);
				if (result.Succeeded)
				{
					if (!string.IsNullOrEmpty(returnUrl))
					{
						return LocalRedirect(returnUrl);
					}
					return RedirectToAction("Create", "Uploads");
				}
				else if (result.IsNotAllowed)
				{
					TempData["Error"] = stringLocalizer["ConfirmationEmailMessage"]?.Value;
				}
			}
			return View();
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					Email = model.Email,
					UserName = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName

				};
				var result = await userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					//create link
					var token =await userManager.GenerateEmailConfirmationTokenAsync(user);
					var url = Url.Action("ConfirmEmail", "Account", new {token=token,userid=user.Id},Request.Scheme);
					//send email
					StringBuilder body= new StringBuilder();
					body.AppendLine("Filesharing Application :Email Confrimation");
					body.AppendFormat("to confirm your email :you should <a href='{0}'>click here</a>",url);
					mailHelper.SendMail(new InputEmailMessage
					{
						Body=body.ToString(),
						Email=model.Email,
						Subject="Email Confirmation"
					});
					return RedirectToAction("RequiredEmailConfirmation");
					/*await signInManager.SignInAsync(user, false);
					return RedirectToAction("Create", "Uploads");*/
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View();
		}

		public IActionResult RequiredEmailConfirmation()
		{
			return View();
		}

		public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByIdAsync(model.UserId);
				if (user != null)
				{
					if (!user.EmailConfirmed)
					{
						var result = await userManager.ConfirmEmailAsync(user, model.Token);
						if (result.Succeeded)
						{
							TempData["Success"] = "your Email confirmed successfully";
							return RedirectToAction("Login");
						}
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}
					}
					else
					{
						TempData["Success"] = "your Email already confirmed";
						return RedirectToAction("Login");
					}
				}
			}
			    return View();
		}
		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		public IActionResult ExternalLogin(string Provider)
		{
				var properties = signInManager.ConfigureExternalAuthenticationProperties(Provider, "/Account/ExternalResponse");
				return Challenge(properties, Provider);
		}
		public async Task<IActionResult> ExternalResponse()
		{
			var info = await signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				TempData["ErrorMessage"] = "Login Faild";
				return RedirectToAction("Login");
			}
			var LoginResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
			if (!LoginResult.Succeeded)
			{
				var email = info.Principal.FindFirstValue(ClaimTypes.Email);
				var FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
				var LastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
				var CreateUserAccount = new ApplicationUser
				{
					Email = email,
					UserName = email,
					FirstName = FirstName,
					LastName = LastName,
					EmailConfirmed = true
				};
				var CreateResult = await userManager.CreateAsync(CreateUserAccount);
				if (CreateResult.Succeeded)
				{
					var exLoginResult = await userManager.AddLoginAsync(CreateUserAccount, info);
					if (exLoginResult.Succeeded)
					{
						await signInManager.SignInAsync(CreateUserAccount, false, info.LoginProvider);
						return RedirectToAction("Index", "Home");
					}
					else
					{
						await userManager.DeleteAsync(CreateUserAccount);
					}
				}
				return RedirectToAction("Login");
			}
			return RedirectToAction("Index", "Home");
		}
		public async Task<IActionResult> Info()
		{
			var CurrentUser = await userManager.GetUserAsync(User);
			if (CurrentUser != null)
			{
				var model = _mapper.Map<UserViewModel>(CurrentUser);
				return View(model);
			}
			return NotFound();

		}
		[HttpPost]
		public async Task<IActionResult> Info(UserViewModel model)
		{
			var CurrentUser = await userManager.GetUserAsync(User);
			if (CurrentUser != null)
			{
				CurrentUser.FirstName = model.FirstName;
				CurrentUser.LastName = model.LastName;

				var UpdateResult = await userManager.UpdateAsync(CurrentUser);
				if (UpdateResult.Succeeded)
				{
					TempData["Success"] = stringLocalizer["SuccessMessage"]?.Value;
					return RedirectToAction("Info");
				}
				foreach (var error in UpdateResult.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
				return View(model);
			}
			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			var CurrentUser = await userManager.GetUserAsync(User);
			if (CurrentUser != null)
			{
				if (ModelState.IsValid)
				{

					var UpdateResult = await userManager.ChangePasswordAsync(CurrentUser, model.CurrentPassword, model.NewPassword);
					if (UpdateResult.Succeeded)
					{
						TempData["Success"] = stringLocalizer["ChangePasswordSuccess"]?.Value;
						await signInManager.SignOutAsync();
						return RedirectToAction("Login");
					}
					foreach (var error in UpdateResult.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}
			else
			{
				return NotFound();
			}

			return View("Info", _mapper.Map<UserViewModel>(CurrentUser));
		}

		[HttpPost]
		public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
		{
			var CurrentUser = await userManager.GetUserAsync(User);
			if (CurrentUser != null)
			{
				if (ModelState.IsValid)
				{

					var UpdateResult = await userManager.AddPasswordAsync(CurrentUser, model.Password);
					if (UpdateResult.Succeeded)
					{
						TempData["Success"] = stringLocalizer["AddPasswordSuccess"]?.Value;
						return RedirectToAction("Info");
					}
					foreach (var error in UpdateResult.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}
			else
			{
				return NotFound();
			}

			return View("Info", _mapper.Map<UserViewModel>(CurrentUser));
		}
		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var existUser =await userManager.FindByEmailAsync(model.Email);
				if (existUser!= null)
				{
					var token =await userManager.GeneratePasswordResetTokenAsync(existUser);
					var url = Url.Action("ResetPassword", "Account", new {token,model.Email},Request.Scheme);

					StringBuilder body = new StringBuilder();
					body.AppendLine("Filesharing Application :Forget Password");
					body.AppendFormat("to reset your password :you should <a href='{0}'>click here</a>", url);
					mailHelper.SendMail(new InputEmailMessage
					{
						Body = body.ToString(),
						Email = model.Email,
						Subject = "Reset Password"
					});
				}
				TempData["Success"] = "You should recieve email";

			}
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> ResetPassword(VerifyResetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var existUser = await userManager.FindByEmailAsync(model.Email);
				if (existUser != null)
				{
					var isvaild = await userManager.VerifyUserTokenAsync(existUser, TokenOptions.DefaultProvider, "ResetPassword", model.token);
					if (isvaild)
					{
						return View(new ResetPasswordViewModel { Email=model.Email,token=model.token});
					}
					else
					{
						TempData["Error"] = "token is invalid";
					}
				}
			}
			return RedirectToAction("Login");
		}
		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var existUser = await userManager.FindByEmailAsync(model.Email);
				if (existUser != null)
				{
					var result = await userManager.ResetPasswordAsync(existUser, model.token,model.NewPassword);
					if (result.Succeeded)
					{
						TempData["Success"] = "Reset Password complitly successfully";
						return RedirectToAction("Login");
					}
					else
					{
						foreach(var error in result.Errors)
						{
							ModelState.AddModelError("",error.Description);
						}
					}
				}
			}
			return View(model);
		}
	}
}

