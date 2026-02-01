using Avalonia.Controls;
using PacMan.App.ViewModels;

namespace PacMan.App.Views;

public sealed partial class GamePage : UserControl
{
    public GamePage()
    {
        InitializeComponent();
        DataContext = new GameViewModel();
    }
}
