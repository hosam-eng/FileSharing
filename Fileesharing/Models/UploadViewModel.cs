using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Fileesharing.Models
{
	public class InputFile
	{
		[Required]
		public IFormFile File { get; set; }
	}

	public class InputUpload
	{
		public string OriginalName { set; get; }
		public string FileName { set; get; }
		public long size { set; get; }
		public string ContentType { set; get; }
		public string UserId { set; get; }
	}

	public class UploadViewModel
	{
		public string Id { get; set; }
		public string OriginalName { get; set; }
		public string FileName { set; get; }
		public decimal Size { set; get; }
		public string ContentType { set; get; }
		public DateTime UploadDate { get; set; }
		public long DownloadCount { get; set; }

	}
}
