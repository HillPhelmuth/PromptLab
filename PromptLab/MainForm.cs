using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using PromptLab.Core.Services;
using Serilog;

namespace PromptLab;

public partial class MainForm : Form
{
    private readonly LocalFileService _filePickerService;
    private WebView2 _webView;
    private string UserDataFolder { get; set; } /*=> Path.Combine(_webView.CoreWebView2.Environment.UserDataFolder, "PromptLab");*/
    public MainForm()
    {
        _filePickerService = new LocalFileService();
        _filePickerService.PickFile += () =>
        {
            var filePath = DesktopFileService.OpenFileDialog();
            _filePickerService.FilePicked(filePath);
        };
        _filePickerService.SaveUserProfile += profile =>
        {
	        DesktopFileService.SaveUserSettings(profile, UserDataFolder);
        };
        _filePickerService.LoadUserProfile += () => DesktopFileService.LoadUserSettings(UserDataFolder);
        //_filePickerService.PickFolder += () =>
        //{
        //    var folderPath = DesktopFilePicker.OpenFolderDialog();
        //    _filePickerService.FolderPicked(folderPath);
        //};
        _filePickerService.SaveFile += (fileName, fileText) =>
        {
			var filePath = DesktopFileService.OpenSaveFile(fileName, fileText);
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
        //var env = _webView.CoreWebView2.Environment.UserDataFolder;
        _webView.CoreWebView2InitializationCompleted += (sender, e) =>
        {
			UserDataFolder = Path.Combine(_webView.CoreWebView2.Environment.UserDataFolder, "PromptLab");
		};
        //UserDataFolder = Path.Combine(_webView.CoreWebView2.Environment.UserDataFolder, "PromptLab");
        Startup.ServiceCollection!.AddSingleton<IFileService>(_filePickerService);
        blazor.Services = Startup.ServiceCollection!.BuildServiceProvider();
        var logger = blazor.Services.GetRequiredService<ILoggerFactory>();
        logger.AddFile("Logs\\AppLogs.txt");
        blazor.RootComponents.Add<Main>("#app");
        Controls.Add(blazor);
		this.FormClosing += MainForm_FormClosing;
        this.Icon = new Icon(@"Resources\PromptLabLogo.ico");
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
