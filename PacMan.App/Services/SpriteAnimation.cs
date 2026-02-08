using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PacMan.App.Services;

/// <summary>
/// Sprite animation helper based on the user's own game prototype (Python version).
/// It advances the frame every N ticks and returns the current frame image.
/// </summary>
public sealed class SpriteAnimation
{
    private readonly List<BitmapImage> _frames;
    private int _index;
    private int _timer;
    private readonly int _ticksPerFrame;

    /// <param name="basePath">
    /// Path under Assets without extension.
    /// Example: "hero/idle_" (will resolve to ms-appx:///Assets/hero/idle_0.png ...).
    /// For a single frame: "hero/idle".
    /// </param>
    /// <param name="frames">Number of frames in the animation.</param>
    /// <param name="ticksPerFrame">How many update ticks before advancing a frame.</param>
    public SpriteAnimation(string basePath, int frames, int ticksPerFrame = 10)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException("Base path cannot be empty.", nameof(basePath));
        }

        if (frames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frames), "Frames must be >= 1.");
        }

        _ticksPerFrame = Math.Max(1, ticksPerFrame);

        if (frames > 1)
        {
            _frames = new List<BitmapImage>(frames);
            for (int i = 0; i < frames; i++)
            {
                var uri = new Uri($"ms-appx:///Assets/{basePath}{i}.png");
                _frames.Add(new BitmapImage(uri));
            }
        }
        else
        {
            var uri = new Uri($"ms-appx:///Assets/{basePath}.png");
            _frames = new List<BitmapImage> { new BitmapImage(uri) };
        }
    }

    public bool Update()
    {
        _timer++;
        if (_timer >= _ticksPerFrame)
        {
            _timer = 0;
            _index = (_index + 1) % _frames.Count;
            return true;
        }

        return false;
    }

    public void Reset()
    {
        _index = 0;
        _timer = 0;
    }

    public BitmapImage GetFrameImage()
    {
        return _frames[_index];
    }
}
