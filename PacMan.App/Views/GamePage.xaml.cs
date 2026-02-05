using System;
using Microsoft.UI.Xaml.Controls;
using PacMan.App.ViewModels;
using Microsoft.UI.Xaml;
using Windows.System;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI;
using PacMan.Core.Models;
using System.Collections.Generic;

namespace PacMan.App.Views;

public sealed partial class GamePage : UserControl
{

    public GameViewModel ViewModel => (GameViewModel)this.DataContext;

    // Referências visuais dos fantasmas
    private Dictionary<Ghost, UIElement> _ghostVisuals = new();

    // Referências visuais do pacman
    private UIElement _playerVisual;
    private double _currentCellSize = 40;

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

        // Atualiza a tela após mover
        UpdateEntityPosition(_playerVisual, vm.Player);
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

        var tiles = ViewModel.Tiles;
        for (int y = 0; y < ViewModel.MapHeight; y++)
        {
            for (int x = 0; x < ViewModel.MapWidth; x++)
            {
                if (tiles[y, x] == TileType.Wall)
                {
                    var wall = new Rectangle
                    {
                        Width = _currentCellSize,
                        Height = _currentCellSize,
                        Fill = new SolidColorBrush(Colors.Blue),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(wall, x * _currentCellSize);
                    Canvas.SetTop(wall, y * _currentCellSize);
                    GameCanvas.Children.Add(wall);
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
}
