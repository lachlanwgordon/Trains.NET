﻿using System;
using System.Collections.Generic;
using Trains.NET.Engine;
using Trains.NET.Engine.Tracks;
using Trains.NET.Rendering;
using Xunit;
using Xunit.Abstractions;

namespace Trains.NET.Tests;

public class TestBase : IDisposable
{
    private int _lastCol;
    private int _lastRow;

    private readonly ITestOutputHelper _output;
    internal readonly IGameStorage Storage;
    internal readonly TestTimer Timer;
    internal readonly GameBoard GameBoard;
    internal readonly ILayout TrackLayout;
    internal readonly ITerrainMap TerrainMap;
    internal readonly ILayout<Track> FilteredLayout;
    internal readonly TrackTool TrackTool;

    protected TestBase(ITestOutputHelper output)
    {
        Storage = new NullStorage();
        Timer = new TestTimer();
        TrackLayout = new Layout();
        TerrainMap = new TerrainMap();
        TerrainMap.Reset(1, 100, 100);
        GameBoard = new GameBoard(TrackLayout, TerrainMap, Storage, Timer);

        FilteredLayout = new FilteredLayout<Track>(TrackLayout);

        var entityFactories = new List<IStaticEntityFactory<Track>>
            {
                new CrossTrackFactory(TerrainMap, TrackLayout),
                new TIntersectionFactory(TerrainMap, TrackLayout),
                new BridgeFactory(TerrainMap, FilteredLayout),
                new SingleTrackFactory(TerrainMap, FilteredLayout)
            };

        TrackTool = new TrackTool(FilteredLayout, entityFactories);

        _output = output;
    }

    protected void StartDrawTrack(int startColumn, int startRow)
    {
        _lastCol = startColumn;
        _lastRow = startRow;
        TrackTool.Execute(startColumn, startRow, new ExecuteInfo(0, 0));
    }

    protected void DrawTrack(DrawDirection step)
    {
        var (nextCol, nextRow) = step switch
        {
            DrawDirection.Up => (_lastCol, _lastRow - 1),
            DrawDirection.Down => (_lastCol, _lastRow + 1),
            DrawDirection.Left => (_lastCol - 1, _lastRow),
            DrawDirection.Right => (_lastCol + 1, _lastRow),
            _ => throw new InvalidOperationException()
        };

        TrackTool.Execute(nextCol, nextRow, new ExecuteInfo(_lastCol, _lastRow));
        _lastCol = nextCol;
        _lastRow = nextRow;
    }

    protected void FlattenTerrain()
    {
        List<Terrain> terrain = new();
        for (int c = 0; c < 100; c++)
        {
            for (int r = 0; r < 100; r++)
            {
                terrain.Add(new Terrain
                {
                    Column = c,
                    Row = r,
                    Height = Terrain.FirstLandHeight
                });
            }
        }
        TerrainMap.Set(terrain);
    }

    protected void AssertTrainMovement(float startAngle, int startColumn, int startRow, int endColumn, int endRow)
    {
        var train = GameBoard.AddTrain(startColumn, startRow) as Train;

        train!.LookaheadDistance = 0.1f;
        train.SetAngle(startAngle);

        _output.WriteLine("Initial: " + train);

        for (int i = 0; i < 100; i++)
        {
            Timer.Tick();
            _output.WriteLine($"Tick {i}: {train}");
        }

        Assert.Equal((endColumn, endRow), (train.Column, train.Row));
    }

    public void Dispose()
    {
        Timer.Dispose();
        GameBoard.Dispose();
    }
}
