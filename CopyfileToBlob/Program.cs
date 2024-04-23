using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobUploader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the file name as a parameter.");
                return;
            }

            string fileName = args[0];

            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not found.");
                return;
            }

            string connectionString = "your_blob_storage_connection_string";
            string containerName = "Input";

            await UploadFileToBlobStorage(connectionString, containerName, fileName);
        }

        static async Task UploadFileToBlobStorage(string connectionString, string containerName, string fileName)
        {
            try
            {
                // Create a BlobServiceClient object which will be used to create a container client
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                // Create the container if it doesn't exist
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                // Get a reference to a blob
                BlobClient blobClient = containerClient.GetBlobClient(Path.GetFileName(fileName));

                // Open the file and upload its data to Azure Blob Storage
                using (FileStream fileStream = File.OpenRead(fileName))
                {
                    await blobClient.UploadAsync(fileStream, true);
                    Console.WriteLine($"File {fileName} uploaded successfully to Azure Blob Storage.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
