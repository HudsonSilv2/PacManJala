using PacMan.Core.Enums;
using PacMan.Core.Models;
using PacMan.Core.Services;

namespace PacMan.Core;

/// <summary>
/// Motor principal do jogo.
/// Controla estado, movimentação e regras básicas.
/// </summary>
public class GameEngine
{
    public Map Map { get; }
    public Player Player { get; }
    public List<Ghost> Ghosts { get; }

    public GameEngine(int width, int height)
    {
        // Gera o mapa
        var generator = new MapGenerator();
        var generatedMap = generator.Generate(width, height);

        // Cria mapa
        Map = new Map(generatedMap.Tiles);

        // Cria jogador
        Player = new Player
        {
            X = generatedMap.PlayerSpawn.X,
            Y = generatedMap.PlayerSpawn.Y
        };

        // Cria fantasmas
        Ghosts = new List<Ghost>();
        foreach (var pos in generatedMap.Ghosts)
        {
            Ghosts.Add(new Ghost
            {
                X = pos.X,
                Y = pos.Y
            });
        }
    }

    /// <summary>
    /// Move o jogador respeitando paredes.
    /// </summary>
    public void MovePlayer(Direction direction)
    {
        int newX = Player.X;
        int newY = Player.Y;
    }

    /// <summary>
    /// Move todos os fantasmas de forma aleatória.
    /// </summary>
    public void MoveGhosts()
    {
        var random = new Random();

        foreach (var ghost in Ghosts)
        {
            var direction = (Direction)random.Next(0, 4);
            TryMove(ghost, direction);
        }
    }

    private void TryMove(Entity entity, Direction direction)
    {
        int newX = entity.X;
        int newY = entity.Y;

        switch (direction)
        {
            case Direction.Up: newY--; break;
            case Direction.Down: newY++; break;
            case Direction.Left: newX--; break;
            case Direction.Right: newX++; break;
        }

        if (CanMoveTo(newX, newY))
        {
            entity.X = newX;
            entity.Y = newY;

            // Consome o item no mapa se for o pacman 
            if (entity is Player)
            {
                var currentTile = Map.Tiles[newY, newX];
                if (currentTile == TileType.Pellet || currentTile == TileType.PowerPellet)
                {
                    Map.Tiles[newY, newX] = TileType.Path; // Remove a pastilha do mapa
                }
            }
        }

        CheckCollisions();
    }

    private void CheckCollisions()
    {
        if (Ghosts.Any(g => g.X == Player.X && g.Y == Player.Y))
        {
            InitializePlayerPosition();
        }
    }

    private bool CanMoveTo(int x, int y)
    {
        // Verifica limites do mapa
        if (x < 0 || y < 0 || x >= Map.Width || y >= Map.Height)
            return false;

        // Não permite atravessar paredes
        return Map.Tiles[y, x] != TileType.Wall;
    }

    private void InitializePlayerPosition()
    {
        // Percorre o mapa para encontrar uma posição que não seja parede
        for (int y = 0; y < Map.Height; y++)
        {
            for (int x = 0; x < Map.Width; x++)
            {
                if (Map.Tiles[y, x] != TileType.Wall)
                {
                    Player.X = x;
                    Player.Y = y;
                    return;
                }
            }
        }
    }
}
