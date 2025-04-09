using GeoApp.Models;
using NetTopologySuite.Geometries;

namespace GeoApp.Services
{
    public interface IDbHelperService
    {
        void SaveObject(Guid id, string number, int type, Geometry geometry);

        IEnumerable<GeoObject> GetAllObjects();

        List<Guid> GetRelatedObjects(Guid id);
    }
}
