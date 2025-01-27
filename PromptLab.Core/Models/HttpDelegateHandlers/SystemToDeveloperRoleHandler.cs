using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PromptLab.Core.Models.HttpDelegateHandlers;

public class SystemToDeveloperRoleHandler(HttpMessageHandler innerHandler, StringWriter output) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private readonly TextWriter _output = output;
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Only handle if there is content and it's JSON.
        if (request.Content is not { Headers.ContentType.MediaType: "application/json" })
            return await base.SendAsync(request, cancellationToken);
        var originalBody = await request.Content.ReadAsStringAsync(cancellationToken);
        this._output.WriteLine("=== Original Request ===");
        this._output.WriteLine(originalBody);
        this._output.WriteLine(string.Empty);
        if (string.IsNullOrWhiteSpace(originalBody)) return await base.SendAsync(request, cancellationToken);
        try
        {
            var root = JsonNode.Parse(originalBody);
            if (root is not null)
            {
                var keysToRemove = new[] { "top_p", "presence_penalty", "frequency_penalty", "logprobs", "top_logprobs", "logit_bias" };
                foreach (var key in keysToRemove)
                {
                    if (root is JsonObject obj)
                    {
                        obj.Remove(key);
                    }
                }
                var modelValue = root["model"]?.GetValue<string>() ?? string.Empty;
                var isO1Mini = modelValue.Contains("o1-mini");
                var messages = root["messages"]?.AsArray();
                if (messages is not null)
                {
                    foreach (var message in messages)
                    {
                        if (message?["role"]?.GetValue<string>() == "system")
                        {
                            message["role"] = isO1Mini ? "user" : "developer";
                        }
                    }

                    var modifiedJson = root.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    this._output.WriteLine("=== Modified Request ===");
                    this._output.WriteLine(modifiedJson);
                    this._output.WriteLine(string.Empty);
                    request.Content = new StringContent(modifiedJson, Encoding.UTF8, "application/json");
                }
            }
        }
        catch
        {
            // If parsing fails, do nothing – just fall through
            // and send the original request body.
        }

        var responseMessage = await base.SendAsync(request, cancellationToken);
        var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        this._output.WriteLine("=== RESPONSE ===");
        this._output.WriteLine(responseBody);
        this._output.WriteLine(string.Empty);
        return responseMessage;
    }
}