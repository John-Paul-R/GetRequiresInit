namespace GetRequiresInit;

using System;

/// <summary>
/// Specified that runtime checks should be added to auto-property getters to
/// ensure that the backing field has been initialized via the auto-property
/// getter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class GetRequiresInitAttribute : Attribute
{
    
}