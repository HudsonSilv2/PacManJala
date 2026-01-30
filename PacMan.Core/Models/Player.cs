namespace PacMan.Core.Models;

public class Player : Entity //Defalt por padrão a partir de Entity
{
    public int Lives { get; set; } = 3; // 3 vidas como o original
    public int Score { get; set; } = 0; //pontuação inicial é 0
}
