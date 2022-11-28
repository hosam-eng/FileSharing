using System.ComponentModel.DataAnnotations;
using Fileesharing.Resources;
namespace Fileesharing.Models
{
	public class LoginViewModel
	{
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
		[EmailAddress(ErrorMessageResourceName = "Email", ErrorMessageResourceType = typeof(SharedResource))]
		[Display(Name = "EmailLabel", ResourceType = typeof(SharedResource))]
		public string Email { get; set; }
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
		[Display(Name = "PasswordLabel", ResourceType = typeof(SharedResource))]
		public string Password { get; set; }
	}
	public class RegisterViewModel
	{
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
		[EmailAddress(ErrorMessageResourceName = "Email", ErrorMessageResourceType = typeof(SharedResource))]
		[Display(Name = "EmailLabel", ResourceType = typeof(SharedResource))]
		public string Email { get; set; }
		[Required]
		public string FirstName { get; set; }
		[Required]
		public string LastName { get; set; }
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
		[Display(Name = "PasswordLabel", ResourceType = typeof(SharedResource))]
		public string Password { get; set; }
		[Compare("Password")]
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
		[Display(Name = "ConfirmPasswordLabel", ResourceType = typeof(SharedResource))]
		public string ConfirmPassword { get; set; }

	}

	public class ChangePasswordViewModel
	{
		[Required]
		public string CurrentPassword { get; set; }
		[Required]
		public string NewPassword { get; set; }
		[Required]
		[Compare("NewPassword")]
		public string ConfirmNewPassword { get; set; }
	}
	public class AddPasswordViewModel
	{
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}

	public class ConfirmEmailViewModel
	{
		[Required]
		public string Token { get; set; }
		[Required]
		public string UserId { get; set; }
	}
	public class ForgotPasswordViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
	public class VerifyResetPasswordViewModel 
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		public string token { get; set; }

	}
	public class ResetPasswordViewModel 
	{
		[Required]
		public string NewPassword { get; set; }
		[Required]
		[Compare("NewPassword")]
		public string ConfirmPassword { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		public string token { get; set; }
	}
}

