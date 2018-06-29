#r "Microsoft.WindowsAzure.Storage"

using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;

public static async Task Run(CloudBlockBlob myBlob, string name, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{name}");
    var client = myBlob.ServiceClient;
    var directory = client.GetContainerReference("mentorbotlearningcenter").GetDirectoryReference("site");
    var tempPath = Path.Combine(Path.GetTempPath(), "site");
    var tempArchivePath = Path.Combine(Path.GetTempPath(), name);

    log.Info($"Containe created.");

    if (File.Exists(tempArchivePath))
    {
        File.Delete(tempArchivePath);
    }

    await myBlob.DownloadToFileAsync(tempArchivePath, FileMode.CreateNew);

    if (Directory.Exists(tempPath))
    {
        Directory.Delete(tempPath, true);
    }

    log.Info($"Extracting {tempArchivePath} to {tempPath}");

    ZipFile.ExtractToDirectory(tempArchivePath, tempPath);

    log.Info($"Site extracted, uploading site");

    var angularFolder = Path.Combine(tempPath, "ClientApp", "dist");

    await UploadFolder(angularFolder, directory, log);

    await myBlob.DeleteAsync();
}

private static async Task UploadFolder(string path, CloudBlobDirectory root, TraceWriter log)
{
    var files = Directory.GetFiles(path);
    foreach (var file in files)
    {
        var name = Path.GetFileName(file);
        var ext = Path.GetExtension(name);
        log.Info($"Upload file to azure {name}: {file}");
        var blob = root.GetBlockBlobReference(name);
        if (extensions.ContainsKey(ext))
        {
            var contentType = extensions[ext];
            log.Info($"Set content type {contentType}.");
            blob.Properties.ContentType = contentType;
        }

        await blob.UploadFromFileAsync(file);
    }
}

private static readonly Dictionary<string, string> extensions = new Dictionary<string, string>
{
    { ".html", "text/html" },
    { ".js", "application/javascript" },
    { ".css", "text/csst" },
    { ".json", "application/json" },
    { ".txt", "text/plain" }
};