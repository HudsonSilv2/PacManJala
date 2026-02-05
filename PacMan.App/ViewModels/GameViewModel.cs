using System.Collections.ObjectModel;
using PacMan.Core;
using PacMan.Core.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PacMan.Core.Enums;
using System.Windows.Input;
using System;
using System.Linq;

namespace PacMan.App.ViewModels;

// Classe auxiliar para o comando, uma implementação simples de ICommand
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;
    public event EventHandler CanExecuteChanged;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
    public void Execute(object parameter) => _execute(parameter);

    // Método para permitir a atualização manual do estado do comando
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}


public class Tile
{
    public int X { get; set; }
    public int Y { get; set; }
    public TileType Type { get; set; }
    public double Size { get; set; } = 20; // Tamanho padrão
}

public class GameViewModel : INotifyPropertyChanged
{
    private readonly GameEngine _engine;
    private bool _isGameStarted = false;

    public int MapWidth => _engine.Map.Width;
    public int MapHeight => _engine.Map.Height;

    public ObservableCollection<Tile> Tiles { get; } = new();
    public ObservableCollection<Entity> GameObjects { get; } = new();

    public ObservableCollection<ScoreEntry> HighScores { get; } = new();

    public ICommand StartGameCommand { get; }
    public ICommand MoveCommand { get; }

    public GameViewModel()
    {
        _engine = new GameEngine(28, 26);

        // Preenche as coleções para a UI
        for (int y = 0; y < _engine.Map.Height; y++)
        {
            for (int x = 0; x < _engine.Map.Width; x++)
            {
                Tiles.Add(new Tile { X = x, Y = y, Type = _engine.Map.Tiles[y, x] });
            }
        }
        
        GameObjects.Add(_engine.Player);
        foreach (var ghost in _engine.Ghosts)
        {
            GameObjects.Add(ghost);
        }

        LoadHighScores();

        StartGameCommand = new RelayCommand(_ => StartGame());
        MoveCommand = new RelayCommand(param => MovePlayer((Direction)param));
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
    }

    private void LoadHighScores()
    {
        var mockScores = new System.Collections.Generic.List<ScoreEntry>
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