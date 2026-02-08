# PacMan

Recriação do jogo Pac-Man utilizando C#, Uno Platform e o padrão MVVM.

**Environment**
- .NET SDK 8.x
- Uno Platform templates: `dotnet new install Uno.ProjectTemplates.Dotnet`
- Windows desktop: .NET Desktop Runtime 8 (x64), MSYS2, GTK 3, PATH com `C:\msys64\mingw64\bin`
- Linux desktop: GTK 3 (`sudo apt install libgtk-3-dev`)

**Estrutura do Projeto**
```
PacMan/
├── PacMan.App/       # Interface gráfica (Uno Platform + XAML)
├── PacMan.Core/      # Domínio, regras do jogo e entidades
└── PacMan.Tests/     # Testes unitários (xUnit)
```

**Como Executar (desktop)**
- `dotnet clean`
- `dotnet run -f net8.0-desktop`
