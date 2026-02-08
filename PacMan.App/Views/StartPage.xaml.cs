using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PacMan.App.Views;

public sealed partial class StartPage : UserControl
{
    public event RoutedEventHandler StartRequested;
    public event RoutedEventHandler ExitRequested;

    public StartPage()
    {
        this.InitializeComponent();
        StartButton.Click += (s, e) => StartRequested?.Invoke(this, e);
        ExitButton.Click += (s, e) => ExitRequested?.Invoke(this, e);
    }
}
