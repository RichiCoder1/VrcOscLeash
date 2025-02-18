using System.Collections.Concurrent;

using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
[assembly: System.Reflection.AssemblyTitle("VrcLeash")]
[assembly: System.Reflection.AssemblyDescription("VrcLeash VRCOSC Module")]

namespace VrcOscLeash;

[ModuleTitle("VRC Leash")]
[ModuleDescription("Powers the VRC Leash Component")]
[ModuleType(ModuleType.NSFW)]
public class VrcLeashModule : Module
{
    private readonly ConcurrentDictionary<string, LeashState> leashes = new();

    protected override void OnPreLoad()
    {
        CreateCustomSetting(VrcLeashSetting.Leashes, new VrcLeashesSetting());

        CreateGroup("Leashes", VrcLeashSetting.Leashes);

        RegisterParameter<bool>(VrcLeashParameter.IsGrabbed, "VrcLeash/*/Leash_IsGrabbed", ParameterMode.Read, "IsGrabbed", "Whether or not the Leash is grabbed.");
        RegisterParameter<float>(VrcLeashParameter.Stretch, "VrcLeash/*/Leash_Stretch", ParameterMode.Read, "Stretch", "The current Stretch value of the Leash.");
        RegisterParameter<float>(VrcLeashParameter.XPos, "VrcLeash/*/XPos", ParameterMode.Read, "XPos", "Current XPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.YPos, "VrcLeash/*/YPos", ParameterMode.Read, "YPos", "Current YPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.ZPos, "VrcLeash/*/ZPos", ParameterMode.Read, "ZPos", "Current ZPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.XNeg, "VrcLeash/*/XNeg", ParameterMode.Read, "XNeg", "Current XNeg Contact Value");
        RegisterParameter<float>(VrcLeashParameter.YNeg, "VrcLeash/*/YNeg", ParameterMode.Read, "YNeg", "Current YNeg Contact Value");
        RegisterParameter<float>(VrcLeashParameter.ZNeg, "VrcLeash/*/ZNeg", ParameterMode.Read, "ZNeg", "Current ZNeg Contact Value");


        RegisterParameter<bool>(VrcLeashParameter.IsGrabbed_Legacy, "Leas*_IsGrabbed", ParameterMode.Read, "IsGrabbed (OSCLeash)", "Whether or not the Leash is grabbed.");
        RegisterParameter<float>(VrcLeashParameter.Stretch_Legacy, "Leas*_Stretch", ParameterMode.Read, "Stretch (OSCLeash)", "The current Stretch value of the Leash.");
        RegisterParameter<float>(VrcLeashParameter.XPos_Legacy, "Leas*_X+", ParameterMode.Read, "XPos (OSCLeash)", "Current XPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.YPos_Legacy, "Leas*_Y+", ParameterMode.Read, "YPos (OSCLeash)", "Current YPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.ZPos_Legacy, "Leas*_Z+", ParameterMode.Read, "ZPos (OSCLeash)", "Current ZPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.XNeg_Legacy, "Leas*_X-", ParameterMode.Read, "XNeg (OSCLeash)", "Current XNeg Contact Value");
        RegisterParameter<float>(VrcLeashParameter.YNeg_Legacy, "Leas*_Y-", ParameterMode.Read, "YNeg (OSCLeash)", "Current YNeg Contact Value");
        RegisterParameter<float>(VrcLeashParameter.ZNeg_Legacy, "Leas*_Z-", ParameterMode.Read, "ZNeg (OSCLeash)", "Current ZNeg Contact Value");
    }

    protected override void OnAvatarChange(AvatarConfig? avatarConfig)
    {
        if (avatarConfig is null)
        {
            return;
        }

        SendMovementUpdate(0, 0, null, false);
        leashes.Clear();

        var uniqueLeashes = avatarConfig.Parameters
            .Where(param => param.Output?.Address is not null)
            .Select(param => param.Output!.Address!)
            .Where(address => address.StartsWith("/avatar/parameters/VrcLeash"))
            .Select(param => param.Replace("/avatar/parameters/VrcLeash/", "").Split("/").First())
            .Distinct().ToList();

        foreach (var leash in uniqueLeashes)
        {
            leashes.TryAdd(leash, new LeashState(leash));
        }
    }

    protected override void OnRegisteredParameterReceived(RegisteredParameter parameter)
    {
        var leashName = parameter.GetWildcard<string>(0);

        if (!string.IsNullOrWhiteSpace(leashName))
        {
            if (((VrcLeashParameter)parameter.Lookup) >= VrcLeashParameter.IsGrabbed_Legacy)
            {
                leashName = $"Leas{leashName}";
            }
            leashes.AddOrUpdate(leashName, new LeashState(leashName), (_, leash) => leash.WithParameter((VrcLeashParameter)parameter.Lookup, parameter.Value));
        }
        CalculateUpdate();
    }

    private void CalculateUpdate()
    {
        var grabbedLeash = leashes.FirstOrDefault(x => x.Value.IsGrabbed);
        var activeLeash = grabbedLeash.Value is not default(LeashState) ? grabbedLeash : leashes.FirstOrDefault();

        if (activeLeash.Value is not default(LeashState))
        {
            CalculateMovementUpdate(activeLeash.Value);
        }
        else
        {
            SendMovementUpdate(0, 0, 0, true);
        }
    }

    private void CalculateMovementUpdate(LeashState leash)
    {
        var leashesSettings = GetSettingValue<List<Leash>>(VrcLeashSetting.Leashes);
        var settings = leashesSettings.First(l => l.Name.Value == leash.Name);

        if (settings == default)
        {
            Log($"Attempted to move via unconfigured leash {leash.Name}. Make sure name matches exactly. Ignoring.");
            return;
        }
        var outputMultiplier = leash.Stretch * settings.PullMultiplier.Value;
        var verticalMove = Math.Clamp((leash.ZPos - leash.ZNeg) * outputMultiplier, -1, 1);
        var horizontalMove = Math.Clamp((leash.XPos - leash.XNeg) * outputMultiplier, -1, 1);

        var yCombined = leash.YPos + leash.YNeg;
        if (yCombined >= settings.UpDownDeadzone.Value)
        {
            verticalMove = 0.0f;
            horizontalMove = 0.0f;
        }

        if (settings.UpDownCompensation.Value != 0)
        {
            var yModifier = Math.Clamp(1.0f - yCombined * settings.UpDownCompensation.Value, -1, 1);
            if (yModifier != 0.0f)
            {
                verticalMove /= yModifier;
                horizontalMove /= yModifier;
            }
        }

        float? turnSpeed = null;
        if (settings.TurningEnabled.Value && leash.Stretch > settings.TurningDeadzone.Value && leash.Direction != LeashDirection.Unknown)
        {
            turnSpeed = settings.TurningMultiplier.Value;
            var turnGoal = settings.TurningGoal.Value / 180.0f;
            switch (leash.Direction)
            {
                case LeashDirection.North when leash.ZPos < turnGoal:
                    turnSpeed *= horizontalMove;
                    if (leash.XPos > leash.XNeg)
                    {
                        turnSpeed += leash.ZNeg;
                    }
                    else
                    {
                        turnSpeed -= leash.ZNeg;
                    }
                    break;
                case LeashDirection.South when leash.ZNeg < turnGoal:
                    turnSpeed *= -horizontalMove;
                    if (leash.XPos > leash.XNeg)
                    {
                        turnSpeed -= leash.ZPos;
                    }
                    else
                    {
                        turnSpeed += leash.ZPos;
                    }
                    break;
                case LeashDirection.East when leash.XPos < turnGoal:
                    turnSpeed *= verticalMove;
                    if (leash.ZPos > leash.ZNeg)
                    {
                        turnSpeed += leash.XNeg;
                    }
                    else
                    {
                        turnSpeed -= leash.XNeg;
                    }
                    break;
                case LeashDirection.West when leash.XNeg < turnGoal:
                    turnSpeed *= -verticalMove;
                    if (leash.ZPos > leash.ZNeg)
                    {
                        turnSpeed -= leash.XPos;
                    }
                    else
                    {
                        turnSpeed += leash.XPos;
                    }
                    break;
                case LeashDirection.Unknown:
                default:
                    turnSpeed = 0.0f;
                    break;
            }

            turnSpeed = Math.Clamp(turnSpeed ?? 0.0f, -1, 1);
        }

        LogDebug($"Leash Calc: grabbed => {leash.IsGrabbed}, stretch => {leash.Stretch}");

        if (leash.IsGrabbed)
        {
            if (leash.Stretch > settings.RunningDeadzone.Value)
            {
                SendMovementUpdate(verticalMove, horizontalMove, turnSpeed, true);
            }
            else if (leash.Stretch > settings.WalkingDeadzone.Value)
            {
                SendMovementUpdate(verticalMove, horizontalMove, turnSpeed, false);
            }
            else
            {
                SendMovementUpdate(0, 0, null, false);
            }
        }
        else
        {
            SendMovementUpdate(0, 0, settings.TurningEnabled.Value ? 0 : null, false);
        }
    }

    private void SendMovementUpdate(float verticalMove, float horizontalMove, float? turn, bool isRunning)
    {
        LogDebug($"Move Update: vertical => {verticalMove:F2}, horizontal => {horizontalMove:F2}, turn => {turn:F2}, running => {isRunning}");
        var player = GetPlayer();

        player.MoveVertical(verticalMove);
        player.MoveHorizontal(horizontalMove);
        if (turn.HasValue)
        {
            player.LookHorizontal(turn.Value);
        }

        if (isRunning)
        {
            player.Run();
        }
        else
        {
            player.StopRun();
        }
    }

    private enum VrcLeashSetting
    {
        Leashes,
    }

    internal enum VrcLeashParameter
    {
        IsGrabbed,
        Stretch,
        XPos,
        YPos,
        ZPos,
        XNeg,
        YNeg,
        ZNeg,
        IsGrabbed_Legacy,
        Stretch_Legacy,
        XPos_Legacy,
        YPos_Legacy,
        ZPos_Legacy,
        XNeg_Legacy,
        YNeg_Legacy,
        ZNeg_Legacy,
    }
}
