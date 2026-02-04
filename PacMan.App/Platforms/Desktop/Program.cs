using System;
using Microsoft.UI.Xaml;
using Uno.UI.Runtime.Skia.Gtk;

namespace PacMan.App.Desktop;

public class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var host = new GtkHost(() => new PacMan.App.App());
        
        host.Run();
    }
}
