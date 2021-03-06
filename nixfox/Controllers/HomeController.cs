﻿using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using nixfox.App;
using Volt.Http;

namespace nixfox.Controllers
{
	public class URLResponse
	{
		public string url { get; set; }
		public string status { get; set; }
		public string token { get; set; }
	}
	public class HomeController : Controller
	{
		

		[HttpGet, Route("/")]
		public IActionResult Index() {
			return View();
		}

		[HttpPost, Route("/")]
		public IActionResult PostURL([FromBody] string url) {
			try {
				if (!url.Contains("http")) {
					url = "http://" + url;
				}
				if (new LiteDB.LiteDatabase("Data/Urls.db").GetCollection<NixURL>().Exists(u=> u.ShortenedURL == url)) {
					Response.StatusCode = 405;
					return Json(new URLResponse(){url = url, status = "already shortened", token = null});
				}
				Shortener shortURL = new Shortener(url);
				return Json(shortURL.Token);
			}catch(Exception ex) {
				if (ex.Message == "URL already exists") {
					Response.StatusCode = 400;
					return Json(new URLResponse() { url = url, status = "URL already exists", token = new LiteDB.LiteDatabase("Data/Urls.db").GetCollection<NixURL>().Find(u=>u.URL == url).FirstOrDefault().Token });
				}
				throw new Exception(ex.Message);
			}
			return StatusCode(500);
		}

		[HttpGet, Route("/{token}")]
		public IActionResult NixRedirect([FromRoute] string token) {
			return Redirect(new LiteDB.LiteDatabase("Data/Urls.db").GetCollection<NixURL>().FindOne(u => u.Token == token).URL);
		}
		
		private string FindRedirect(string url){
			string result = string.Empty;
			using (var client = new HttpClient())
			{
				var response = client.GetAsync(url).Result;
				if (response.IsSuccessStatusCode)
				{
					result = response.Headers.Location.ToString();
				}
			}
			return result;
		}

	}
}
