using System;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Controllers;

namespace UmbracoDevelopmentProject.Controller
{
	[Route("api/[controller]")]
	[ApiController]
	public class ApiController : UmbracoApiController
	{
		private readonly ConfigImporter _configImporter;

		public ApiController(ConfigImporter configImporter)
		{
			_configImporter = configImporter ?? throw new ArgumentNullException(nameof(configImporter));
		}

		[HttpPost("ImportXml")]
		public IActionResult ImportXml()
		{
			try
			{
				// Call the ImportXml method of ConfigImporter
				_configImporter.ImportXml("C:/Users/amalgovind.ps/source/repos/ContentType/FirstPage.config");

				// Return a success response
				return Ok("XML import successful!");
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error importing XML: {ex}");

				// Return an error response with status code 500
				return StatusCode(500, $"Error importing XML: {ex.Message}");
			}
		}
	}
}