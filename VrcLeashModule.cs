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

        RegisterParameter<bool>(VrcLeashParameter.IsGrabbed, "VRCLeash/*/Leash_IsGrabbed", ParameterMode.Read, "IsGrabbed", "Whether or not the Leash is grabbed.");
        RegisterParameter<float>(VrcLeashParameter.Stretch, "VRCLeash/*/Leash_Stretch", ParameterMode.Read, "Stretch", "The current Stretch value of the Leash.");
        RegisterParameter<float>(VrcLeashParameter.XPos, "VRCLeash/*/X+", ParameterMode.Read, "XPos", "Current XPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.YPos, "VRCLeash/*/Y+", ParameterMode.Read, "YPos", "Current YPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.ZPos, "VRCLeash/*/Z+", ParameterMode.Read, "ZPos", "Current ZPos Contact Value");
        RegisterParameter<float>(VrcLeashParameter.XNeg, "VRCLeash/*/X-", ParameterMode.Read, "XNeg", "Current XNeg Contact Value");
        RegisterParameter<float>(VrcLeashParameter.YNeg, "VRCLeash/*/Y-", ParameterMode.Read, "YNeg", "Current YNeg Contact Value");
        RegisterParameter<float>(VrcLeashParameter.ZNeg, "VRCLeash/*/Z-", ParameterMode.Read, "ZNeg", "Current ZNeg Contact Value");
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
            .Select(param => param.Output!.Address)
            .Where(address => address.StartsWith("/avatar/parameters/VRCLeash"))
            .Select(param => param.Replace("/avatar/parameters/VRCLeash/", "").Split("/").First())
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
        var leashesSettings = GetSettingValue<VrcLeashesSetting>(VrcLeashSetting.Leashes);
        var settings = leashesSettings.Attribute.First(l => l.Name.Value == leash.Name);

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
    }
}
