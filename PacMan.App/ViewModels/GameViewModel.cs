using System.Collections.ObjectModel;
using PacMan.Core;
using PacMan.Core.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PacMan.Core.Enums;
using System.Collections.Generic;

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

    public ObservableCollection<ScoreEntry> HighScores { get; } = new();

    public GameViewModel()
    {
        _engine = new GameEngine(28, 26);
        Ghosts = new ObservableCollection<Ghost>(_engine.Ghosts);

        Ghosts = new ObservableCollection<Ghost>(_engine.Ghosts);
        LoadHighScores();
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

    public void MovePlayer(Direction direction)
    {
        if (!IsGameStarted) return;

        _engine.MovePlayer(direction);

        _engine.MoveGhosts();

        OnPropertyChanged(nameof(Player));
        OnPropertyChanged(nameof(Ghosts));
        OnPropertyChanged(nameof(Tiles)); 
    }

    private void LoadHighScores()
    {
        var mockScores = new List<ScoreEntry>
        {
            new ScoreEntry { PlayerName = "PAC MAN", Score = 5000},
            new ScoreEntry { PlayerName = "GHOST YELLOW", Score = 3500},
            new ScoreEntry { PlayerName = "GHOST RED", Score = 1200}
        };

        foreach (var s in mockScores) HighScores.Add(s);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
