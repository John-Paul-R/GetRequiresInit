namespace SmokeTestNamespace;

using GetRequiresInit;

// This file exists so that the GetRequiresInitAttribute shows up in the
// Module's CustomAttributes. If it is missing, things break.

[GetRequiresInit]
class Class2
{
    public int Class2IntField { get; set; }
}

class Class3
{
    [GetRequiresInit]
    public int Class3IntField { get; set; }
}
