using Microsoft.UI.Xaml.Data;
using System;

namespace PacMan.App.Converters;    

public class PositionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is int v && int.TryParse(parameter?.ToString(), out var size)
            ? v * size
            : 0;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
