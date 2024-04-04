using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.WinForms;
using PromptLab.Core.Services;

namespace PromptLab;

public partial class MainForm : Form
{
    private readonly FilePickerService _filePickerService;
    private WebView2 _webView;
    public MainForm()
    {
        _filePickerService = new FilePickerService();
        _filePickerService.PickFile += () =>
        {
            var filePath = OpenFileDialog();
            _filePickerService.FilePicked(filePath);
        };
        _filePickerService.PickFolder += () =>
        {
            var folderPath = OpenFolderDialog();
            _filePickerService.FolderPicked(folderPath);
        };
        _filePickerService.Zoom += AdjustZoom;
        InitializeComponent();

        var blazor = new BlazorWebView()
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html",
            //Services = Startup.Services!,
            StartPath = "/"
        };
        _webView = blazor.WebView;
        Startup.ServiceCollection!.AddSingleton(_filePickerService);
        blazor.Services = Startup.ServiceCollection!.BuildServiceProvider();
        blazor.RootComponents.Add<Main>("#app");
        Controls.Add(blazor);
    }
    public void AdjustZoom(double zoomFactor)
    {
        _webView.ZoomFactor = zoomFactor;
    }
    public static string OpenFolderDialog()
    {
        using var folderDialog = new FolderBrowserDialog();
        var result = folderDialog.ShowDialog();

        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
        {
            return folderDialog.SelectedPath;
        }

        return string.Empty;
    }
    public static string OpenFileDialog()
    {

        var fileDialog = new OpenFileDialog
        {
            Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*", 
            FilterIndex = 2, 
            RestoreDirectory = true,
            //Multiselect = multiSelect
        };


        return fileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fileDialog.FileName) ? fileDialog.FileName : string.Empty;
    }

}
