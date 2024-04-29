using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Models;

public class BenchmarkQandA
{
	public List<Benchmark> Benchmarks { get; set; } = [];
}
public record Benchmark(string Id, string Question, string Answer);