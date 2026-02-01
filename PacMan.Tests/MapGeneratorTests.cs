using PacMan.Core.Services;
using PacMan.Core.Models;
using Xunit;

namespace PacMan.Tests;

/// <summary>
/// Testes básicos da geração de mapa.
/// </summary>
public class MapGeneratorTests
{
    [Fact]
    public void Generate_Should_Create_Map_With_Border_Walls()
    {
        var generator = new MapGenerator();

        var result = generator.Generate(10, 10);

        // Verifica paredes superiores e inferiores
        for (int x = 0; x < result.Width; x++)
        {
            Assert.Equal(TileType.Wall, result.Tiles[0, x]);
            Assert.Equal(TileType.Wall, result.Tiles[result.Height - 1, x]);
        }

        // Verifica paredes laterais
        for (int y = 0; y < result.Height; y++)
        {
            Assert.Equal(TileType.Wall, result.Tiles[y, 0]);
            Assert.Equal(TileType.Wall, result.Tiles[y, result.Width - 1]);
        }
    }

    [Fact]
    public void Generate_Should_Create_Player_Spawn()
    {
        var generator = new MapGenerator();

        var result = generator.Generate(10, 10);

        Assert.True(result.PlayerSpawn.X > 0);
        Assert.True(result.PlayerSpawn.Y > 0);
    }
}
