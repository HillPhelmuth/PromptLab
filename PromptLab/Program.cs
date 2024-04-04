using Microsoft.Extensions.DependencyInjection;
using PromptLab.Core.Services;

namespace PromptLab;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Startup.Init();
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
