using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using Fileesharing.Data;
using Fileesharing.Models;
using Fileesharing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting.Internal;

namespace Fileesharing.Controllers
{
	[Authorize]
	public class UploadsController : Controller
	{
		private readonly IUploadService uploadService;
		private readonly IMapper _mapper;

		public IWebHostEnvironment Env { get; }

		public UploadsController(IUploadService uploadService, IWebHostEnvironment env,IMapper mapper)
		{
			this.uploadService = uploadService;
			Env = env;
			_mapper = mapper;
		}
		[HttpGet]
		public IActionResult Index()
		{ 
			var result = uploadService.getBy(userId);
			return View(result);
		}

		private string userId
		{
			get
			{
				return User.FindFirstValue(ClaimTypes.NameIdentifier);
			}
		}
		public async Task<List<UploadViewModel>> GetPagedData(IQueryable<UploadViewModel> result,int RequiredPage = 1, string term = "")
		{
			const int pageSize = 3;

			decimal RowsCount =await uploadService.GetUploadsCount();
			var PagesCount = Math.Ceiling(RowsCount / pageSize);

			RequiredPage = RequiredPage <= 0 ? 1 : RequiredPage;
			RequiredPage = RequiredPage > PagesCount ? 1 : RequiredPage;

			var SkipCount = (RequiredPage - 1) * pageSize;

			var PagedData = await result
				.Skip(SkipCount)
				.Take(pageSize)
				.ToListAsync();
			ViewBag.CurrentPage = RequiredPage;
			ViewBag.word = term;
			return PagedData;
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Result(string term,int RequiredPage)
		{
			var result = uploadService.Search(term);
			var model = await GetPagedData(result,RequiredPage, term);
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ActionName("Result")]
		public async Task<IActionResult> firstSearch(string term, int RequiredPage = 1)
		{
			var result = uploadService.Search(term);
			ViewBag.word = term;
			var model = await GetPagedData(result,RequiredPage, term);
			return View(model);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Browse(int RequiredPage=1)
		{
		  var result = uploadService.getAll();
		  var model= await GetPagedData(result, RequiredPage);
		  return View(model);
		}


		[HttpGet]
		public async Task<IActionResult> Download(string id)
		{
			var selectedFile = await uploadService.FindAsync(id);
			if (selectedFile == null)
			{
				return NotFound();
			}
			await uploadService.IncreamentDownloadCount(id);
			var path = "~/Uploads/" + selectedFile.FileName;
			Response.Headers.Add("Expires", DateTime.Now.AddDays(-3).ToLongDateString());
			Response.Headers.Add("Cache-Control", "no-cache");
			return File(path, selectedFile.ContentType, selectedFile.OriginalName);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(InputFile model)
		{
			if (ModelState.IsValid)
			{
				var NewName = Guid.NewGuid().ToString();
				var extention = Path.GetExtension(model.File.FileName);
				var fileName = string.Concat(NewName, extention);
				var root = Env.WebRootPath;
				var path = Path.Combine(root, "Uploads", fileName);

				using (var fs = System.IO.File.Create(path))
				{
					await model.File.CopyToAsync(fs);
				}

				await uploadService.CreateAsync(new InputUpload
				{
					OriginalName = model.File.FileName,
					FileName = fileName,
					size = model.File.Length,
					ContentType = model.File.ContentType,
					UserId = userId
				});
				return RedirectToAction("Index");
			}
			return View(model);

		}

		[HttpGet]
		public async Task<IActionResult> Delete(string id)
		{
			var selectedfile = await uploadService.FindAsync(id, userId);
			if (selectedfile == null)
			{
				return NotFound();
			}
			else
			{
				return View(selectedfile);
			}
		}
		[HttpPost]
		[ActionName("Delete")]
		public async Task<IActionResult> ConfirmationDelete(string id)
		{
			var selectedfile = await uploadService.FindAsync(id, userId);
			if (selectedfile == null)
			{
				return NotFound();
			}
			await uploadService.DeleteAsync(id,userId);
			return RedirectToAction("Index");
		}
	}
}

