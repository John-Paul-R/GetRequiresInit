namespace GetRequiresInit;

using System;

/// <summary>
/// Specifies that runtime checks should be added to auto-property getters to
/// ensure that the backing field has been initialized via the auto-property
/// setter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class GetRequiresInitAttribute : Attribute
{
    
}