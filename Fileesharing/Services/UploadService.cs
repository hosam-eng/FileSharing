using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fileesharing.Data;
using Fileesharing.Models;
using Microsoft.EntityFrameworkCore;

namespace Fileesharing.Services
{
	public class UploadService : IUploadService
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public UploadService(ApplicationDbContext context,IMapper mapper)
		{
			_db = context;
			_mapper = mapper;
		}
		public async Task CreateAsync(InputUpload model)
		{
			var mappedObj = _mapper.Map<Uploads>(model);
		    await _db.Uploads.AddAsync(mappedObj);
			await _db.SaveChangesAsync();
		}

		public async  Task DeleteAsync(string id, string userId)
		{
			var selectedFile = await _db.Uploads.FirstOrDefaultAsync(u => u.id == id && u.UserId == userId);
			if (selectedFile != null)
			{
				_db.Remove(selectedFile);
				await _db.SaveChangesAsync();
			}
		}

		public async Task<Uploads> FindAsync(string id, string userId)
		{
			var selectedFile = await _db.Uploads.FirstOrDefaultAsync(u=>u.id==id && u.UserId==userId);
			if (selectedFile != null)
			{
			  return selectedFile;
			}
			return null;
		}
		public async Task<Uploads> FindAsync(string id)
		{
			var selectedFile = await _db.Uploads.FindAsync(id);
			if (selectedFile != null)
			{
				return selectedFile;
			}
			return null;
		}

		public IQueryable<UploadViewModel> getAll()
		{
			var result=_db.Uploads
				.OrderByDescending(u => u.DownloadCount)
				.ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
			return result;
		}

		public IQueryable<UploadViewModel> getBy(string userid)
		{
			var result =_db.Uploads.Where(u => u.UserId == userid)
				.OrderByDescending(u => u.DownloadCount)
				.ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
			return result;
		}

		public async Task IncreamentDownloadCount(string id)
		{
			var selectedFile = await _db.Uploads.FindAsync(id);
			if (selectedFile != null)
			{
				selectedFile.DownloadCount++;
				_db.Update(selectedFile);
				await _db.SaveChangesAsync();

			}
		}
		public async Task<int> GetUploadsCount()
		{
		   return await _db.Uploads.CountAsync();
		}

		public IQueryable<UploadViewModel> Search(string term)
		{
			var result=_db.Uploads
			   .Where(u => u.OriginalName.Contains(term))
			   .OrderByDescending(u => u.DownloadCount)
			   .ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
			return result;
		}
	}

}
