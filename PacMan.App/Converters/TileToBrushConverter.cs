using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PacMan.Core.Models;
using System;

namespace PacMan.App.Converters;

public class TileToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is TileType t
            ? t switch
            {
                TileType.Wall => new SolidColorBrush(Colors.Blue),
                TileType.Pellet => new SolidColorBrush(Colors.White),
                TileType.PowerPellet => new SolidColorBrush(Colors.Yellow),
                _ => new SolidColorBrush(Colors.Black)
            }
            : new SolidColorBrush(Colors.Black);

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
