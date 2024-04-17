using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.WinForms;
using PromptLab.Core.Services;
using Serilog;

namespace PromptLab;

public partial class MainForm : Form
{
    private readonly LocalFileService _filePickerService;
    private WebView2 _webView;
    public MainForm()
    {
        _filePickerService = new LocalFileService();
        _filePickerService.PickFile += () =>
        {
            var filePath = DesktopFilePicker.OpenFileDialog();
            _filePickerService.FilePicked(filePath);
        };
        //_filePickerService.PickFolder += () =>
        //{
        //    var folderPath = DesktopFilePicker.OpenFolderDialog();
        //    _filePickerService.FolderPicked(folderPath);
        //};
        _filePickerService.SaveFile += (fileName, fileText) =>
        {
			var filePath = DesktopFilePicker.OpenSaveFile(fileName, fileText);
			_filePickerService.FilePicked(filePath);
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
        Startup.ServiceCollection!.AddSingleton<IFileService>(_filePickerService);
        blazor.Services = Startup.ServiceCollection!.BuildServiceProvider();
        var logger = blazor.Services.GetRequiredService<ILoggerFactory>();
        logger.AddFile("Logs\\AppLogs.txt");
        blazor.RootComponents.Add<Main>("#app");
        Controls.Add(blazor);
		this.FormClosing += MainForm_FormClosing;
    }

	private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
	{
		Log.CloseAndFlush();
	}

	public void AdjustZoom(double zoomFactor)
    {
        _webView.ZoomFactor = zoomFactor;
    }
    

}
