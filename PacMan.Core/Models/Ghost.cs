namespace PacMan.Core.Models;

public class Ghost : Entity
{
    public string Name { get; set; } = string.Empty;
    public int SpawnX { get; set; }
    public int SpawnY { get; set; }
}
