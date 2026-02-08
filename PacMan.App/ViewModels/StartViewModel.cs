using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PacMan.App.Services;
using PacMan.Core.Models;

namespace PacMan.App.ViewModels;

public class StartViewModel : INotifyPropertyChanged
{
    private readonly HighScoreService _highScoreService;
    private bool _isMuted;

    public ObservableCollection<ScoreEntry> HighScores { get; } = new();
    public ICommand ToggleMuteCommand { get; }

    public StartViewModel()
    {
        _highScoreService = new HighScoreService();
        ToggleMuteCommand = new RelayCommand(_ => ToggleMute());
        _highScoreService.ResetAllOnce();
        LoadHighScores();
        IsMuted = AudioService.Instance.IsMuted;
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (_isMuted != value)
            {
                _isMuted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MuteLabel));
            }
        }
    }

    public string MuteLabel => IsMuted ? "Unmute" : "Mute";

    public void LoadHighScores()
    {
        HighScores.Clear();
        var scores = _highScoreService.Load();
        foreach (var s in scores)
        {
            HighScores.Add(s);
        }
    }

    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        AudioService.Instance.SetMuted(IsMuted);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
