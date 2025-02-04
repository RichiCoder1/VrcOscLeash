using System.Windows;
using System.Windows.Controls;

using VRChat.API.Model;

using VRCOSC.App.UI.Core;

namespace VrcOscLeash.UI;
/// <summary>
/// Interaction logic for LeashSettingView.xaml
/// </summary>
public partial class LeashSettingView : UserControl
{
    private readonly VrcLeashesSetting oscLeashSetting;
    private readonly VrcLeashModule module;
    private WindowManager windowManager = null!;


    public LeashSettingView(VrcLeashModule module, VrcLeashesSetting oscLeashSetting)
    {
        this.oscLeashSetting = oscLeashSetting;
        this.module = module;

        InitializeComponent();

        DataContext = oscLeashSetting;
    }


    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var instance = element.Tag as Leash;

        ArgumentNullException.ThrowIfNull(instance, nameof(sender));

        oscLeashSetting.Remove(instance);
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        var leash = new Leash()
        {
            Name = new(oscLeashSetting.Attribute.Count == 0 ? "Leash" : $"Leash_{oscLeashSetting.Attribute.Count}")
        };
        oscLeashSetting.Attribute.Add(leash);
        windowManager.TrySpawnChild(new LeashEditWindow(module, leash));
    }

    private void EditButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var instance = element.Tag as Leash;

        ArgumentNullException.ThrowIfNull(instance, nameof(sender));

        windowManager.TrySpawnChild(new LeashEditWindow(module, instance));
    }

    public static Lazy<Dictionary<LeashDirection, string>> LeashDirectionValues = new(() =>
    {
        return Enum.GetValues(typeof(LeashDirection))
            .Cast<LeashDirection>().ToDictionary(d => d, d => Enum.GetName(d)!);
    });

    private void ListingSettingView_Loaded(object sender, RoutedEventArgs e)
    {
        windowManager = new WindowManager(this);
    }
}
