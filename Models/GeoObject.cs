using NetTopologySuite.Geometries;

namespace GeoApp.Models
{
    public class GeoObject
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public int Type { get; set; }
        public string GeoData { get; set; }
    }
}
