namespace DotnetAzureStarter.Core.Enums;

/// <summary>
/// Priority level of a todo item. Replaces magic integer constants (1/2/3)
/// with a named, type-safe enum to prevent invalid values at compile time.
/// </summary>
public enum TodoPriority
{
    /// <summary>Urgent — must be done immediately.</summary>
    High = 1,

    /// <summary>Normal — standard priority.</summary>
    Medium = 2,

    /// <summary>Nice-to-have — no immediate deadline.</summary>
    Low = 3
}
