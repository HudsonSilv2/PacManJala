namespace PacMan.Core.Models;

/// <summary>
/// Representa uma posição no mapa (grid).
/// Criado para evitar uso excessivo de tuplas.
/// </summary>
public struct Position
{
    public int X { get; }
    public int Y { get; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    
}
