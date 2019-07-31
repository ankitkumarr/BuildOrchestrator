using DurableBuildOFunctionApp.Contexts;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableBuildOFunctionApp.Common
{
    public static class StorageHelper
    {
        public async static Task UploadArtifactToStorage(BlobUploadContext uploadContext)
        {
            string containerName = $"{uploadContext.Worker}-{uploadContext.Platform}-{uploadContext.Environment}";
            string blobName = $"results-{Guid.NewGuid().ToString()}.zip";

            string artifactUrl = uploadContext?.Artifact?.Resource?.DownloadUrl
                ?? throw new ArgumentNullException("Could not retrieve the download Url from the context.");

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(new Uri(artifactUrl)))
                using (Stream downloadStream = await response.Content.ReadAsStreamAsync())
                {
                    await UploadStreamToStorage(blobName, containerName, downloadStream);
                }
            }
        }

        public async static Task UploadStreamToStorage(string blobName, string containerName, Stream package)
        {
            CloudBlobContainer blobContainer = null;
            try
            {
                var storageConnection = EnvironmentHelper.GetStorageConnectionString();
                var storageAccount = CloudStorageAccount.Parse(storageConnection);
                var blobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = blobClient.GetContainerReference(containerName);
                await blobContainer.CreateIfNotExistsAsync();
            }
            catch (Exception)
            {
                // Sometimes storage client's exceptions are unhelpful
                Console.WriteLine($"Error creating a Blob container reference. " +
                    $"Please make sure your connection string in {Constants.EnvVars.UploadStorageConString} is valid");
                throw;
            }
            var blob = blobContainer.GetBlockBlobReference(blobName);
            await blob.UploadFromStreamAsync(package);
        }
    }
}
