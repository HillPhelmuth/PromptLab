using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PromptLab.Core.Helpers;

public static class FileHelper
{
    public static T ExtractFromAssembly<T>(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var jsonName = assembly.GetManifestResourceNames()
            .SingleOrDefault(s => s.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) ?? "";
        using var stream = assembly.GetManifestResourceStream(jsonName);
        using var reader = new StreamReader(stream);
        object result = reader.ReadToEnd();
        if (typeof(T) == typeof(string))
            return (T)result;
        return JsonSerializer.Deserialize<T>(result.ToString());
    }
    public static MemoryStream ToMemoryStream(this byte[] byteArray) => new(byteArray);

    public static bool TryConvertFromBase64String(string input, out byte[] output)
    {
        output = Enumerable.Empty<byte>().ToArray();

        // Check if the string is null or empty
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        // Attempt to remove the data URL prefix if present
        var base64StartIndex = input.IndexOf("base64,", StringComparison.Ordinal);
        if (base64StartIndex != -1)
        {
            input = input[(base64StartIndex + 7)..]; // Skip past "base64," to start of actual base64 data
        }

        // Replace any whitespace and check the length
        input = input.Trim();
        if (input.Length % 4 != 0)
        {
            return false;
        }

        // Check for valid Base64 characters
        if (!Regex.IsMatch(input, @"^[a-zA-Z0-9\+/]*={0,2}$"))
        {
            return false;
        }

        try
        {
            output = Convert.FromBase64String(input);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}