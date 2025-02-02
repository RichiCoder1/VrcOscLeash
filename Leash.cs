namespace VrcOscLeash;

using OscLeashParameter = OscLeashModule.OscLeashParameter;

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
    public LeashState WithParameter(OscLeashParameter parameterKey, object? value)
    {
        return value switch
        {
            float floatValue => parameterKey switch
            {
                OscLeashParameter.Stretch => this with { Stretch = floatValue },
                OscLeashParameter.XPos => this with { XPos = floatValue },
                OscLeashParameter.YPos => this with { YPos = floatValue },
                OscLeashParameter.ZPos => this with { ZPos = floatValue },
                OscLeashParameter.XNeg => this with { XNeg = floatValue },
                OscLeashParameter.YNeg => this with { YNeg = floatValue },
                OscLeashParameter.ZNeg => this with { ZNeg = floatValue },
                _ => this
            },
            bool boolValue => parameterKey switch
            {
                OscLeashParameter.IsGrabbed => this with { IsGrabbed = boolValue, Stretch = boolValue ? Stretch : 0.0f },
                _ => this
            },
            _ => this
        };
    }
}