# PacMan

Recriação do jogo Pac-Man utilizando C#, Uno Platform e o padrão MVVM.

## Sobre o Projeto

Projeto final da disciplina de Programação III com foco em arquitetura limpa, separação de responsabilidades, testes unitários e documentação técnica.

## Tecnologias

- **C#** — Linguagem principal
- **Uno Platform** — Framework para aplicações multiplataforma
- **XAML** — Markup language para interface
- **MVVM** — Padrão de arquitetura
- **xUnit** — Framework de testes

## Status

🔧 Em desenvolvimento

---

## Começando

### Pré-requisitos

- .NET SDK 8.x
- Git (opcional)

### Instalação dos Templates do Uno Platform

```bash
dotnet new install Uno.ProjectTemplates.Dotnet
```

---

## Execução

### Windows

#### Requisitos Adicionais

- .NET Desktop Runtime 8 (x64)
- MSYS2
- GTK 3

#### Instalando GTK 3 no Windows

1. **Baixar e instalar o MSYS2**
   ```
   https://www.msys2.org
   ```

2. **Abrir o terminal MSYS2 MinGW64**

3. **Atualizar o sistema:**
   ```bash
   pacman -Syu
   ```

4. **Instalar GTK 3:**
   ```bash
   pacman -S mingw-w64-x86_64-gtk3
   ```

5. **Adicionar ao PATH do Windows:**
   ```
   C:\msys64\mingw64\bin
   ```

6. **Fechar e reabrir todos os terminais**

#### Executando o Projeto

No diretório `PacMan.App`:

```bash
dotnet clean
dotnet run -f net8.0-desktop
```

### Linux

#### Requisitos

- .NET SDK 8
- GTK 3

#### Instalando GTK 3

**Ubuntu/Debian:**
```bash
sudo apt install libgtk-3-dev
```

#### Executando o Projeto

```bash
dotnet clean
dotnet run -f net8.0-desktop
```

---

## Estrutura do Projeto

```
PacMan/
├── PacMan.App/       # Interface gráfica (Uno Platform + XAML)
├── PacMan.Core/      # Domínio, regras do jogo e entidades
└── PacMan.Tests/     # Testes unitários (xUnit)
```

- **PacMan.App** — Interface gráfica desenvolvida com Uno Platform e XAML
- **PacMan.Core** — Camada de domínio, regras do jogo e entidades
- **PacMan.Tests** — Testes unitários utilizando xUnit

---

## Objetivos do Projeto

- ✅ Arquitetura limpa
- ✅ Separação de responsabilidades
- ✅ Testes unitários
- ✅ Documentação técnica