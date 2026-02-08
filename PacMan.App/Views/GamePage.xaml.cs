using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Input;
using Windows.System;
using PacMan.Core.Models;
using PacMan.App.ViewModels;
using PacMan.App.Services;
using PacMan.Core.Enums;

namespace PacMan.App.Views;

public sealed partial class GamePage : UserControl
{
    public GameViewModel ViewModel { get; } = new GameViewModel();

    public event EventHandler GameEnded;

    private sealed class GhostSpriteState
    {
        public Image Image { get; init; }
        public SpriteAnimation Up { get; init; }
        public SpriteAnimation Down { get; init; }
        public SpriteAnimation Left { get; init; }
        public SpriteAnimation Right { get; init; }
        public Direction CurrentDirection { get; set; } = Direction.Left;
        public int LastX { get; set; }
        public int LastY { get; set; }
    }

    // Referências visuais do pacman
    private Image _playerImage;
    private double _currentCellSize = 40;
    private Dictionary<(int x, int y), UIElement> _pelletVisuals = new();
    private DispatcherTimer _gameTimer;
    private DispatcherTimer _winTimer;
    private Direction? _currentDirection;
    private bool _winSequenceStarted;

    private readonly Dictionary<Ghost, GhostSpriteState> _ghostSprites = new();
    private SpriteAnimation _playerUp;
    private SpriteAnimation _playerDown;
    private SpriteAnimation _playerLeft;
    private SpriteAnimation _playerRight;
    private SpriteAnimation _playerAnim;
    private Direction _playerDirection = Direction.Right;

    public GamePage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
        this.IsTabStop = true;
        this.Loaded += GamePage_Loaded;
        this.PointerPressed += (s, e) => this.Focus(FocusState.Programmatic);
    }

    public void StartFromMenu()
    {
        ViewModel.StartGame();
        _playerDirection = Direction.Right;
        DrawMap();
        StartTimer();
        this.Focus(FocusState.Programmatic);
    }

    private void GamePage_Loaded(object sender, RoutedEventArgs e)
    {
        this.KeyDown += GamePage_KeyDown;
        this.IsTabStop = true;
        this.Focus(FocusState.Programmatic);

        if (_gameTimer == null)
        {
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(120)
            };
            _gameTimer.Tick += GameTimer_Tick;
        }

        if (_winTimer == null)
        {
            _winTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _winTimer.Tick += WinTimer_Tick;
        }
    }

    private void GamePage_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        var vm = ViewModel;
        var key = e.Key;

        // Lógica para sair do jogo
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            Microsoft.UI.Xaml.Application.Current.Exit();
            e.Handled = true;
            return;
        }

        if (e.Key == Windows.System.VirtualKey.P)
        {
            vm.TogglePause();
            e.Handled = true;
            return;
        }

        if (e.Key == Windows.System.VirtualKey.R)
        {
            vm.RestartGame();
            DrawMap();
            _currentDirection = null;
            _winSequenceStarted = false;
            e.Handled = true;
            return;
        }

        // Lógica de início de jogo
        if (!vm.IsGameStarted)
        {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            vm.StartGame();
            e.Handled = true;
            _playerDirection = Direction.Right;
            DrawMap();
            StartTimer();
            _winSequenceStarted = false;
        }
            return;
        }

        // Movimentação do pacman
        switch (key)
        {
            case Windows.System.VirtualKey.Up: _currentDirection = Direction.Up; break;
            case Windows.System.VirtualKey.Down: _currentDirection = Direction.Down; break;
            case Windows.System.VirtualKey.Left: _currentDirection = Direction.Left; break;
            case Windows.System.VirtualKey.Right: _currentDirection = Direction.Right; break;
            default: return;
        }

        _playerDirection = _currentDirection.Value;
        _playerAnim = GetPlayerAnimation(_playerDirection);
        _playerAnim.Reset();
        if (_playerImage != null)
        {
            _playerImage.Source = _playerAnim.GetFrameImage();
        }
    }

    private void GameContainer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawMap();
    }

    private void DrawMap()
    {
        double containerWidth = ((Grid)GameCanvas.Parent).ActualWidth;
        double containerHeight = ((Grid)GameCanvas.Parent).ActualHeight;

        if (containerWidth == 0 || ViewModel.MapWidth == 0) return;

        double sizeX = containerWidth / ViewModel.MapWidth;
        double sizeY = containerHeight / ViewModel.MapHeight;
        _currentCellSize = Math.Min(sizeX, sizeY);

        GameCanvas.Width = _currentCellSize * ViewModel.MapWidth;
        GameCanvas.Height = _currentCellSize * ViewModel.MapHeight;

        GameCanvas.Children.Clear();
        _ghostSprites.Clear();
        _pelletVisuals.Clear();

        var tiles = ViewModel.RawTiles;
        for (int y = 0; y < ViewModel.MapHeight; y++)
        {
            for (int x = 0; x < ViewModel.MapWidth; x++)
            {
                UIElement visual = null;
                TileType type = tiles[y, x];

                if (type == TileType.Wall)
                {
                    visual = new Rectangle
                    {
                        Width = _currentCellSize,
                        Height = _currentCellSize,
                        Fill = new SolidColorBrush(Colors.Blue),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 0.5
                    };
                }
                else if (type == TileType.Pellet)
                {
                    // Desenha a pastilha pequena
                    visual = new Ellipse
                    {
                        Width = _currentCellSize * 0.2, // 20% do tamanho da célula
                        Height = _currentCellSize * 0.2,
                        Fill = new SolidColorBrush(Colors.White),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    _pelletVisuals[(x, y)] = visual;
                }
                else if (type == TileType.PowerPellet)
                {
                    visual = new Ellipse
                    {
                        Width = _currentCellSize * 0.5,
                        Height = _currentCellSize * 0.5,
                        Fill = new SolidColorBrush(Colors.Yellow)
                    };
                    _pelletVisuals[(x, y)] = visual;
                }

                if (visual != null)
                {
                    double left = x * _currentCellSize + (_currentCellSize - (visual is FrameworkElement feW ? feW.Width : 0)) / 2;
                    double top = y * _currentCellSize + (_currentCellSize - (visual is FrameworkElement feH ? feH.Height : 0)) / 2;

                    Canvas.SetLeft(visual, left);
                    Canvas.SetTop(visual, top);
                    GameCanvas.Children.Add(visual);
                }
            }
        }
        RenderEntities();
    }

    private void GameTimer_Tick(object sender, object e)
    {
        var vm = ViewModel;
        if (!vm.IsGameStarted || vm.IsPaused || vm.IsGameOver)
        {
            return;
        }

        if (vm.IsWin)
        {
            StartWinSequence();
            return;
        }

        if (_currentDirection == null)
        {
            return;
        }

        vm.MovePlayer(_currentDirection.Value);
        RefreshAfterMove();
    }

    private void RenderEntities()
    {
        InitializePlayerAnimations();

        _playerImage = new Image
        {
            Width = _currentCellSize - 4,
            Height = _currentCellSize - 4,
            Stretch = Stretch.Uniform
        };
        _playerAnim = GetPlayerAnimation(_playerDirection);
        _playerImage.Source = _playerAnim.GetFrameImage();
        GameCanvas.Children.Add(_playerImage);
        UpdateEntityPosition(_playerImage, ViewModel.Player);

        // Cria o visual dos Fantasmas
        var ghostBases = new[]
        {
            "fantasmavermelho",
            "fantasmarosa",
            "fantasmaazul",
            "fantasmalaranja"
        };

        int ghostIndex = 0;
        foreach (var ghost in ViewModel.Ghosts)
        {
            var baseName = ghostBases[ghostIndex % ghostBases.Length];
            ghostIndex++;

            var ghostImage = new Image
            {
                Width = _currentCellSize - 4,
                Height = _currentCellSize - 4,
                Stretch = Stretch.Uniform
            };
            var state = CreateGhostSpriteState(ghostImage, baseName, ghost);
            ghostImage.Source = state.Left.GetFrameImage();
            GameCanvas.Children.Add(ghostImage);
            _ghostSprites[ghost] = state;
            UpdateEntityPosition(ghostImage, ghost);
        }
    }

    private void RefreshAfterMove()
    {
        var vm = ViewModel;
        var playerX = vm.Player.X;
        var playerY = vm.Player.Y;

        if (vm.RawTiles[playerY, playerX] == TileType.Path)
        {
            UpdatePelletsVisual(playerX, playerY);
        }

        UpdateEntityPosition(_playerImage, vm.Player);

        foreach (var ghost in vm.Ghosts)
        {
            if (_ghostSprites.TryGetValue(ghost, out var state))
            {
                UpdateEntityPosition(state.Image, ghost);
                UpdateGhostSprite(state, ghost);
            }
        }

        UpdatePlayerSprite();

        if (vm.IsWin)
        {
            StartWinSequence();
        }
    }

    private void UpdateEntityPosition(UIElement visual, Entity entity)
    {
        if (visual != null && entity != null)
        {
            Canvas.SetLeft(visual, entity.X * _currentCellSize + 2);
            Canvas.SetTop(visual, entity.Y * _currentCellSize + 2);
        }
    }

    private void UpdatePelletsVisual(int x, int y)
    {
        if (_pelletVisuals.TryGetValue((x, y), out var visual))
        {
            GameCanvas.Children.Remove(visual);
            _pelletVisuals.Remove((x, y));
        }
    }

    private void InitializePlayerAnimations()
    {
        _playerUp = new SpriteAnimation("sprites/32x32/pacman/pacman_cima", 2, 1);
        _playerDown = new SpriteAnimation("sprites/32x32/pacman/pacman_baixo", 2, 1);
        _playerLeft = new SpriteAnimation("sprites/32x32/pacman/pacman_esquerda", 2, 1);
        _playerRight = new SpriteAnimation("sprites/32x32/pacman/pacman_direita", 2, 1);
    }

    private SpriteAnimation GetPlayerAnimation(Direction direction)
    {
        return direction switch
        {
            Direction.Up => _playerUp,
            Direction.Down => _playerDown,
            Direction.Left => _playerLeft,
            _ => _playerRight
        };
    }

    private void UpdatePlayerSprite()
    {
        if (_playerImage == null || _playerAnim == null)
        {
            return;
        }

        if (_playerAnim.Update())
        {
            _playerImage.Source = _playerAnim.GetFrameImage();
        }
    }

    private GhostSpriteState CreateGhostSpriteState(Image image, string baseName, Ghost ghost)
    {
        var up = new SpriteAnimation($"sprites/32x32/ghosts/{baseName}_cima", 2, 2);
        var down = new SpriteAnimation($"sprites/32x32/ghosts/{baseName}_baixo", 2, 2);
        var left = new SpriteAnimation($"sprites/32x32/ghosts/{baseName}_esquerda", 2, 2);
        var right = new SpriteAnimation($"sprites/32x32/ghosts/{baseName}_direita", 2, 2);

        return new GhostSpriteState
        {
            Image = image,
            Up = up,
            Down = down,
            Left = left,
            Right = right,
            LastX = ghost.X,
            LastY = ghost.Y,
            CurrentDirection = Direction.Left
        };
    }

    private void UpdateGhostSprite(GhostSpriteState state, Ghost ghost)
    {
        var dx = ghost.X - state.LastX;
        var dy = ghost.Y - state.LastY;

        if (dx == 0 && dy == 0)
        {
            return;
        }

        var previousDirection = state.CurrentDirection;

        if (dx > 0)
        {
            state.CurrentDirection = Direction.Right;
        }
        else if (dx < 0)
        {
            state.CurrentDirection = Direction.Left;
        }
        else if (dy > 0)
        {
            state.CurrentDirection = Direction.Down;
        }
        else if (dy < 0)
        {
            state.CurrentDirection = Direction.Up;
        }

        var anim = state.CurrentDirection switch
        {
            Direction.Up => state.Up,
            Direction.Down => state.Down,
            Direction.Left => state.Left,
            _ => state.Right
        };

        if (previousDirection != state.CurrentDirection)
        {
            anim.Reset();
            state.Image.Source = anim.GetFrameImage();
        }
        else if (anim.Update())
        {
            state.Image.Source = anim.GetFrameImage();
        }

        state.LastX = ghost.X;
        state.LastY = ghost.Y;
    }

    private void Restart_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RestartGame();
        _playerDirection = Direction.Right;
        DrawMap();
        _currentDirection = null;
        _winSequenceStarted = false;
        StartTimer();
        this.Focus(FocusState.Programmatic);
    }

    private void Menu_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetForMenu();
        _playerDirection = Direction.Right;
        _currentDirection = null;
        _winSequenceStarted = false;
        StopWinTimer();
        StopTimer();
        GameEnded?.Invoke(this, EventArgs.Empty);
    }

    private void StartTimer()
    {
        if (_gameTimer == null)
        {
            return;
        }

        if (!_gameTimer.IsEnabled)
        {
            _gameTimer.Start();
        }
    }

    private void StopTimer()
    {
        if (_gameTimer != null && _gameTimer.IsEnabled)
        {
            _gameTimer.Stop();
        }
    }

    private void StartWinSequence()
    {
        if (_winSequenceStarted)
        {
            return;
        }

        _winSequenceStarted = true;
        StopTimer();
        if (_winTimer != null && !_winTimer.IsEnabled)
        {
            _winTimer.Start();
        }
    }

    private void StopWinTimer()
    {
        if (_winTimer != null && _winTimer.IsEnabled)
        {
            _winTimer.Stop();
        }
    }

    private void WinTimer_Tick(object sender, object e)
    {
        StopWinTimer();
        _winSequenceStarted = false;
        ViewModel.StartNextRoundKeepScore();
        DrawMap();
        StartTimer();
    }
}
