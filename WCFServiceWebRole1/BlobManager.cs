using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole1
{
    public class BlobManager
    {
        public CloudStorageAccount storageAccount;
        public CloudBlobClient blobClient;
        public CloudBlobContainer container;


        public BlobManager(){
            try
            {
                storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));

                blobClient = storageAccount.CreateCloudBlobClient();
                container = blobClient.GetContainerReference("supstorage");
                container.CreateIfNotExists();
            }
            catch (ArgumentNullException)
            {
                Trace.TraceInformation("CloudStorageAccount Exception null ou vide");
                // Use Application Local Storage Account String
            }
            catch (NullReferenceException)
            {
                Trace.TraceInformation("CloudBlobClient Or CloudBlobContainer Exception");
                // Create Container 
            }
            catch (FormatException)
            {
                Trace.TraceInformation("CloudStorageAccount Exception Connection String Invalid");
            }
            catch (ArgumentException)
            {
                Trace.TraceInformation("CloudStorageAccount Exception connectionString ne peut pas être analysée");
            }
        }
    }
}