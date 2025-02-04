using VRCOSC.App.UI.Core;

namespace VrcOscLeash.UI;
/// <summary>
/// Interaction logic for LeashEditWindow.xaml
/// </summary>
public partial class LeashEditWindow : IManagedWindow
{

    public LeashEditWindow(VrcLeashModule _, Leash leash)
    {
        InitializeComponent();
        DataContext = leash;

        leash.Name.Subscribe(name => Title = $"{name} Settings", true);
    }

    public object GetComparer() => DataContext;
}
