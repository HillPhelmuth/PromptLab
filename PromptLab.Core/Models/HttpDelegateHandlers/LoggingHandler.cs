using System.Text.Json;

namespace PromptLab.Core.Models.HttpDelegateHandlers;

public sealed class LoggingHandler(HttpMessageHandler innerHandler, StringWriter output) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private readonly TextWriter _output = output;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        
        _output.WriteLine(request.RequestUri?.ToString());
        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            _output.WriteLine("=== REQUEST ===");
            try
            {
                string formattedContent = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(content), s_jsonSerializerOptions);
                _output.WriteLine(formattedContent);
            }
            catch (JsonException)
            {
                _output.WriteLine(content);
            }
            _output.WriteLine(string.Empty);
        }

        // Call the next handler in the pipeline
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