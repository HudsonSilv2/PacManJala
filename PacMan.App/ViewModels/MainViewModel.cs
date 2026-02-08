using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PacMan.App.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private bool _isInGame;

    public bool IsInGame
    {
        get => _isInGame;
        set
        {
            if (_isInGame != value)
            {
                _isInGame = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOnStart));
            }
        }
    }

    public bool IsOnStart => !IsInGame;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
