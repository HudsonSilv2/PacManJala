using Microsoft.UI.Xaml;

namespace PacMan.App;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new Window
        {
            Content = new MainPage()
        };

        window.Activate();
    }
}
