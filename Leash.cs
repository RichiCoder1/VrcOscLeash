namespace VrcOscLeash;

using VrcLeashParameter = VrcLeashModule.VrcLeashParameter;

public enum LeashDirection
{
    Unknown,
    North,
    East,
    South,
    West,
}

internal record LeashState(
    string Name,
    LeashDirection Direction = LeashDirection.Unknown,
    bool IsGrabbed = false,
    float Stretch = 0.0f,
    float XPos = 0.0f,
    float YPos = 0.0f,
    float ZPos = 0.0f,
    float XNeg = 0.0f,
    float YNeg = 0.0f,
    float ZNeg = 0.0f)
{
    public LeashState WithParameter(VrcLeashParameter parameterKey, object? value)
    {
        return value switch
        {
            float floatValue => parameterKey switch
            {
                VrcLeashParameter.Stretch => this with { Stretch = floatValue },
                VrcLeashParameter.XPos => this with { XPos = floatValue },
                VrcLeashParameter.YPos => this with { YPos = floatValue },
                VrcLeashParameter.ZPos => this with { ZPos = floatValue },
                VrcLeashParameter.XNeg => this with { XNeg = floatValue },
                VrcLeashParameter.YNeg => this with { YNeg = floatValue },
                VrcLeashParameter.ZNeg => this with { ZNeg = floatValue },
                _ => this
            },
            bool boolValue => parameterKey switch
            {
                VrcLeashParameter.IsGrabbed => this with { IsGrabbed = boolValue, Stretch = boolValue ? Stretch : 0.0f },
                _ => this
            },
            _ => this
        };
    }
}