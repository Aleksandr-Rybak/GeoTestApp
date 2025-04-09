using GeoApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoApp.Controllers
{
    public class ObjectsController : Controller
    {
        private readonly IDbHelperService _dbHelperService;

        public ObjectsController(IDbHelperService dbHelperService)
        {
            _dbHelperService = dbHelperService ?? throw new ArgumentNullException(nameof(dbHelperService));
        }

        [HttpGet("/api/objects")]
        public async Task<IActionResult> GetObjects()
        {
            var objects = _dbHelperService.GetAllObjects().ToList();
            return Json(objects);
        }

        [HttpGet("/api/objects/{id:guid}/related")]
        public async Task<IActionResult> GetRelatedGeoObjects(Guid id)
        {
            var objects = _dbHelperService.GetRelatedObjects(id).ToList();
            return Json(objects);
        }
    }
}
