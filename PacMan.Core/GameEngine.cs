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
    private readonly Position _playerSpawn;
    public int PelletsRemaining { get; private set; }
    public bool IsPlayerPoweredUp { get; private set; }

    public int LivesRemaining { get; private set; } = 3;
    public bool IsGameOver => LivesRemaining <= 0;

    private bool _playerDiedThisTurn;

    public GameEngine(int width, int height)
    {
        // Gera o mapa
        var generator = new MapGenerator();
        var generatedMap = generator.Generate(width, height);
        _playerSpawn = generatedMap.PlayerSpawn;

        // Cria mapa
        Map = new Map(generatedMap.Tiles);

        // Contabiliza as pastilhas
        PelletsRemaining = CountPellets();

        // Cria jogador
        Player = new Player
        {
            X = _playerSpawn.X,
            Y = _playerSpawn.Y
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

    public bool ConsumePlayerDeath()
    {
        if (_playerDiedThisTurn)
        {
            _playerDiedThisTurn = false;
            return true;
        }

        return false;
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

            if (entity is Player player)
            {
                var currentTile = Map.Tiles[newY, newX];

                // Adicionando pontuação
                if (currentTile == TileType.Pellet || currentTile == TileType.PowerPellet)
                {
                    player.Score += (currentTile == TileType.Pellet) ? 10 : 50;
                    Map.Tiles[newY, newX] = TileType.Path;
                    PelletsRemaining--; // Decrementa para saber quando o nível termina

                    if (currentTile == TileType.PowerPellet)
                    {
                        IsPlayerPoweredUp = true;
                    }
                }
            }
        }

        CheckCollisions();
    }

    private void CheckCollisions()
    {
        if (Ghosts.Any(g => g.X == Player.X && g.Y == Player.Y))
        {
            if (LivesRemaining > 0)
            {
                LivesRemaining--;
            }

            _playerDiedThisTurn = true;
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
        Player.X = _playerSpawn.X;
        Player.Y = _playerSpawn.Y;
    }

    private int CountPellets()
    {
        int count = 0;
        for (int y = 0; y < Map.Height; y++)
            for (int x = 0; x < Map.Width; x++)
                if (Map.Tiles[y, x] == TileType.Pellet || Map.Tiles[y, x] == TileType.PowerPellet)
                    count++;
        return count;
    }
}
