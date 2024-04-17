using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Services
{
    public class WebFileService : IFileService
    {
        
        public Task<string?> OpenFileAsync(string fileName = "")
        {
            throw new NotImplementedException();
        }

        public Task<string?> SaveFileAsync(string fileName, string file)
        {
            throw new NotImplementedException();
        }

        public event Action? PickFile;
        public event Action<string, string>? SaveFile;
    }
}
