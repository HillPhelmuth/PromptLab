using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PromptLab.Core.Models.HttpDelegateHandlers;

public class DeepseekReasoningContentHandler(HttpMessageHandler innerHandler, StringWriter output) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private readonly TextWriter _output = output;
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            this._output.WriteLine("=== REQUEST ===");
            try
            {
                string formattedContent = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(content), s_jsonSerializerOptions);
                this._output.WriteLine(formattedContent);
            }
            catch (JsonException)
            {
                this._output.WriteLine(content);
            }
            this._output.WriteLine(string.Empty);
        }
        // Send the original request.
        var response = await base.SendAsync(request, cancellationToken);

        // Only process JSON content and successful responses.
        if (response is not
            { IsSuccessStatusCode: true, Content.Headers.ContentType.MediaType: "application/json" }) return response;

        var originalJson = await response.Content.ReadAsStringAsync(cancellationToken);
        this._output.WriteLine("=== RESPONSE ===");
        this._output.WriteLine(originalJson);
        this._output.WriteLine(string.Empty);
        if (string.IsNullOrWhiteSpace(originalJson)) return response;

        try
        {
            var root = JsonNode.Parse(originalJson);
            if (root is null) return response;

            var choices = root["choices"]?.AsArray();
            if (choices is null || choices.Count == 0) return response;

            var firstChoice = choices[0];
            var message = firstChoice?["message"];
            if (message is null) return response;

            var content = message["content"]?.GetValue<string>();
            var reasoning = message["reasoning_content"]?.GetValue<string>();

            // If both content and reasoning_content exist, append reasoning to content.
            if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(reasoning))
            {
                message["content"] = $"{content}\n\n<think>{reasoning}</think>";

                // Optionally remove the reasoning_content field entirely:
                // message.AsObject().Remove("reasoning_content");

                var modifiedJson = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                response.Content = new StringContent(modifiedJson, Encoding.UTF8, "application/json");
            }
        }
        catch
        {
            // If parsing fails, just return the original response.
            return response;
        }


        return response;
    }
}