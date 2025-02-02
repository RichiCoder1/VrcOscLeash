using Newtonsoft.Json;

using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Utils;

using VrcOscLeash.UI;

namespace VrcOscLeash;

public class OscLeashesSetting : ListModuleSetting<Leash>
{
    public OscLeashesSetting()
        : base("Leashes", "Create, edit, and delete Leashes", typeof(LeashSettingView), [])
    {
    }

    protected override Leash CreateItem() => new();
}

[JsonObject(MemberSerialization.OptOut)]
public record Leash
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public Observable<string> Name { get; init; } = new();

    public Observable<LeashDirection> Direction { get; init; } = new(LeashDirection.North);

    public Observable<float> WalkingDeadzone { get; init; } = new(0.15f);

    public Observable<float> RunningDeadzone { get; init; } = new(0.70f);

    public Observable<float> UpDownDeadzone { get; init; } = new(0.5f);

    public Observable<float> UpDownCompensation { get; init; } = new(1.0f);

    public Observable<float> PullMultiplier { get; init; } = new(1.2f);

    public Observable<bool> TurningEnabled { get; init; } = new(false);

    public Observable<float> TurningDeadzone { get; init; } = new(0.15f);

    public Observable<float> TurningMultiplier { get; init; } = new(1.0f);

    public Observable<float> TurningGoal { get; init; } = new(90.0f);
}