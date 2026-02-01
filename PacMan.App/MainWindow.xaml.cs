using Avalonia.Controls;
using PacMan.App.Views;

namespace PacMan.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var rootFrame = this.FindControl<ContentControl>("RootFrame");
        if (rootFrame != null)
        {
            rootFrame.Content = new GamePage();
        }
    }
}
