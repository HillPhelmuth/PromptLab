using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PromptLab.Core.Helpers;

public class JsonExtractionHelper
{
	private const string LiteralDelimiter = "```";
	private const string JsonPrefix = "json";

	/// <summary>
	/// Utility method for extracting a JSON result from an agent response.
	/// </summary>
	/// <param name="result">A text result</param>
	/// <typeparam name="TResult">The target type of the <see cref="FunctionResult"/>.</typeparam>
	/// <returns>The JSON translated to the requested type.</returns>
	public static TResult? Translate<TResult>(string result)
	{
		var rawJson = ExtractJson(result);
		try
		{
			return JsonSerializer.Deserialize<TResult>(rawJson);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error translating JSON: {ex.Message}");
			return default;
		}
	}

	private static string ExtractJson(string result)
	{
		// Search for initial literal delimiter: ```
		int startIndex = result.IndexOf(LiteralDelimiter, StringComparison.Ordinal);
		if (startIndex < 0)
		{
			// No initial delimiter, return entire expression.
			return result;
		}

		startIndex += LiteralDelimiter.Length;

		// Accommodate "json" prefix, if present.
		if (JsonPrefix.Equals(result.Substring(startIndex, JsonPrefix.Length), StringComparison.OrdinalIgnoreCase))
		{
			startIndex += JsonPrefix.Length;
		}

		// Locate final literal delimiter
		int endIndex = result.IndexOf(LiteralDelimiter, startIndex, StringComparison.OrdinalIgnoreCase);
		if (endIndex < 0)
		{
			endIndex = result.Length;
		}

		// Extract JSON
		return result.Substring(startIndex, endIndex - startIndex);
	}
}