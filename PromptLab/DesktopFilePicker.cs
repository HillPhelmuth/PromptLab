using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab
{
	public class DesktopFilePicker
	{
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
				Filter = "txt files (*.txt)|*.txt|markdown files (*.md)|*.md|All files (*.*)|*.*",
				FilterIndex = 2,
				RestoreDirectory = true,
				//Multiselect = multiSelect
			};


			return fileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fileDialog.FileName) ? fileDialog.FileName : string.Empty;
		}
		public static string OpenSaveFile(string filename, string fileText)
		{
			var saveFileDialog = new SaveFileDialog
			{
				FileName = filename,
				Filter = "txt files (*.txt)|*.txt|markdown files (*.md)|*.md|All files (*.*)|*.*",
				FilterIndex = 2,
				RestoreDirectory = true
			};
			var result = saveFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				File.WriteAllText(saveFileDialog.FileName, fileText);
				return saveFileDialog.FileName;
			}
			return string.Empty;

		}
	}
}
