using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace PromptLab.Core.Models;

public class BenchmarkQandA
{
	public static List<Benchmark> Benchmarks { get; set; } = BatchLineResult.BenchMarkQandA();
	public static List<Benchmark> GeneratedQuestions { get; set; } = TopicQuestions.BenchmarksFromFile();

}
public record Benchmark(string Id, string Question)
{
	public string Answer { get; set; } = "";
	public string? GoldenAnswer { get; set; }

    public string Context { get; set; } = "";
}
public class TopicQuestions
{
	[JsonPropertyName("Topic")]
	public string Topic { get; set; }

	[JsonPropertyName("Questions")]
	public List<string> Questions { get; set; }
	public static List<TopicQuestions> FromJson(string json) => JsonSerializer.Deserialize<List<TopicQuestions>>(json)!;
	public static List<TopicQuestions> FromFile()
	{
		var file = @"C:\Users\adamh\source\repos\PromptLab\PromptLab.Core\BatchFiles\GeneratedTopicQuestions.json";
		var json = File.ReadAllText(file);
		return FromJson(json);
	}
	public static List<Benchmark> BenchmarksFromFile()
	{
		var topicQs = FromFile();
		var result = new List<Benchmark>();
		foreach (var topic in topicQs)
		{
			var topicName = topic.Topic;
			var index = 0;

			foreach (var question in topic.Questions)
			{
				index++;
				result.Add(new Benchmark($"{topicName}-{index}", question));
			}
		}
		return result;
	}
}

