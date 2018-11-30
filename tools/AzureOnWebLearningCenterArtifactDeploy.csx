#r "Microsoft.WindowsAzure.Storage"

using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

public static async Task Run(CloudBlockBlob myBlob, string name, ILogger log)
{
    log.LogInformation($"C# Blob trigger function Processed blob\n Name:");
    var client = myBlob.ServiceClient;
    var directory = client.GetContainerReference("mentorbotlearningcenter").GetDirectoryReference("site");
    var tempPath = Path.Combine(Path.GetTempPath(), "site");
    var tempArchivePath = Path.Combine(Path.GetTempPath(), name);

    log.LogInformation($"Container created.");

    if (File.Exists(tempArchivePath))
    {
        File.Delete(tempArchivePath);
    }

    await myBlob.DownloadToFileAsync(tempArchivePath, FileMode.CreateNew);

    if (Directory.Exists(tempPath))
    {
        Directory.Delete(tempPath, true);
    }

    log.LogInformation($"Extracting {tempArchivePath} to {tempPath}");

    ZipFile.ExtractToDirectory(tempArchivePath, tempPath);

    log.LogInformation($"Site extracted, uploading site");

    var angularFolder = Path.Combine(tempPath, "ClientApp", "dist");

    await CleanFolder(directory, log);

    await UploadFolder(angularFolder, directory, log);

    await myBlob.DeleteAsync();
}

private static async Task UploadFolder(string path, CloudBlobDirectory root, ILogger log)
{
    var files = Directory.GetFiles(path);
    foreach (var file in files)
    {
        var name = Path.GetFileName(file);
        var ext = Path.GetExtension(name);
        log.LogInformation($"Upload file to azure {name}: {file}");
        var blob = root.GetBlockBlobReference(name);
        if (extensions.ContainsKey(ext))
        {
            var contentType = extensions[ext];
            log.LogInformation($"Set content type {contentType}.");
            blob.Properties.ContentType = contentType;
        }

        await blob.UploadFromFileAsync(file);
    }
}

private static async Task CleanFolder(CloudBlobDirectory root, ILogger log)
{
    var result = await root.ListBlobsSegmentedAsync(useFlatBlobListing: true, blobListingDetails: BlobListingDetails.None, maxResults: null, currentToken:null, options: null, operationContext: null);
    foreach (var item in result.Results)
    {
        if (item is CloudBlob blob)
        {
            log.LogInformation($"Delete blob {blob.Name}.");
            await blob.DeleteAsync();
        }
    }
}

private static readonly Dictionary<string, string> extensions = new Dictionary<string, string>
{
    { ".html", "text/html" },
    { ".js", "application/javascript" },
    { ".css", "text/csst" },
    { ".json", "application/json" },
    { ".txt", "text/plain" },
    { ".woff2", "font/woff2" },
    { ".svg", "image/svg+xml" },
    { ".ttf", "font/ttf" },
    { ".eot", "application/vnd.ms-fontobject" },
    { ".woff", "font/woff" }
};
