using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CosmosDbToBlob
{
    class Program
    {
        private static string accName = "iot18storageblob";
        private static string accKey = "Tr0vMtuIDKbUZM8tt5ewedsEB63nHY726fSRWma7cQ76pwtKTDD9hAkHSzRmc47Y5trBlVxr+toYsaBzwN+7uA==";

        static void Main(string[] args)
        {
            StorageCredentials creds = new StorageCredentials(accName, accKey);
            CloudStorageAccount strAcc = new CloudStorageAccount(creds, true);
            
            CloudBlobClient blobClient = strAcc.CreateCloudBlobClient();
  
            CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
 
            container.CreateIfNotExistsAsync();

            DateTime dateLogEntry = DateTime.Now; 
            CloudAppendBlob appBlob = container.GetAppendBlobReference("iotmessages.csv");
            CloudBlockBlob blockblob = container.GetBlockBlobReference("blockmessages.csv");
            blockblob.OpenWrite();
            //appBlob.CreateOrReplace();

            

            // Add the entry to file.  

            //appBlob.AppendText
            //(
            //    string.Format(
            //        "{0} | Message for blob!!!\r\n",
            //        dateLogEntry.ToString()));

            // Now lets pull down our file and see what it looks like.  
          //  Console.WriteLine(appBlob.DownloadText());
            var response = appBlob.DownloadTextAsync();
            //Console.WriteLine(response.Result.ToString());
            byte[] byteArray = Encoding.UTF8.GetBytes(response.Result.ToString());
            MemoryStream stream = new MemoryStream(byteArray);
            blockblob.UploadFromStreamAsync(stream);
          //  Console.WriteLine(blockblob.DownloadTextAsync().Result.ToString());
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}


/* FROM AZURE FUNCTIIONS AS BACKUPP
 using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

private static string accName = "iot18storageblob";
private static string accKey = "Tr0vMtuIDKbUZM8tt5ewedsEB63nHY726fSRWma7cQ76pwtKTDD9hAkHSzRmc47Y5trBlVxr+toYsaBzwN+7uA==";

public static void Run(string input, TraceWriter log, IEnumerable<dynamic> inputDocument, TextWriter outputBlob)
{
    //log.Info($"C# manually triggered function called with input: {input}");
    
    StorageCredentials creds = new StorageCredentials(accName, accKey);
    CloudStorageAccount strAcc = new CloudStorageAccount(creds, true);
            
    CloudBlobClient blobClient = strAcc.CreateCloudBlobClient();
  
    CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
 
    container.CreateIfNotExistsAsync();

    DateTime dateLogEntry = DateTime.Now; 
    CloudAppendBlob appBlob = container.GetAppendBlobReference("iotmessages.csv");
            
   // appBlob.CreateOrReplace();  
   //  appBlob.AppendText
   //     (
   //     string.Format(
   //     "deviceid, type, student, latitude, longitude, temperature, humidity, created\r\n"
   //     )
   //     );
    outputBlob.WriteLine("deviceid, type, student, latitude, longitude, temperature, humidity, created\r\n");
    foreach(var doc in inputDocument)
    {
        var outstring = string.Format(
        "{0},{1},{2},{3},{4},{5},{6},{7}",
        doc.deviceid, doc.type, doc.student, doc.position[0], doc.position[1], doc.dht[0],doc.dht[1], doc._ts
        );

        outputBlob.WriteLine(outstring);

       // appBlob.AppendText
       // (
       // string.Format(
       // "{0},{1},{2},{3},{4},{5},{6},{7}\r\n",
       // doc.deviceid, doc.type, doc.student, doc.position[0], doc.position[1], doc.dht[0],doc.dht[1], doc._ts
       // )
       // );
    }
    log.Info("C# manually triggered function called with all messages from cosmosDB");
}
 
     
     
     
     
     
     */
