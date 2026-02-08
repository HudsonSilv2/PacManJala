using Microsoft.UI.Xaml.Controls;
using PacMan.App.ViewModels;

namespace PacMan.App;

public sealed partial class MainPage : Page
{
    private readonly MainViewModel _mainViewModel = new();
    private readonly StartViewModel _startViewModel = new();

    public MainPage()
    {
        this.InitializeComponent();
        DataContext = _mainViewModel;

        StartPageView.DataContext = _startViewModel;
        StartPageView.StartRequested += (_, __) => StartGame();
        StartPageView.ExitRequested += (_, __) => Microsoft.UI.Xaml.Application.Current.Exit();

        GamePageView.GameEnded += (_, __) => ReturnToStart();

        ReturnToStart();
    }

    private void StartGame()
    {
        _mainViewModel.IsInGame = true;
        StartPageView.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        GamePageView.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        GamePageView.StartFromMenu();
    }

    private void ReturnToStart()
    {
        _mainViewModel.IsInGame = false;
        GamePageView.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        StartPageView.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        _startViewModel.LoadHighScores();
    }
}
