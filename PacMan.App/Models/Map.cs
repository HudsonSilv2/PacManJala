namespace PacMan.App.Models;

public class Map
{
    public TileType[,] Tiles { get; }

    public int Width => Tiles.GetLength(1);
    public int Height => Tiles.GetLength(0);

    public Map(TileType[,] tiles)
    {
        Tiles = tiles;
    }
}
