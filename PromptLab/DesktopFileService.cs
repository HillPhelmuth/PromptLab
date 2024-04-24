﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PromptLab.Core.Models;

namespace PromptLab;

public class DesktopFileService
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
	public static void SaveUserSettings(UserProfile userProfile, string userDataFolder)
	{
		var json = JsonSerializer.Serialize(userProfile);
		if (!Directory.Exists(userDataFolder))
			Directory.CreateDirectory(userDataFolder);
		File.WriteAllText(Path.Combine(userDataFolder, "userSettings.json"), json);
	}
	public static UserProfile LoadUserSettings(string userDataFolder)
	{
		if (!Directory.Exists(userDataFolder))
			Directory.CreateDirectory(userDataFolder);
		var path = Path.Combine(userDataFolder, "userSettings.json");
		if (!File.Exists(path)) return new UserProfile();
		var json = File.ReadAllText(path);
		return JsonSerializer.Deserialize<UserProfile>(json) ?? new UserProfile();
	}
}