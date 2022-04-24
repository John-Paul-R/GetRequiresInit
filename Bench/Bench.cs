#nullable  disable
namespace Bench;

using BenchmarkDotNet.Attributes;
using GetRequiresInit;

[SimpleJob()]
public class Bench
{
    private PropertyOwner Unchecked = null!;
    private ManuallyChecked ManuallyChecked = null!;
    private CheckedPropertyOwner Checked = null!;

    [GlobalSetup]
    public void Setup()
    {
        Unchecked = new PropertyOwner();
        Unchecked.OwnedProperty = "test1";
        
        ManuallyChecked = new();
        ManuallyChecked.CheckedStringField = "test2";
        
        Checked = new();
        Checked.OwnedProperty = "test3";
        string num = Unchecked.OwnedProperty;
        num = ManuallyChecked.CheckedStringField;
        num = Checked.OwnedProperty;
    }

    [Benchmark]
    public string GetIntUnchecked() => Unchecked.OwnedProperty;

    [Benchmark]
    public string GetIntManuallyChecked() => ManuallyChecked.CheckedStringField;

    [Benchmark]
    public string GetIntChecked() => Checked.OwnedProperty;

}

public class PropertyOwner
{
    public string OwnedProperty { get; set; }
}

public class CheckedPropertyOwner
{
    [GetRequiresInit]
    public string OwnedProperty { get; set; }
}

public class ManuallyChecked
{
    private bool _initFlag_CheckedIntField = false;
    private string _checkedStringField;
    public string CheckedStringField
    {
        get
        {
            if (!_initFlag_CheckedIntField) {
                throw new InvalidOperationException();
            }
            return _checkedStringField;
        }
        set
        {
            _checkedStringField = value;
            _initFlag_CheckedIntField = true;
        }
    }
}