using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PacMan.Core.Models;

namespace PacMan.App.Selectors;

public class GameObjectTemplateSelector : DataTemplateSelector
{
    public DataTemplate PlayerTemplate { get; set; }
    public DataTemplate GhostTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
        => item switch
        {
            Player => PlayerTemplate,
            Ghost => GhostTemplate,
            _ => base.SelectTemplateCore(item)
        };
}
