namespace AssemblyToProcess;

using System;
using System.Collections.Generic;
using GetRequiresInit;

public class Class1
{
    private bool _initFlag_CheckedIntField = false;
    private int _checkedIntField;
    public int CheckedIntField
    {
        get
        {
            if (!_initFlag_CheckedIntField) {
                throw new InvalidOperationException();
                // "Get was called before Set") {
                //     Data = {
                //         ["Property"] = nameof(CheckedIntField),
                //     },
                // };
            }
            return _checkedIntField;
        }
        set
        {
            _checkedIntField = value;
            _initFlag_CheckedIntField = true;
        }
    }
}

[GetRequiresInit]
public class Class2
{
    public int Class2IntField { get; set; }
}

public class Class3
{
    [GetRequiresInit]
    public int Class3IntField { get; set; }
}

class TestClass<TDerived> where TDerived : TestClass<TDerived>
{
    
}

class TestClassImpl : TestClass<TestClassImpl>
{
    
}

class ImplThing
{
    private List<TestClass<T>> GetItems<T>() where T : TestClass<T>
    {
        return new List<TestClass<T>>();
    }

    private List<TestClassImpl> List { get; set; } = null!;
}
