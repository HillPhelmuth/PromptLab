namespace PromptLab.Core.Models;

public class AppSettings
{
	public double ZoomFactor { get; set; } = 1.0;
	public StyleTheme Theme { get; set; }
	public bool ShowTimestamps { get; set; }
	public bool AutoCompleteView { get; set; }
	public int AutoCompleteLength { get; set; }

}