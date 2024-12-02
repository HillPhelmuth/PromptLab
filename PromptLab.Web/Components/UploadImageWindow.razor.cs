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

   
    private MultiFileUpload _multiFileUpload = new(){FileUploads = [new FileUpload()]};
    private void Submit(MultiFileUpload fileUploads)
    {
        foreach (var fileUpload in fileUploads.FileUploads)
        {
            if (FileHelper.TryConvertFromBase64String(fileUpload.FileBase64!, out var bytes))
            {
                fileUpload.FileBytes = bytes;
            }
        }
        
        DialogService.Close(fileUploads);
    }
    private void Add()
    {
        if (_multiFileUpload.FileUploads.Count < 5)
            _multiFileUpload.FileUploads.Add(new FileUpload());
    }
    private void Remove(FileUpload fileUpload)
    {
        if (_multiFileUpload.FileUploads.Count > 1)
            _multiFileUpload.FileUploads.Remove(fileUpload);
    }
}