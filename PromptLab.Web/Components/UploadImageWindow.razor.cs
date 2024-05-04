using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Radzen;
using PromptLab.Core.Helpers;
using PromptLab.Core.Models;

namespace PromptLab.Web.Components;

public partial class UploadImageWindow
{
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    private FileUpload _fileUpload = new();
    private int maxFileSize = 1024 * 1024 * 500;
    private void Submit(FileUpload fileUpload)
    {
        if (FileHelper.TryConvertFromBase64String(fileUpload.FileBase64, out var bytes))
        {
            fileUpload.FileBytes = bytes;
        }
        DialogService.Close(fileUpload);
    }
}