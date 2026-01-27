using PacMan.Core.Enums;

namespace PacMan.Core;

public class GameEngine
{
    public int PacManX { get; private set; }
    public int PacManY { get; private set; }

    public void MovePacMan(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                PacManY--;
                break;
            case Direction.Down:
                PacManY++;
                break;
            case Direction.Left:
                PacManX--;
                break;
            case Direction.Right:
                PacManX++;
                break;
        }
    }
}
