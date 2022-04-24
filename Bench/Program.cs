namespace Bench;

using BenchmarkDotNet.Running;
using Fody;

public class Program
{
    public static void Main(string[] args)
    {

        BenchmarkRunner.Run<Bench>();
    }
}