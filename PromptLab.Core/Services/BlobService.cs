using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace PromptLab.Core.Services;

public class BlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobService> _logger;
    private const string CdnUrl = "https://promptlabimages.z19.web.core.windows.net";
    public BlobService(BlobServiceClient blobServiceClient, ILoggerFactory loggerFactory)
    {
        _blobServiceClient = blobServiceClient;
        _logger = loggerFactory.CreateLogger<BlobService>();
    }
    public async Task<string> UploadBlobFile(string fileName, byte[] fileBytes)
    {
        BlobContainerClient? container = _blobServiceClient.GetBlobContainerClient("$web");
        BlobClient? client = container.GetBlobClient(fileName);
        
        var result = await client.UploadAsync(new MemoryStream(fileBytes), options:new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "image/png" }
        });
        return $"{CdnUrl}/{fileName}";
    }
}