using AutoMapper;
using Fileesharing.Data;
using Fileesharing.Models;
using Microsoft.AspNetCore.Identity;

namespace Fileesharing
{
	public class UploadProfile:Profile
	{
		public UploadProfile()
		{
			CreateMap<InputUpload, Uploads>()
				.ForMember(u => u.UploadDate, op=>op.Ignore())
				.ForMember(u => u.id, op => op.Ignore());
			CreateMap<Uploads, UploadViewModel>();
		}
	}
	public class UserProfile : Profile
	{ 
		public UserProfile()
		{
			CreateMap<ApplicationUser, UserViewModel>()
				.ForMember(u=>u.HasPassword,op=>op.MapFrom(u=>u.PasswordHash !=null));
		}
	}
}
