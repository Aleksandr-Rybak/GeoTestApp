using GeoApp.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;

namespace GeoApp
{
    public class DbHelperService
    {
        private readonly string _connectionString;

        public DbHelperService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DbConnection");
        }

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
                SaveObject(id, number, type, geometry);
            }
        }

        private void SaveObject(Guid id, string number, int type, Geometry geometry)
        {
            var relatedIds = new List<Guid>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var writer = new NetTopologySuite.IO.WKTWriter();
                string wGeometry = writer.Write(geometry);
                // Insert into objects table
                using (var command =
                       new NpgsqlCommand(
                           "INSERT INTO objects (id, number, type, geodata) VALUES (@id, @number, @type, ST_GeomFromText(@geo, 4326))",
                           connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("number", number);
                    command.Parameters.AddWithValue("type", type);
                    command.Parameters.AddWithValue("@geo", wGeometry);
                    command.ExecuteNonQuery();
                }

                // Check for intersections with other objects
                relatedIds = CheckAndInsertRelations(connection, id, wGeometry);

            }

            InsertRelation(id, relatedIds);
        }

        private List<Guid> CheckAndInsertRelations(NpgsqlConnection connection, Guid currentObjectId, string geoJson)
        {
            var relatedIds = new List<Guid>();
            using (var command = new NpgsqlCommand(
                       @"SELECT id FROM objects WHERE  ST_Intersects(geodata, ST_GeomFromText(@geo, 4326))
                                            AND id != @currentObjectId", connection))
                {
                    command.Parameters.AddWithValue("geo", geoJson);
                    command.Parameters.AddWithValue("currentObjectId", currentObjectId);

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        relatedIds.Add(reader.GetGuid(0));
                    }
                }

                return relatedIds;
        }

        private void InsertRelation(Guid objectId, List<Guid> relatedObjectIds)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    foreach (var relatedObjectId in relatedObjectIds)
                    {
                        using (var command =
                               new NpgsqlCommand(
                                   "INSERT INTO object_relations (object_id, related_object_id) VALUES (@objectId, @relatedObjectId)",
                                   connection))
                        {
                            command.Parameters.AddWithValue("objectId", objectId);
                            command.Parameters.AddWithValue("relatedObjectId", relatedObjectId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public IEnumerable<GeoObject> GetAllObjects()
        {
            var objects = new List<GeoObject>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT id, number, type, ST_AsGeoJSON(geodata) FROM objects";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var obj = new GeoObject
                            {
                                Id = reader.GetGuid(0),
                                Number = reader.GetString(1),
                                Type = reader.GetInt32(2),
                                GeoData = reader.GetString(3)
                            };
                            objects.Add(obj);
                        }
                    }
                }
            }
            return objects;
        }

        public List<Guid> GetRelatedObjects(Guid id)
        {
            var relatedObjectIds = new List<Guid>(); // Список связанных объектов

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(@"
                SELECT related_object_id  FROM object_relations 
                WHERE object_id = @id 
                union
                SELECT object_id  FROM object_relations 
                WHERE related_object_id = @id", connection);
                command.Parameters.AddWithValue("id", id);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    relatedObjectIds.Add(reader.GetGuid(0));
                }
            }
            return relatedObjectIds;
        }
    }
}
