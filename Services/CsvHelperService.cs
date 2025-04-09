using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GeoApp.Services
{
    public class CsvHelperService(IDbHelperService dbHelperService): ICsvHelperService
    {
        private readonly IDbHelperService _dbHelperService = dbHelperService;

        public void ImportDataFromCsv(string csvFilePath)
        {
            var csvLines = File.ReadAllLines(csvFilePath);
            foreach (var line in csvLines)
            {
                var columns = line.Split('\t');
                var id = Guid.Parse(columns[0]);
                var number = columns[1];
                var type = int.Parse(columns[2]);
                var geoJson = columns[3]?.Replace("\"\"", "\"").Replace("'", "");
                var reader = new GeoJsonReader();
                Geometry geometry = reader.Read<Geometry>(geoJson);
                _dbHelperService.SaveObject(id, number, type, geometry);
            }
        }
    }
}
