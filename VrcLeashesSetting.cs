using Newtonsoft.Json;

using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Utils;

using VrcOscLeash.UI;

namespace VrcOscLeash;

public class VrcLeashesSetting : ListModuleSetting<Leash>
{
    public VrcLeashesSetting()
        : base("Leashes", "Create, edit, and delete Leashes", typeof(LeashSettingView), [])
    {
    }

    protected override Leash CreateItem() => new();
}

[JsonObject(MemberSerialization.OptOut)]
public record Leash
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public Observable<string> Name { get; set; } = new("Leash");

    public Observable<LeashDirection> Direction { get; set; } = new(LeashDirection.North);

    public Observable<float> WalkingDeadzone { get; set; } = new(0.15f);

    public Observable<float> RunningDeadzone { get; set; } = new(0.70f);

    public Observable<float> UpDownDeadzone { get; set; } = new(0.5f);

    public Observable<float> UpDownCompensation { get; set; } = new(1.0f);

    public Observable<float> PullMultiplier { get; set; } = new(1.2f);

    public Observable<bool> TurningEnabled { get; set; } = new(false);

    public Observable<float> TurningDeadzone { get; set; } = new(0.15f);

    public Observable<float> TurningMultiplier { get; set; } = new(1.0f);

    public Observable<float> TurningGoal { get; set; } = new(90.0f);

    public Observable<bool> LegacyEnabled { get; set; } = new(false);
}