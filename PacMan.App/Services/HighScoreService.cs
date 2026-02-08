using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PacMan.Core.Models;
using Windows.Storage;

namespace PacMan.App.Services;

public class HighScoreService
{
    private const string FileName = "highscores.json";
    private const string ResetFlagKey = "HighScoresResetKeepPacMan_v1";
    private readonly string _filePath;

    public HighScoreService()
    {
        var folder = ApplicationData.Current.LocalFolder.Path;
        _filePath = Path.Combine(folder, FileName);
    }

    public List<ScoreEntry> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new List<ScoreEntry>();
        }

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<ScoreEntry>();
        }

        return JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
    }

    public void ResetKeepPacManOnce()
    {
        var settings = ApplicationData.Current.LocalSettings;
        if (settings.Values.ContainsKey(ResetFlagKey))
        {
            return;
        }

        var scores = Load();
        var filtered = scores
            .Where(s => string.Equals(s.PlayerName, "PAC MAN", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (filtered.Count == 0)
        {
            filtered.Add(new ScoreEntry { PlayerName = "PAC MAN", Score = 5000 });
        }

        Save(filtered);
        settings.Values[ResetFlagKey] = true;
    }

    public void Save(IEnumerable<ScoreEntry> scores)
    {
        var ordered = scores
            .OrderByDescending(s => s.Score)
            .ToList();

        var json = JsonSerializer.Serialize(ordered, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
    }
}
