using Microsoft.AspNetCore.Mvc;

namespace GeoApp.Controllers;

public class HomeController : Controller
{
    private readonly DbHelperService _dbHelperService;

    public HomeController(DbHelperService dbHelperService)
    {
        _dbHelperService = dbHelperService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadData(IFormFile csvFile)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", csvFile.FileName);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            csvFile.CopyTo(stream);
        }

        _dbHelperService.ImportDataFromCsv(path);

        return Ok();
    }
}
