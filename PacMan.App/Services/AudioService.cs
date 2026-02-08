using System;
using System.Diagnostics;
using System.IO;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace PacMan.App.Services;

public sealed class AudioService
{
    private static readonly Lazy<AudioService> _instance = new(() => new AudioService());
    public static AudioService Instance => _instance.Value;

    private readonly MediaPlayer _sfxPlayer = new();
    private readonly MediaPlayer _musicPlayer = new();
    private bool _dotToggle;

    private AudioService()
    {
        _sfxPlayer.Volume = 1.0;
        _musicPlayer.Volume = 1.0;
    }

    public bool IsMuted { get; private set; }

    public void SetMuted(bool muted)
    {
        IsMuted = muted;
        var volume = muted ? 0.0 : 1.0;
        _sfxPlayer.Volume = volume;
        _musicPlayer.Volume = volume;

        if (muted)
        {
            _sfxPlayer.Pause();
            _musicPlayer.Pause();
        }
    }

    // funções simples e facil de lembrar, hudson.
    public void PlayStart()
        => PlaySfx("Assets/sounds/music/start.wav");

    public void PlayEatDot()
    {
        var name = _dotToggle ? "eat_dot_0.wav" : "eat_dot_1.wav";
        _dotToggle = !_dotToggle;
        PlaySfx($"Assets/sounds/sfx/{name}");
    }

    public void PlayEatFruit()
        => PlaySfx("Assets/sounds/sfx/eat_fruit.wav");

    public void PlayEatGhost()
        => PlaySfx("Assets/sounds/sfx/eat_ghost.wav");

    public void PlayDeath()
        => PlaySfx("Assets/sounds/sfx/death_0.wav");

    public void PlayGameOver()
        => PlaySfx("Assets/sounds/sfx/death_1.wav");

    public void PlayWin()
        => PlayMusic("Assets/sounds/music/intermission.wav");

    private void PlaySfx(string assetPath)
    {
        if (IsMuted) return;
        if (OperatingSystem.IsLinux())
        {
            TryPlayWav(GetAssetFilePath(assetPath));
            return;
        }

        _sfxPlayer.Source = MediaSource.CreateFromUri(ToMsAppxUri(assetPath));
        _sfxPlayer.Play();
    }

    private void PlayMusic(string assetPath)
    {
        if (IsMuted) return;
        if (OperatingSystem.IsLinux())
        {
            TryPlayWav(GetAssetFilePath(assetPath));
            return;
        }

        _musicPlayer.Source = MediaSource.CreateFromUri(ToMsAppxUri(assetPath));
        _musicPlayer.Play();
    }

    private static Uri ToMsAppxUri(string assetPath)
    {
        var normalized = assetPath.Replace('\\', '/');
        return new Uri($"ms-appx:///{normalized}");
    }

    private static string GetAssetFilePath(string assetPath)
    {
        var normalized = assetPath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(AppContext.BaseDirectory, normalized);
    }

    private static void TryPlayWav(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "aplay",
                Arguments = $"-q \"{path}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }
        catch
        {
            // Ignore audio errors on Linux if aplay isn't available.
        }
    }
}
