using PacMan.Core.Models;

namespace PacMan.Core.Services;

/// <summary>
/// Responsável por gerar o mapa do jogo.
/// Geração simples: paredes apenas nas bordas e interior livre.
/// Tudo aqui foi feito para ser fácil de alterar no futuro (KISS).
/// </summary>
public class MapGenerator
{
    // # = Parede, * = Ponto pequeno, O = Ponto grande, . = Caminho vazio
    private readonly string[] StaticMapLayout = new string[]
    {
        "############################",
        "#************##************#",
        "#*####*#####*##*#####*####*#",
        "#O####*#####*##*#####*####O#",
        "#*####*#####*##*#####*####*#",
        "#**************************#",
        "#*####*##*########*##*####*#",
        "#******##****##****##******#",
        "######*#####*##*#####*######",
        "######*#####*##*#####*######",
        "######*##..........##*######",
        "######*##.###--###.##*######",
        "######*##.#      #.##*######",
        "......*...#      #...*......",
        "######*##.#      #.##*######",
        "######*##.###--###.##*######",
        "######*##..........##*######",
        "######*#####*##*#####*######",
        "######*#####*##*#####*######",
        "#******##****##****##******#",
        "#*####*##*########*##*####*#",
        "#**************************#",
        "#*####*#####*##*#####*####*#",
        "#O**##****************##**O#",
        "###*##*##*########*##*##*###",
        "#******##****##****##******#",
        "#*##########*##*##########*#",
        "#**************************#",
        "############################"
    };

    /// <summary>
    /// Gera um mapa com paredes nas bordas e caminhos no interior.
    /// Também define posição inicial do jogador e dos fantasmas.
    /// </summary>
    public GeneratedMap Generate(int width, int height)
    {
        int layoutHeight = StaticMapLayout.Length;
        int layoutWidth = StaticMapLayout[0].Length;

        // Cria a matriz de tiles
        var tiles = new TileType[layoutHeight, layoutWidth];
        var ghosts = new List<Position>();
        var playerSpawn = new Position(1, 1);

        for (int y = 0; y < layoutHeight; y++)
        {
            for (int x = 0; x < layoutWidth; x++)
            {
                char tileChar = StaticMapLayout[y][x];

                // Mapeia o caractere da string para o Enum TileType
                tiles[y, x] = tileChar switch
                {
                    '#' => TileType.Wall,
                    '*' => TileType.Pellet,
                    'O' => TileType.PowerPellet,
                    '-' => TileType.Path,
                    _ => TileType.Path
                };

                if (tileChar == '.') playerSpawn = new Position(x, y);
                
                // Lógica que define a posição inicial dos fantasmas no mapa
                if (tileChar == ' ' && ghosts.Count < 4)
                {
                    ghosts.Add(new Position(x, y));
                }
            }
        }

        return new GeneratedMap
        {
            Tiles = tiles,
            PlayerSpawn = playerSpawn,
            Ghosts = ghosts,
            Width = layoutWidth,
            Height = layoutHeight
        };
    }
}
