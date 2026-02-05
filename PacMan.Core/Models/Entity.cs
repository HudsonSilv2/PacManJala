using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PacMan.Core.Models;

public abstract class Entity : INotifyPropertyChanged
{
    private int _x;
    public int X
    {
        get => _x;
        set { _x = value; OnPropertyChanged(); }
    }

    private int _y;
    public int Y
    {
        get => _y;
        set { _y = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

}
