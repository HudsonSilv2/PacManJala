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
        TryMove(Player, direction);
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
}
