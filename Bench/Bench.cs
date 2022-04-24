namespace Bench;

using AssemblyToProcess;
using BenchmarkDotNet.Attributes;
using Fody;
using GetRequiresInit;

[MemoryDiagnoser]
public class Bench
{
    private PropertyOwner Unchecked = null!;
    private Class1 ManuallyChecked = null!;
    private CheckedPropertyOwner Checked = null!;

    [GlobalSetup]
    public void Setup()
    {
        Unchecked = new PropertyOwner();
        Unchecked.OwnedProperty = 5;
        
        ManuallyChecked = new();
        ManuallyChecked.CheckedIntField = 5;
        
        Checked = new();
        Checked.OwnedProperty = 5;
        int num = Unchecked.OwnedProperty;
        num = ManuallyChecked.CheckedIntField;
        num = Checked.OwnedProperty;
    }

    [Benchmark]
    public int GetIntUnchecked() => Unchecked.OwnedProperty;

    [Benchmark]
    public int GetIntManuallyChecked() => ManuallyChecked.CheckedIntField;

    [Benchmark]
    public int GetIntChecked() => Checked.OwnedProperty;

}

public class PropertyOwner
{
    public int OwnedProperty { get; set; }
}

// public class ManuallyCheckedPropertyOwner
// {
//     public int OwnedProperty { get; set; }
// }
//
public class CheckedPropertyOwner
{
    [GetRequiresInit]
    public int OwnedProperty { get; set; }
}