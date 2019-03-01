using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace noSQLReader
{
    class Program
    {
        private static readonly string endpointUrl = "https://iot18cosmosdb1.documents.azure.com:443/";
        private static readonly string authorizationKey = "hOIRiGHfqwAvH9S4NbJ5afsWd9KCJHARBCoRz6s8HZYVkame0iildXLTpRMqzH3RHWjWMgdIV97yLKJABIAIcQ==";
        private static readonly string databaseId = "iotmessages";
        private static readonly string collectionId = "BigData";
        private static readonly string SqlConnectionString = @"Server=tcp:iot18sqlserver1.database.windows.net,1433;Initial Catalog=IoT18SQLdb;Persist Security Info=False;User ID=gustafeden;Password=Doiylv12;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private static readonly ConnectionPolicy connectionPolicy = new ConnectionPolicy { UserAgentSuffix = " samples-net/2" };

        private static DocumentClient client;
        private static Database database;
        static void Main(string[] args)
        {
            try
            {
                //Instantiate a new DocumentClient instance
                using (client = new DocumentClient(new Uri(endpointUrl), authorizationKey, connectionPolicy))
                {
                    //Get, or Create, a reference to Database
                    database = GetOrCreateDatabaseAsync(databaseId).Result;

                    //Do operations on Collections
                    RunCollectionDemo().Wait();
                }
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
            
        }

        private static async Task RunCollectionDemo()
        {
            var DeviceId = 0;
            var LocationId = 0;
            var DevicetypeId = 0;
            var StudentId = 0;

            var StudentSql = "IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [Name] = @name) INSERT INTO [Students] ([Name]) output INSERTED.StudentId VALUES(@name) ELSE SELECT StudentId FROM [Students] WHERE [Name] = @name";
            var LocationSql = "IF NOT EXISTS (SELECT 1 FROM [Locations] WHERE [Latitude] = @latitude AND [Longitude] = @longitude) INSERT INTO [Locations] ([Longitude], [Latitude]) output INSERTED.LocationId VALUES(@longitude, @latitude) ELSE SELECT LocationId FROM [Locations] WHERE [Latitude] = @latitude AND [Longitude] = @longitude";
            var DeviceTypeSql = "IF NOT EXISTS (SELECT 1 FROM [DeviceTypes] WHERE [Type] = @type) INSERT INTO [DeviceTypes] ([Type]) output INSERTED.DeviceTypeId VALUES(@type) ELSE SELECT DeviceTypeId FROM [DeviceTypes] WHERE [Type] = @type";
            var DeviceSql = "IF NOT EXISTS (SELECT 1 FROM [Devices] WHERE [MacAdress] = @macadress) INSERT INTO [Devices] ([LocationId], [DeviceTypeId], [StudentId], [MacAdress]) output INSERTED.DeviceId VALUES(@locationid, @devicetypeid, @studentid, @macadress) ELSE SELECT DeviceId FROM [Devices] WHERE [MacAdress] = @macadress";
            var MessageSql = "INSERT INTO [Messages] ([DeviceId], [Temperature], [Humidity], [Created]) VALUES(@deviceid, @temperature, @humidity, @created)";
            var counterall = 0;
            var counterreal = 0;
           
            DocumentCollection collection = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));
            // SQL -- Id == "value" OR City == "value"
            var q = client.CreateDocumentQuery(collection.SelfLink,
                "SELECT * from c" );
            using (SqlConnection conn = new SqlConnection(SqlConnectionString))
            {
                conn.Open();
                foreach (var item in q.ToList())
                {
                    counterall++;
                    var json = JsonConvert.DeserializeObject(item.ToString());
                    var mac = json["deviceid"];
                    var type = json["type"];
                    var student = json["student"];
                    var position = json["position"];
                    var dht = json["dht"];
                    var created = json["_ts"];

                    var obj = JsonConvert.DeserializeObject<DeviceMessage>(item.ToString());
                    Console.WriteLine("{0}", obj.Student);
                    if (obj.DeviceId == null || obj.Type == null || obj.Student == null || obj.Position[0] == null || obj.Position[1] == null || obj.Dht[0] == null || obj.Dht[1] == null)
                        continue;
                    int intValue;
                    float floatValue;
                    if (!Int32.TryParse(obj.Dht[0].ToString(), out intValue) && !float.TryParse(obj.Dht[1].ToString(), out floatValue))
                        continue;
                    /* if(mac == null || type == null || student == null || position[0] == null || position[1] == null || dht[0] == null || dht[1] == null || created == null)
                         continue;
                     if (mac.ToString().Length < 5 || type.ToString().Length < 3 || student.ToString().Length < 3 || position[0].ToString().Length < 4)
                         continue;
                     if (json["Student"] == "FelixEdenborgh")
                         continue;*/
                    Console.WriteLine("{0} {1}", obj.Dht[0], obj.Dht[1]);
                    LocationId = InsertLocationSql(conn, LocationSql, obj.Position[1].ToString(), obj.Position[0].ToString());
                    DevicetypeId = InsertTypeSql(conn, DeviceTypeSql, (string)obj.Type);
                    StudentId = InsertStudentSql(conn, StudentSql, (string)obj.Student);
                    DeviceId = InsertDeviceSql(conn, DeviceSql, (string)obj.DeviceId, LocationId, DevicetypeId, StudentId);
                    InsertMessagesSql(conn, MessageSql, obj.Dht[0], obj.Dht[1], DeviceId, ConvertUnixEpochTime((long)obj.Created));
                    /*
                    LocationId = InsertLocationSql(conn, LocationSql, (string)position[1], (string)position[0]);
                    DevicetypeId = InsertTypeSql(conn, DeviceTypeSql, type.ToString());
                    StudentId = InsertStudentSql(conn, StudentSql, (string)student);
                    DeviceId = InsertDeviceSql(conn, DeviceSql, mac.ToString(), LocationId, DevicetypeId, StudentId);
                    InsertMessagesSql(conn, MessageSql, (string)dht[0], (string)dht[1], DeviceId, ConvertUnixEpochTime((long)created));
                   */ counterreal++;
                    Console.Clear();
                    Console.WriteLine("{0} {1}", counterall, counterreal);
                    // break;
                    //if (counterreal > 10)
                    //    break;
                }
                conn.Close();
            }
           
        }
        private static int InsertLocationSql(SqlConnection conn, string sqlstring, string lon, string lat)
        {
            var returnint = 0;
           
                using (SqlCommand cmd = new SqlCommand(sqlstring, conn))
                {
                    cmd.Parameters.AddWithValue("@longitude", lon);
                    cmd.Parameters.AddWithValue("@latitude", lat);
                    returnint = (int)cmd.ExecuteScalar();
                    Console.WriteLine("ID: {0}", returnint);
                }
           
            return returnint;
        }
        private static int InsertTypeSql(SqlConnection conn, string sqlstring, string type)
        {
            var returnint = 0;

            using (SqlCommand cmd = new SqlCommand(sqlstring, conn))
            {
                cmd.Parameters.AddWithValue("@type", type);
                returnint = (int)cmd.ExecuteScalar();
                Console.WriteLine("ID: {0}", returnint);
            }

            return returnint;
        }
        private static int InsertStudentSql(SqlConnection conn, string sqlstring, string student)
        {
           var returnint = 0;

            using (SqlCommand cmd = new SqlCommand(sqlstring, conn))
            {
                cmd.Parameters.AddWithValue("@name", student);
                returnint = (int)cmd.ExecuteScalar();
                Console.WriteLine("ID: {0}", returnint);
                
            }

            return returnint;
        }
        private static int InsertDeviceSql(SqlConnection conn, string sqlstring, string macadress, int locid, int devid, int studid)
        {
           var returnint = 0;

            using (SqlCommand cmd = new SqlCommand(sqlstring, conn))
            {
                cmd.Parameters.AddWithValue("@macadress", macadress);
                cmd.Parameters.AddWithValue("@studentid", studid);
                cmd.Parameters.AddWithValue("@devicetypeid", devid);
                cmd.Parameters.AddWithValue("@locationid", locid);
                returnint = (int)cmd.ExecuteScalar();
                Console.WriteLine("ID: {0}", returnint);
            }

            return returnint;
        }
        private static void InsertMessagesSql(SqlConnection conn, string sqlstring, float temp, float humid, int devid, DateTime date)
        {
            using (SqlCommand cmd = new SqlCommand(sqlstring, conn))
            {
                cmd.Parameters.AddWithValue("@deviceid", devid);
                cmd.Parameters.AddWithValue("@temperature", temp);
                cmd.Parameters.AddWithValue("@humidity", humid);
                cmd.Parameters.AddWithValue("@created", date);
                cmd.ExecuteScalar();
            }
            
        }
        private static async Task<Database> GetOrCreateDatabaseAsync(string id)
        {
            IEnumerable<Database> query = from db in client.CreateDatabaseQuery()
                                          where db.Id == id
                                          select db;
            
            Database database = query.FirstOrDefault();

            return database;
        }
        private static DateTime ConvertUnixEpochTime(long seconds)

        {

            DateTime Fecha = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return Fecha.ToLocalTime().AddSeconds(seconds);

        }
        public class DeviceMessage
        {
            [JsonProperty("deviceid")]
            public string DeviceId { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("student")]
            public string Student { get; set; }
            [JsonProperty("position")]
            public string[] Position { get; set; }
            [JsonProperty("dht")]
            public float[] Dht { get; set; }
            [JsonProperty("_ts")]
            public long Created { get; set; }
        }
    }
}
