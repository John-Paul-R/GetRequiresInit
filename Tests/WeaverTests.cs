using System;
using System.Reflection;
using Fody;
using Xunit;

#region WeaverTests

public class WeaverTests
{
    static TestResult testResult;

    static WeaverTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("ExampleAssemblyToProcess.dll");
    }

    [Fact]
    public void ValidateHelloWorldIsInjected()
    {
        var type = testResult.Assembly.GetType("TheNamespace.Hello")!;
        var instance = (dynamic)Activator.CreateInstance(type)!;

        Assert.Equal("Hello World", instance.World());
    }
    
    [Fact]
    public void ValidateManualGetThrowsErrorWhenNoInit()
    {
        var type = testResult.Assembly.GetType("AssemblyToProcess.Class1")!;
        var instance = (dynamic)Activator.CreateInstance(type)!;

        var method = type.GetProperty("CheckedIntField")!.GetMethod;
        // var outStr = string.Join("","", methods);
        // throw new NullReferenceException(string.Join("","", type.GetMethods()));
        // var val = instance.get_Class2IntField();
        // Console.WriteLine(val);
        bool threwInvalidOperationException = false;
        try {
            method!.Invoke(instance, Array.Empty<object?>());
        } catch (TargetInvocationException ex) {
            if (ex.InnerException is InvalidOperationException) {
                threwInvalidOperationException = true;
            }
        }

        Assert.True(threwInvalidOperationException);
    }
    
    [Fact]
    public void ValidateGetThrowsErrorWhenNoInit()
    {
        var type = testResult.Assembly.GetType("AssemblyToProcess.Class2")!;
        var instance = (dynamic)Activator.CreateInstance(type)!;

        var method = type.GetProperty("Class2IntField")!.GetMethod;

        // var field = type.GetField("_initFlag_Class2IntField", BindingFlags.NonPublic | BindingFlags.Instance);
        
        bool threwInvalidOperationException = false;
        int outVal = 1031231;
        try {
            outVal = method!.Invoke(instance, Array.Empty<object?>());
        } catch (TargetInvocationException ex) {
            if (ex.InnerException is InvalidOperationException) {
                threwInvalidOperationException = true;
            }
        }

        Assert.True(threwInvalidOperationException);
    }

    [Fact]
    public void ValidateGetSucceedsWhenInit()
    {
        var type = testResult.Assembly.GetType("AssemblyToProcess.Class2")!;
        var instance = (dynamic)Activator.CreateInstance(type)!;

        var prop = type.GetProperty("Class2IntField")!;
        var getMethod = prop.GetMethod!;
        var setMethod = prop.SetMethod!;

        // var field = type.GetField("_initFlag_Class2IntField", BindingFlags.NonPublic | BindingFlags.Instance);
        int valueToSet = 1031231;
        setMethod.Invoke(instance, new object?[1] {valueToSet});
        
        bool threwInvalidOperationException = false;
        int outVal = 0;
        try {
            outVal = getMethod.Invoke(instance, Array.Empty<object?>());
        } catch (TargetInvocationException ex) {
            if (ex.InnerException is InvalidOperationException) {
                threwInvalidOperationException = true;
            }
        }

        Assert.False(threwInvalidOperationException);
        Assert.Equal(valueToSet, outVal);
    }


}

#endregion