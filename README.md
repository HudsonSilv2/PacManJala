# PacMan

Recriação do Pac‑Man em C# usando Uno Platform (UI em XAML) e MVVM. O projeto está dividido em três camadas: app (UI), core (regras do jogo) e testes.

**Stack**
- .NET 8
- Uno Platform (WinUI/Skia)
- MVVM (ViewModels próprios)
- xUnit (testes)

**Estrutura**
```
PacMan.App/     UI e integração com o usuário (XAML + code-behind)
PacMan.Core/    Motor do jogo, mapa e entidades
PacMan.Tests/   Testes unitários do core
```

## Como executar
**Pré‑requisitos**
- .NET SDK 8.x
- Uno Platform templates: `dotnet new install Uno.ProjectTemplates.Dotnet`
- Desktop Windows: .NET Desktop Runtime 8 (x64), MSYS2, GTK 3, PATH com `C:\msys64\mingw64\bin`
- Desktop Linux: GTK 3 (`sudo apt install libgtk-3-dev`)

**Rodar no desktop**
```
dotnet clean

dotnet run -f net8.0-desktop --project PacMan.App/PacMan.App.csproj
```

## Controles
- Setas: movimentação
- `Enter`: iniciar jogo (na tela inicial)
- `P`: pausar
- `R`: reiniciar
- `ESC`: sair

## Visão geral do funcionamento
O fluxo principal é: **UI → ViewModel → GameEngine → UI**. A UI captura entrada e chama o `GameViewModel`, que delega regras ao `GameEngine` e depois atualiza o desenho do mapa e sprites.

### 1) Motor do jogo (PacMan.Core)
**Arquivo principal:** `PacMan.Core/GameEngine.cs`

Responsável por:
- Gerar mapa e entidades iniciais
- Movimento do jogador e fantasmas
- Colisões
- Pontuação, vidas e power‑up

Fluxo simplificado:
1. `GameEngine` cria o mapa via `MapGenerator`.
2. O jogador e os fantasmas são posicionados.
3. Cada movimento do jogador chama `MovePlayer(...)`.
4. A cada 3 movimentos do jogador, os fantasmas se movem aleatoriamente.
5. O motor detecta colisões, aplica score e controla fim de jogo.

**Regras principais (GameEngine)**
- **Movimento**: `TryMove` bloqueia paredes e limites do mapa.
- **Pastilhas**: `Pellet` (+10) e `PowerPellet` (+50) são removidas do mapa ao serem consumidas.
- **Power‑up**: ao comer `PowerPellet`, o jogador fica fortalecido por 50 ticks.
- **Colisão com fantasmas**:
  - Se estiver powered‑up, fantasma volta ao spawn e o jogador ganha +200.
  - Caso contrário, perde uma vida e volta ao spawn.

**Entidades e tipos**
- `PacMan.Core/Models/Entity.cs` (base com X/Y e notificação)
- `PacMan.Core/Models/Player.cs`
- `PacMan.Core/Models/Ghost.cs`
- `PacMan.Core/Models/Map.cs` e `TileType.cs`
- `PacMan.Core/Enums/Direction.cs`

### 2) Geração do mapa
**Arquivo:** `PacMan.Core/Services/MapGenerator.cs`

- Usa um layout fixo (ASCII) para gerar as paredes, pellets e power pellets.
- Identifica a posição do jogador (`P`) e cria até 4 fantasmas nas posições de espaço (`' '`).
- Retorna um `GeneratedMap` com tiles e spawns.

### 3) Camada de UI (PacMan.App)
**Páginas principais**
- `PacMan.App/MainPage.xaml` alterna entre a tela inicial e o jogo.
- `PacMan.App/Views/StartPage.xaml` mostra o menu e ranking.
- `PacMan.App/Views/GamePage.xaml` desenha o jogo e HUD.

**ViewModels**
- `GameViewModel` controla o estado do jogo e chama o `GameEngine`.
- `StartViewModel` carrega ranking e controla o mute.
- `MainViewModel` gerencia se está no menu ou no jogo.

**Loop do jogo**
- Em `GamePage.xaml.cs`, um `DispatcherTimer` roda a cada 120ms.
- A cada tick, se houver direção, chama `GameViewModel.MovePlayer(...)`.
- O canvas é redesenhado com as posições atualizadas.

**Sprites e animações**
- `SpriteAnimation` troca frames a cada N ticks.
- Pac‑Man e fantasmas usam sprites em `Assets/sprites/32x32`.
- Fantasmas ficam “vulneráveis” quando o jogador está powered‑up.

**Áudio**
- `AudioService` toca efeitos e músicas.
- Em Linux, tenta tocar `.wav` via `aplay`.

### 4) Ranking (High Scores)
**Arquivo:** `PacMan.App/Services/HighScoreService.cs`

- Persiste `highscores.json` em `ApplicationData.Current.LocalFolder`.
- Mantém top 10 ordenado por pontuação.

### 5) Testes
**Arquivos:**
- `PacMan.Tests/GameEngineTests.cs`
- `PacMan.Tests/MapGeneratorTests.cs`

Cobrem:
- Movimento e bloqueio por parede
- Consumo de pellets e power‑pellets
- Colisão com fantasmas
- Estrutura básica do mapa

Executar testes:
```
dotnet test PacMan.Tests/PacMan.Tests.csproj
```

## Pontos principais para apresentar
- **Separação de camadas**: `Core` com regras puras e `App` com UI.
- **Mapeamento de tiles**: string ASCII → `TileType`.
- **Loop simples e eficiente**: `DispatcherTimer` + atualizações incrementais do canvas.
- **MVVM**: UI observa propriedades do ViewModel com `INotifyPropertyChanged`.
- **Persistência de ranking** com JSON local.
