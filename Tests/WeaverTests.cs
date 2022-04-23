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
    
    [Theory]
    [InlineData("Class2", "Class2IntField")]
    [InlineData("Class3", "Class3IntField")]
    public void ValidateGetThrowsErrorWhenNoInit(string className, string propertyName)
    {
        var type = testResult.Assembly.GetType($"AssemblyToProcess.{className}")!;
        var instance = (dynamic)Activator.CreateInstance(type)!;

        var method = type.GetProperty(propertyName)!.GetMethod;

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

    [Theory]
    [InlineData("Class2", "Class2IntField")]
    [InlineData("Class3", "Class3IntField")]
    public void ValidateGetSucceedsWhenInit(string className, string propertyName)
    {
        var type = testResult.Assembly.GetType($"AssemblyToProcess.{className}")!;
        var instance = (dynamic)Activator.CreateInstance(type)!;

        var prop = type.GetProperty(propertyName)!;
        var getMethod = prop.GetMethod!;
        var setMethod = prop.SetMethod!;

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