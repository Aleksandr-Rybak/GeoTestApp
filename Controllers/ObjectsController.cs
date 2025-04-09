using Microsoft.AspNetCore.Mvc;

namespace GeoApp.Controllers
{
    public class ObjectsController : Controller
    {
        private readonly DbHelperService _dbHelperService;

        public ObjectsController(DbHelperService dbHelperService)
        {
            _dbHelperService = dbHelperService;
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
