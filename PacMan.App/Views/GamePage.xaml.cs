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

namespace PacMan.App.Views;

public sealed partial class GamePage : UserControl
{
    public GameViewModel ViewModel { get; } = new GameViewModel();

    // Referências visuais dos fantasmas
    private Dictionary<Ghost, UIElement> _ghostVisuals = new();

    // Referências visuais do pacman
    private UIElement _playerVisual;
    private double _currentCellSize = 40;
    private Dictionary<(int x, int y), UIElement> _pelletVisuals = new();

    public GamePage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
        this.IsTabStop = true;
        this.Loaded += GamePage_Loaded;
        this.PointerPressed += (s, e) => this.Focus(FocusState.Programmatic);
    }

    private void GamePage_Loaded(object sender, RoutedEventArgs e)
    {
        this.KeyDown += GamePage_KeyDown;
        this.IsTabStop = true;
        this.Focus(FocusState.Programmatic);
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

        // Lógica de início de jogo
        if (!vm.IsGameStarted)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                vm.StartGame();
                e.Handled = true;
                DrawMap();
            }
            return;
        }

        // Movimentação do pacman
        switch (key)
        {
            case Windows.System.VirtualKey.Up: vm.MovePlayer(PacMan.Core.Enums.Direction.Up); break;
            case Windows.System.VirtualKey.Down: vm.MovePlayer(PacMan.Core.Enums.Direction.Down); break;
            case Windows.System.VirtualKey.Left: vm.MovePlayer(PacMan.Core.Enums.Direction.Left); break;
            case Windows.System.VirtualKey.Right: vm.MovePlayer(PacMan.Core.Enums.Direction.Right); break;
        }

        var playerX = vm.Player.X;
        var playerY = vm.Player.Y;

        if (vm.RawTiles[playerY, playerX] == TileType.Path)
        {
            UpdatePelletsVisual(playerX, playerY);
        }

        // Atualiza a tela após mover
        UpdateEntityPosition(_playerVisual, vm.Player);

        foreach (var ghost in vm.Ghosts)
        {
            if (_ghostVisuals.ContainsKey(ghost))
            {
                UpdateEntityPosition(_ghostVisuals[ghost], ghost);
            }
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
        _ghostVisuals.Clear();
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

    private void RenderEntities()
    {
        // Cria o visual do Pac-Man
        _playerVisual = new Ellipse
        {
            Width = _currentCellSize - 4,
            Height = _currentCellSize - 4,
            Fill = new SolidColorBrush(Colors.Yellow)
        };
        GameCanvas.Children.Add(_playerVisual);
        UpdateEntityPosition(_playerVisual, ViewModel.Player);

        // Cria o visual dos Fantasmas
        foreach (var ghost in ViewModel.Ghosts)
        {
            var ghostShape = new Rectangle
            {
                Width = _currentCellSize - 4,
                Height = _currentCellSize - 4,
                Fill = new SolidColorBrush(Colors.Red),
                RadiusX = 8,
                RadiusY = 8
            };
            GameCanvas.Children.Add(ghostShape);
            _ghostVisuals[ghost] = ghostShape;
            UpdateEntityPosition(ghostShape, ghost);
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
}
