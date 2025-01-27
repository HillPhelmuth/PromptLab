using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorJoditEditor;

public class JoditEditorOptions
{
    [JsonPropertyName("autofocus")]
    public bool Autofocus { get; set; } = true;

    [JsonPropertyName("spellcheck")]
    public bool Spellcheck { get; set; } = true;

    [JsonPropertyName("toolbarButtonSize")]
    public string ToolbarButtonSize { get; set; } = "xsmall";

    [JsonPropertyName("defaultMode")]
    public string DefaultMode { get; set; } = "1";

    [JsonPropertyName("toolbarInlineForSelection")]
    public bool ToolbarInlineForSelection { get; set; } = true;

    [JsonPropertyName("showPlaceholder")]
    public bool ShowPlaceholder { get; set; } = false;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 650;

    [JsonPropertyName("toolbarAdaptive")]
    public bool ToolbarAdaptive { get; set; } = false;

    [JsonPropertyName("toolbarStickyOffset")]
    public int ToolbarStickyOffset { get; set; } = 10;

    [JsonPropertyName("buttons")]
    public string Buttons { get; set; } = "bold,italic,underline,ul,ol,paragraph,|,undo,redo,|,spellcheck,|,cut,copy,paste,selectall,|,table,symbols,indent,outdent";

    public JsonElement AsJavascriptObject()
    {
        var optionsJson = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        Console.WriteLine($"Options as Json:\n{optionsJson}");
        return JsonSerializer.Deserialize<JsonElement>(optionsJson);
    }
}
