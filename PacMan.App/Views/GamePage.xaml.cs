using Microsoft.UI.Xaml.Controls;
using PacMan.App.ViewModels;
using Microsoft.UI.Xaml;
using Windows.System;

namespace PacMan.App.Views;

public sealed partial class GamePage : UserControl
{

    public GameViewModel ViewModel => (GameViewModel)this.DataContext;

    public GamePage()
    {
        this.InitializeComponent();
        this.DataContext = new GameViewModel();
        this.IsTabStop = true;
        this.Loaded += (s, e) =>
        {
            this.Focus(FocusState.Programmatic);

            if (Window.Current?.CoreWindow != null)
            {
                Window.Current.CoreWindow.KeyDown += (sender, args) =>
                {
                    if (args.VirtualKey == VirtualKey.Enter && !ViewModel.IsGameStarted)
                    {
                        ViewModel.StartGame();
                    }
                };
            }
        };
        this.KeyDown += GamePage_KeyDown;
    }

    private void GamePage_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (!ViewModel.IsGameStarted) return;
    }
}
