using PacMan.Core.Models;

namespace PacMan.Core.Services;

/// <summary>
/// Responsável por gerar o mapa do jogo.
/// Geração simples: paredes apenas nas bordas e interior livre.
/// Tudo aqui foi feito para ser fácil de alterar no futuro (KISS).
/// </summary>
public class MapGenerator
{
    private readonly Random _random = new();

    // Valores fáceis de ajustar depois
    private const int MinGhosts = 2;
    private const int MaxGhosts = 6;

    /// <summary>
    /// Gera um mapa com paredes nas bordas e caminhos no interior.
    /// Também define posição inicial do jogador e dos fantasmas.
    /// </summary>
    public GeneratedMap Generate(int width, int height)
    {
        // Cria a matriz de tiles
        var tiles = new TileType[height, width];

        //Inicializa tudo como caminho
        for (int y = 0; y < height; y++) //laço duplo para preencher a matriz
        {
            for (int x = 0; x < width; x++)
            {
                tiles[y, x] = TileType.Path;
            }
        }

        //Cria paredes apenas nas bordas
        for (int x = 0; x < width; x++)
        {
            tiles[0, x] = TileType.Wall;
            tiles[height - 1, x] = TileType.Wall;
        }

        for (int y = 0; y < height; y++)
        {
            tiles[y, 0] = TileType.Wall;
            tiles[y, width - 1] = TileType.Wall;
        }

        //Coleta todas as posições livres (caminhos)
        var freePositions = new List<Position>();

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                freePositions.Add(new Position(x, y));
            }
        }

        Shuffle(freePositions);

        //Define posição inicial do jogador
        var playerSpawn = freePositions.Count > 0
            ? freePositions[0]
            : new Position(1, 1);

        freePositions.RemoveAt(0);

        //Define posições dos fantasmas (movimento aleatório depois)
        var ghosts = new List<Position>();
        int ghostCount = Math.Min(
            freePositions.Count,
            _random.Next(MinGhosts, MaxGhosts + 1)
        );

        for (int i = 0; i < ghostCount; i++)
        {
            ghosts.Add(freePositions[0]);
            freePositions.RemoveAt(0);
        }

            // Coloca pellets em todos os caminhos
        for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            if (tiles[y, x] == TileType.Path)
            {
                tiles[y, x] = TileType.Pellet;
            }
        }
    }

        // Garante que o spawn do jogador não tenha pellet
        tiles[playerSpawn.Y, playerSpawn.X] = TileType.Path;

        //Retorna o mapa gerado
        return new GeneratedMap
        {
            Tiles = tiles,
            PlayerSpawn = playerSpawn,
            Ghosts = ghosts,
            Width = width,
            Height = height
        };
    }

    /// <summary>
    /// Embaralha a lista de posições.
    /// Método separado para facilitar testes ou troca futura.
    /// </summary>
    private void Shuffle(List<Position> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
