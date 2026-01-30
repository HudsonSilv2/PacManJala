namespace PacMan.Core.Models;

/// <summary>
/// Representa um mapa já gerado e pronto para uso pelo jogo.
/// </summary>
public class GeneratedMap
{
    public TileType[,] Tiles { get; set; } = null!;

    public Position PlayerSpawn { get; set; }

    public List<Position> Ghosts { get; set; } = new();

    public int Width { get; set; }
    public int Height { get; set; }
}
