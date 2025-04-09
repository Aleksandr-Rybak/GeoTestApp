using GeoApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoApp.Controllers;

public class HomeController : Controller
{
    private readonly ICsvHelperService _csvHelperService;

    public HomeController(ICsvHelperService csvHelperService)
    {
        _csvHelperService = csvHelperService ?? throw new ArgumentNullException(nameof(csvHelperService));
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadData(IFormFile csvFile)
    {
        _csvHelperService.ImportDataFromCsv(csvFile.OpenReadStream());
        return Ok();
    }
}
