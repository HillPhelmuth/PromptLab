using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using PromptLab.Core.Services;
using Serilog;
using System.Diagnostics;

namespace PromptLab;

public partial class MainForm : Form
{
    private readonly LocalFileService _filePickerService;
    private WebView2 _webView;
    private string UserDataFolder { get; set; } /*=> Path.Combine(_webView.CoreWebView2.Environment.UserDataFolder, "PromptLab");*/
    public MainForm()
    {
        _filePickerService = new LocalFileService();
        _filePickerService.PickFile += filterString =>
        {
            var filePath = DesktopFileService.OpenFileDialog(filterString);
            _filePickerService.FilePicked(filePath);
        };
        _filePickerService.SaveUserProfile += profile =>
        {
            DesktopFileService.SaveUserSettings(profile, UserDataFolder);
        };
        _filePickerService.LoadUserProfile += () => DesktopFileService.LoadUserSettings(UserDataFolder);
        _filePickerService.PickImageFile += () =>
        {
            var files = DesktopFileService.OpenImageFileDialog();
            _filePickerService.ImagePicked(files);
        };
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
        blazor.BlazorWebViewInitialized += Bwv_Initialized;
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
        FormClosing += MainForm_FormClosing;
        Icon = new Icon(@"Resources\PromptLabLogo.ico");
    }
    private void Bwv_Initialized(object? sender, BlazorWebViewInitializedEventArgs? e)
    {
        e.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        e.WebView.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
    }

    private void CoreWebView2_ContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
    {
        Debug.WriteLine($"IsEditable? {e.ContextMenuTarget.IsEditable}");

        if (e.ContextMenuTarget.IsEditable)
        {
            // For editable elements such as <input> and <textarea> we enable the context menu but remove items we don't want in this app
            var itemNamesToRemove = new[] { "share", "webSelect", "webCapture" };
            var menuIndexesToRemove =
                e.MenuItems
                    .Select((m, i) => (m, i))
                    .Where(m => itemNamesToRemove.Contains(m.m.Name))
                    .Select(m => m.i)
                    .Reverse();

            Debug.WriteLine($"Removing these indexes: {string.Join(", ", menuIndexesToRemove.Select(i => i.ToString()))}");
            foreach (var menuIndexToRemove in menuIndexesToRemove)
            {
                Debug.WriteLine($"Removing {e.MenuItems[menuIndexToRemove].Name}...");
                e.MenuItems.RemoveAt(menuIndexToRemove);
            }

            // Trim extra separators from the end
            while (e.MenuItems.Last().Kind == CoreWebView2ContextMenuItemKind.Separator)
            {
                e.MenuItems.RemoveAt(e.MenuItems.Count - 1);
            }
        }
        else
        {
            // For non-editable elements such as <div> we disable the context menu
            e.Handled = true;
        }
    }
    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        Log.CloseAndFlush();
    }

    public void AdjustZoom(double zoomFactor)
    {
        _webView.ZoomFactor = zoomFactor;
    }

    private void RefreshButtonClick(object sender, EventArgs e)
    {
        _webView.Refresh();
        _webView.Reload();
        Log.Logger.Information("Refreshed WebView");
    }
}
