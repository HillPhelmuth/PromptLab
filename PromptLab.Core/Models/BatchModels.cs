using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace PromptLab.Core.Models;

public class BatchResultsHelper
{
	public static Dictionary<string, string> GetBatchResults(string filePath)
	{
		var lines = File.ReadAllLines(filePath);
		var results = lines.Select(x => JsonSerializer.Deserialize<BatchLineResult>(x)).ToDictionary(x => x!.Id, y => y!.GetAssistentMessage());
		return results;
	}
	public static IEnumerable<Usage?> GetUsageResults(string filePath)
	{
		var lines = File.ReadAllLines(filePath);
		var results = lines.Select(x => JsonSerializer.Deserialize<BatchLineResult>(x)).Select(x => x!.GetUsage());
		return results;
	}
	public static Dictionary<string, string> GetBatchRequest(string filePath)
	{
		var lines = File.ReadAllLines(filePath);
		var results = lines.Select(x => JsonSerializer.Deserialize<BatchRequestLine>(x)).ToDictionary(x => x!.CustomId, y => y!.Body.Messages[0].Content);
		return results;
	}
	public List<Message> Messages { get; set; } = [];
}
public class BatchLineBody
{
	[JsonPropertyName("model")]
	public string Model { get; set; } = "gpt-3.5-turbo";

	[JsonPropertyName("messages")]
	public List<Message> Messages { get; set; }	
	
}

public class Message
{
	[JsonPropertyName("role")]
	public string Role { get; set; }

	[JsonPropertyName("content")]
	public string Content { get; set; }
}
public class BatchRequestLine
{
	[JsonPropertyName("custom_id")]
	public string CustomId { get; set; }

	[JsonPropertyName("method")]
	public string Method { get; set; } = "POST";

	[JsonPropertyName("url")]
	public string Url { get; set; } = "/v1/chat/completions";

	[JsonPropertyName("body")]
	public BatchLineBody Body { get; set; }
	public override string ToString()
	{
		return JsonSerializer.Serialize(this);
	}
	public static string ToJsonLines(List<BatchRequestLine> batchLines)
	{
		var sb = new StringBuilder();
		foreach (var line in batchLines)
		{
			sb.AppendLine(line.ToString());
		}
		return sb.ToString();
	}
}

public class BatchLineResult
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = default!;

	[JsonPropertyName("custom_id")]
	public string? CustomId { get; set; }

	[JsonPropertyName("response")]
	public Response? Response { get; set; }

	[JsonPropertyName("error")]
	public object? Error { get; set; }
	public string GetAssistentMessage()
	{
		return Response?.Body?.Choices?[0].Message?.Content ?? "";
	}
	public Usage? GetUsage()
	{
		return Response?.Body?.Usage;
	}
}

public class Response
{
	[JsonPropertyName("status_code")]
	public int StatusCode { get; set; }

	[JsonPropertyName("request_id")]
	public string? RequestId { get; set; }

	[JsonPropertyName("body")]
	public BatchResponseBody? Body { get; set; }
}

public class BatchResponseBody
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("object")]
	public string? Object { get; set; }

	[JsonPropertyName("created")]
	public int Created { get; set; }

	[JsonPropertyName("model")]
	public string? Model { get; set; }

	[JsonPropertyName("choices")]
	public List<Choice>? Choices { get; set; }

	[JsonPropertyName("usage")]
	public Usage? Usage { get; set; }

	[JsonPropertyName("system_fingerprint")]
	public string? SystemFingerprint { get; set; }
}

public class Choice
{
	[JsonPropertyName("index")]
	public int Index { get; set; }

	[JsonPropertyName("message")]
	public Message? Message { get; set; }

	[JsonPropertyName("logprobs")]
	public object? Logprobs { get; set; }

	[JsonPropertyName("finish_reason")]
	public string? FinishReason { get; set; }
}

public class Usage
{
	[JsonPropertyName("prompt_tokens")]
	public int PromptTokens { get; set; }

	[JsonPropertyName("completion_tokens")]
	public int CompletionTokens { get; set; }

	[JsonPropertyName("total_tokens")]
	public int TotalTokens { get; set; }
}