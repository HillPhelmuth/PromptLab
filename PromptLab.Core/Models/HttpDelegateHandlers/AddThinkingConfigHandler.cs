using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;

namespace PromptLab.Core.Models.HttpDelegateHandlers;

public class AddThinkingConfigHandler(HttpMessageHandler innerHandler, StringEventWriter output) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private readonly TextWriter _output = output;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Only handle if there is content and it's JSON.
        request.RequestUri = new Uri(request.RequestUri.ToString().Replace("v1beta", "v1alpha"));
       _output.WriteLine(request.RequestUri?.ToString());
       var headers = request.Headers.ToList();
       _output.WriteLine(JsonSerializer.Serialize(headers, s_jsonSerializerOptions));
        if (request.Content is not { Headers.ContentType.MediaType: "application/json" })
            return await base.SendAsync(request, cancellationToken);
        var originalBody = await request.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(originalBody)) return await base.SendAsync(request, cancellationToken);
        try
        {
            // Parse the existing JSON.
            var root = JsonNode.Parse(originalBody);
            if (root is not null)
            {
                // Root should be a JsonObject to add a new property.
                var generationConfigNode = root["generationConfig"];
                if (generationConfigNode is not JsonObject generationConfig)
                {
                    generationConfig = new JsonObject();
                    root["generationConfig"] = generationConfig;
                }

                // Add or overwrite the 'thinking_config' property within 'generationConfig'.
                generationConfig["thinking_config"] = new JsonObject
                {
                    ["include_thoughts"] = true
                };

                // Re-serialize and replace the request content.
                var modifiedJson = root.ToJsonString(new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                this._output.WriteLine("=== Modified Request ===");
                this._output.WriteLine(modifiedJson);
                this._output.WriteLine(string.Empty);
                //request.Content = new StringContent(modifiedJson, Encoding.UTF8, "application/json");
            }
        }
        catch
        {
            // If parsing fails, do nothing – just fall through
            // and send the original request body.
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.Content is not null)
        {
            // Log the response details
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _output.WriteLine("=== RESPONSE ===");
            _output.WriteLine(responseContent);
            _output.WriteLine(string.Empty);
        }

        return response;
    }
}