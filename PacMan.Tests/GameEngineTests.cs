using System;
using System.Collections.Generic;
using PacMan.Core;
using PacMan.Core.Enums;
using PacMan.Core.Models;
using Xunit;

namespace PacMan.Tests;

public class GameEngineTests
{
    [Fact]
    public void MovePlayer_Should_Not_Move_Into_Wall()
    {
        var engine = new GameEngine(1, 1);

        var (path, wallDir) = FindPathToAdjacentWall(engine.Map.Tiles, engine.Player.X, engine.Player.Y);
        MoveAlong(engine, path);

        var beforeX = engine.Player.X;
        var beforeY = engine.Player.Y;

        engine.MovePlayer(wallDir);

        Assert.Equal(beforeX, engine.Player.X);
        Assert.Equal(beforeY, engine.Player.Y);
    }

    [Fact]
    public void MovePlayer_Should_Consume_Pellet_And_Increase_Score()
    {
        var engine = new GameEngine(1, 1);

        var path = FindPathToTileType(engine.Map.Tiles, engine.Player.X, engine.Player.Y, TileType.Pellet, out var target);
        Assert.NotEmpty(path);

        int expectedScore = 0;
        int x = engine.Player.X;
        int y = engine.Player.Y;

        foreach (var dir in path)
        {
            var next = NextPos(x, y, dir);
            var tile = engine.Map.Tiles[next.y, next.x];

            if (tile == TileType.Pellet) expectedScore += 10;
            else if (tile == TileType.PowerPellet) expectedScore += 50;

            engine.MovePlayer(dir);

            Assert.Equal(expectedScore, engine.Player.Score);

            if (tile == TileType.Pellet || tile == TileType.PowerPellet)
            {
                Assert.Equal(TileType.Path, engine.Map.Tiles[next.y, next.x]);
            }

            x = engine.Player.X;
            y = engine.Player.Y;
        }

        Assert.Equal(target.x, engine.Player.X);
        Assert.Equal(target.y, engine.Player.Y);
        Assert.Equal(TileType.Path, engine.Map.Tiles[target.y, target.x]);
        Assert.True(expectedScore >= 10);
    }

    [Fact]
    public void MovePlayer_Should_Consume_PowerPellet_And_Increase_Score()
    {
        var engine = new GameEngine(1, 1);

        var path = FindPathToTileType(engine.Map.Tiles, engine.Player.X, engine.Player.Y, TileType.PowerPellet, out var target);
        Assert.NotEmpty(path);
        Assert.Equal(TileType.PowerPellet, engine.Map.Tiles[target.y, target.x]);

        int expectedScore = 0;
        int x = engine.Player.X;
        int y = engine.Player.Y;

        foreach (var dir in path)
        {
            var next = NextPos(x, y, dir);
            var tile = engine.Map.Tiles[next.y, next.x];

            if (tile == TileType.Pellet) expectedScore += 10;
            else if (tile == TileType.PowerPellet) expectedScore += 50;

            engine.MovePlayer(dir);

            Assert.Equal(expectedScore, engine.Player.Score);

            if (tile == TileType.Pellet || tile == TileType.PowerPellet)
            {
                Assert.Equal(TileType.Path, engine.Map.Tiles[next.y, next.x]);
            }

            x = engine.Player.X;
            y = engine.Player.Y;
        }

        Assert.Equal(target.x, engine.Player.X);
        Assert.Equal(target.y, engine.Player.Y);
        Assert.Equal(TileType.Path, engine.Map.Tiles[target.y, target.x]);
        Assert.True(expectedScore >= 50);
    }

    [Fact]
    public void MovePlayer_When_Colliding_With_Ghost_Should_Reset_To_Spawn()
    {
        var engine = new GameEngine(1, 1);
        var spawnX = engine.Player.X;
        var spawnY = engine.Player.Y;

        var path = FindPathToAnyOtherTile(engine.Map.Tiles, spawnX, spawnY);
        Assert.NotEmpty(path);

        MoveAlong(engine, path);
        Assert.False(engine.Player.X == spawnX && engine.Player.Y == spawnY);

        if (engine.Ghosts.Count == 0)
        {
            engine.Ghosts.Add(new Ghost());
        }

        engine.Ghosts[0].X = engine.Player.X;
        engine.Ghosts[0].Y = engine.Player.Y;

        engine.MovePlayer(Direction.Up);

        Assert.Equal(spawnX, engine.Player.X);
        Assert.Equal(spawnY, engine.Player.Y);
    }

    private static void MoveAlong(GameEngine engine, IEnumerable<Direction> path)
    {
        foreach (var dir in path)
        {
            engine.MovePlayer(dir);
        }
    }

    private static List<Direction> FindPathToAnyOtherTile(TileType[,] tiles, int startX, int startY)
    {
        return FindPath(tiles, startX, startY, (x, y) => !(x == startX && y == startY), out _);
    }

    private static List<Direction> FindPathToTileType(TileType[,] tiles, int startX, int startY, TileType targetType, out (int x, int y) target)
    {
        return FindPath(tiles, startX, startY, (x, y) => tiles[y, x] == targetType, out target);
    }

    private static (List<Direction> path, Direction wallDir) FindPathToAdjacentWall(TileType[,] tiles, int startX, int startY)
    {
        int height = tiles.GetLength(0);
        int width = tiles.GetLength(1);

        var visited = new bool[height, width];
        var prevX = new int[height, width];
        var prevY = new int[height, width];
        var prevDir = new Direction[height, width];

        var queue = new Queue<(int x, int y)>();
        visited[startY, startX] = true;
        queue.Enqueue((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            foreach (var dir in Directions)
            {
                var next = NextPos(x, y, dir);
                if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height)
                {
                    continue;
                }

                if (tiles[next.y, next.x] == TileType.Wall)
                {
                    var path = ReconstructPath(prevX, prevY, prevDir, startX, startY, x, y);
                    return (path, dir);
                }
            }

            foreach (var dir in Directions)
            {
                var next = NextPos(x, y, dir);
                if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height)
                {
                    continue;
                }

                if (visited[next.y, next.x] || tiles[next.y, next.x] == TileType.Wall)
                {
                    continue;
                }

                visited[next.y, next.x] = true;
                prevX[next.y, next.x] = x;
                prevY[next.y, next.x] = y;
                prevDir[next.y, next.x] = dir;
                queue.Enqueue((next.x, next.y));
            }
        }

        throw new InvalidOperationException("No wall-adjacent position found.");
    }

    private static List<Direction> FindPath(TileType[,] tiles, int startX, int startY, Func<int, int, bool> isTarget, out (int x, int y) target)
    {
        int height = tiles.GetLength(0);
        int width = tiles.GetLength(1);

        var visited = new bool[height, width];
        var prevX = new int[height, width];
        var prevY = new int[height, width];
        var prevDir = new Direction[height, width];

        var queue = new Queue<(int x, int y)>();
        visited[startY, startX] = true;
        queue.Enqueue((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            if (isTarget(x, y))
            {
                target = (x, y);
                return ReconstructPath(prevX, prevY, prevDir, startX, startY, x, y);
            }

            foreach (var dir in Directions)
            {
                var next = NextPos(x, y, dir);
                if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height)
                {
                    continue;
                }

                if (visited[next.y, next.x] || tiles[next.y, next.x] == TileType.Wall)
                {
                    continue;
                }

                visited[next.y, next.x] = true;
                prevX[next.y, next.x] = x;
                prevY[next.y, next.x] = y;
                prevDir[next.y, next.x] = dir;
                queue.Enqueue((next.x, next.y));
            }
        }

        throw new InvalidOperationException("Target not found.");
    }

    private static List<Direction> ReconstructPath(int[,] prevX, int[,] prevY, Direction[,] prevDir, int startX, int startY, int targetX, int targetY)
    {
        var path = new List<Direction>();
        int cx = targetX;
        int cy = targetY;

        while (!(cx == startX && cy == startY))
        {
            var dir = prevDir[cy, cx];
            path.Add(dir);
            int px = prevX[cy, cx];
            int py = prevY[cy, cx];
            cx = px;
            cy = py;
        }

        path.Reverse();
        return path;
    }

    private static (int x, int y) NextPos(int x, int y, Direction dir)
    {
        return dir switch
        {
            Direction.Up => (x, y - 1),
            Direction.Down => (x, y + 1),
            Direction.Left => (x - 1, y),
            Direction.Right => (x + 1, y),
            _ => (x, y)
        };
    }

    private static readonly Direction[] Directions =
    {
        Direction.Up,
        Direction.Down,
        Direction.Left,
        Direction.Right
    };
}
