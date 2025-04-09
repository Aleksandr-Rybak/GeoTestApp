using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text;

namespace GeoApp.Services
{
    public class CsvHelperService(IDbHelperService dbHelperService): ICsvHelperService
    {
        private readonly IDbHelperService _dbHelperService = dbHelperService;

        public void ImportDataFromCsv(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Обработка строки CSV
                    var columns = line.Split('\t');
                    var id = Guid.Parse(columns[0]);
                    var number = columns[1];
                    var type = int.Parse(columns[2]);
                    var geoJson = columns[3]?.Replace("\"\"", "\"").Replace("'", "");
                    var readerGeo = new GeoJsonReader();
                    Geometry geometry = readerGeo.Read<Geometry>(geoJson);
                    _dbHelperService.SaveObject(id, number, type, geometry);
                }
            }
        }
    }
}
