using System.Collections.ObjectModel;
using PacMan.Core;
using PacMan.Core.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PacMan.App.ViewModels;

/// <summary>
/// ViewModel principal do jogo.
/// Faz a ponte entre o Core e a interface.
/// </summary>
public class GameViewModel : INotifyPropertyChanged
{
    private readonly GameEngine _engine;
    private bool _isGameStarted = false;

    public int MapWidth => _engine.Map.Width;
    public int MapHeight => _engine.Map.Height;

    // Expondo o mapa para a UI
    public TileType[,] Tiles => _engine.Map.Tiles;

    public Player Player => _engine.Player;

    public ObservableCollection<Ghost> Ghosts { get; }

    public GameViewModel()
    {
        // Mapa pequeno só pra visualização inicial
        _engine = new GameEngine(15, 11);

        Ghosts = new ObservableCollection<Ghost>(_engine.Ghosts);
    }

    public bool IsGameStarted
    {
        get => _isGameStarted;
        private set
        {
            if (_isGameStarted != value)
            {
                _isGameStarted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WelcomeMessageVisibility));
            }
        }
    }

    // Define se a mensagem de start deve aparecer se o jogo não começou)
    public bool WelcomeMessageVisibility => !IsGameStarted;

    public void StartGame()
    {
        if (!IsGameStarted)
        {
            IsGameStarted = true;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
