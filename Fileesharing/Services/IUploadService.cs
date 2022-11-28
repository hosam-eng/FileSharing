using System.Linq;
using System.Threading.Tasks;
using Fileesharing.Data;
using Fileesharing.Models;

namespace Fileesharing.Services
{
	public interface IUploadService
	{
		IQueryable<UploadViewModel> getBy(string userid);
		IQueryable<UploadViewModel> getAll();
		IQueryable<UploadViewModel> Search(string term);
		Task CreateAsync(InputUpload model);
		Task<Uploads> FindAsync(string id,string userId);
		Task<Uploads> FindAsync(string id);
		Task DeleteAsync(string id, string userId);
		Task IncreamentDownloadCount(string id);
		Task<int> GetUploadsCount();
	}
}
