using System.Collections.ObjectModel;
using System.Collections.Generic;
using PacMan.Core;
using PacMan.Core.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PacMan.Core.Enums;
using System.Windows.Input;
using System;
using System.Linq;
using PacMan.App.Services;

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


public class Tile : INotifyPropertyChanged
{
    private TileType _type;
    public int X { get; set; }
    public int Y { get; set; }
    public TileType Type
    {
        get => _type;
        set { _type = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class GameViewModel : INotifyPropertyChanged
{
    private GameEngine _engine;
    private readonly HighScoreService _highScoreService = new();
    private readonly AudioService _audioService = AudioService.Instance;
    private bool _isGameStarted;
    private bool _isPaused;
    private bool _isGameOver;
    private bool _isWin;

    private const int DefaultMapWidth = 28;
    private const int DefaultMapHeight = 29;

    public Player Player => _engine.Player;
    public IEnumerable<Ghost> Ghosts => _engine.Ghosts;
    public TileType[,] RawTiles => _engine.Map.Tiles;

    public int MapWidth => _engine.Map.Width;
    public int MapHeight => _engine.Map.Height;

    public ObservableCollection<Tile> Tiles { get; } = new();
    public ObservableCollection<Entity> GameObjects { get; } = new();

    public ObservableCollection<ScoreEntry> HighScores { get; } = new();

    public ICommand StartGameCommand { get; }
    public ICommand MoveCommand { get; }

    public GameViewModel()
    {
        _engine = new GameEngine(DefaultMapWidth, DefaultMapHeight);
        InitializeCollections();

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

    public bool IsPaused
    {
        get => _isPaused;
        private set
        {
            if (_isPaused != value)
            {
                _isPaused = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsGameOver
    {
        get => _isGameOver;
        private set
        {
            if (_isGameOver != value)
            {
                _isGameOver = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsWin
    {
        get => _isWin;
        private set
        {
            if (_isWin != value)
            {
                _isWin = value;
                OnPropertyChanged();
            }
        }
    }

    public bool WelcomeMessageVisibility => !IsGameStarted;

    public int CurrentScore => _engine.Player.Score;

    public int LivesRemaining => _engine.LivesRemaining;

    public void StartGame()
    {
        if (!IsGameStarted)
        {
            IsGameStarted = true;
            IsPaused = false;
            _audioService.PlayStart();
        }
    }

    public void TogglePause()
    {
        if (!IsGameStarted || IsGameOver) return;
        IsPaused = !IsPaused;
    }

    public void RestartGame()
    {
        _engine = new GameEngine(DefaultMapWidth, DefaultMapHeight);
        InitializeCollections();
        IsGameOver = false;
        IsWin = false;
        IsPaused = false;
        IsGameStarted = true;
        _audioService.PlayStart();

        OnPropertyChanged(nameof(CurrentScore));
        OnPropertyChanged(nameof(LivesRemaining));
    }

    public void ResetForMenu()
    {
        _engine = new GameEngine(DefaultMapWidth, DefaultMapHeight);
        InitializeCollections();
        IsGameOver = false;
        IsWin = false;
        IsPaused = false;
        IsGameStarted = false;

        OnPropertyChanged(nameof(CurrentScore));
        OnPropertyChanged(nameof(LivesRemaining));
    }

    // Atualize o método MovePlayer para avisar a UI sobre o score
    public void MovePlayer(Direction direction)
    {
        if (!IsGameStarted || IsPaused || IsGameOver || IsWin) return;

        var scoreBefore = CurrentScore;
        var pelletsBefore = _engine.PelletsRemaining;

        _engine.MovePlayer(direction);
        _engine.MoveGhosts();

        // Notifica que o Score mudou para atualizar o TextBlock na tela
        OnPropertyChanged(nameof(CurrentScore));

        var scoreDelta = CurrentScore - scoreBefore;
        if (_engine.PelletsRemaining < pelletsBefore)
        {
            if (scoreDelta >= 50)
            {
                _audioService.PlayEatFruit();
            }
            else
            {
                _audioService.PlayEatDot();
            }
        }

        if (_engine.ConsumePlayerDeath())
        {
            OnPropertyChanged(nameof(LivesRemaining));
            _audioService.PlayDeath();
            if (_engine.IsGameOver)
            {
                IsGameOver = true;
                _audioService.PlayGameOver();
                SaveScore();
            }
        }

        if (_engine.PelletsRemaining <= 0 && !IsGameOver)
        {
            IsWin = true;
            _audioService.PlayWin();
        }

        OnPropertyChanged(nameof(GameObjects));
        UpdateTilesUI();
    }

    private void InitializeCollections()
    {
        Tiles.Clear();
        GameObjects.Clear();

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

        OnPropertyChanged(nameof(MapWidth));
        OnPropertyChanged(nameof(MapHeight));
    }

    public void StartNextRoundKeepScore()
    {
        var score = CurrentScore;
        _engine = new GameEngine(DefaultMapWidth, DefaultMapHeight);
        _engine.Player.Score = score;

        InitializeCollections();
        IsWin = false;
        IsPaused = false;
        IsGameStarted = true;

        OnPropertyChanged(nameof(CurrentScore));
        OnPropertyChanged(nameof(LivesRemaining));
    }

    private void UpdateTilesUI()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            var tile = Tiles[i];
            tile.Type = _engine.Map.Tiles[tile.Y, tile.X];
        }
    }

    private void LoadHighScores()
    {
        var scores = _highScoreService.Load();
        if (scores.Count == 0)
        {
            scores = new List<ScoreEntry>
            {
                new ScoreEntry { PlayerName = "PAC MAN", Score = 5000},
                new ScoreEntry { PlayerName = "GHOST YELLOW", Score = 3500},
                new ScoreEntry { PlayerName = "GHOST RED", Score = 1200}
            };
            _highScoreService.Save(scores);
        }

        HighScores.Clear();
        foreach (var s in scores.OrderByDescending(s => s.Score))
        {
            HighScores.Add(s);
        }
    }

    private void SaveScore()
    {
        var scores = _highScoreService.Load();
        scores.Add(new ScoreEntry { PlayerName = "PLAYER", Score = CurrentScore });

        var top = scores
            .OrderByDescending(s => s.Score)
            .Take(10)
            .ToList();

        _highScoreService.Save(top);

        HighScores.Clear();
        foreach (var s in top)
        {
            HighScores.Add(s);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
