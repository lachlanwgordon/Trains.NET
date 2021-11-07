using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trains.NET.Instrumentation;

namespace Trains.NET.Engine;

// Ensure this gets initialized last
[Order(999999)]
public class GameBoard : IGameBoard, IInitializeAsync
{
    private readonly ElapsedMillisecondsTimedStat _gameUpdateTime = InstrumentationBag.Add<ElapsedMillisecondsTimedStat>("Game-LoopStepTime");

    private const int GameLoopInterval = 16;

    private readonly ITimer _gameLoopTimer;
    private readonly IGameStateManager _gameStateManager;
    private readonly IEnumerable<IGameStep> _gameSteps;

    public bool Enabled { get; set; } = true;

    public GameBoard(IEnumerable<IGameStep> gameSteps, IGameStateManager gameStateManager, ITimer timer)
    {
        _gameLoopTimer = timer;
        _gameSteps = gameSteps;
        _gameStateManager = gameStateManager;

        _gameLoopTimer.Interval = GameLoopInterval;
        _gameLoopTimer.Elapsed += GameLoopTimerElapsed;
    }

    public Task InitializeAsync(int columns, int rows)
    {
        _columns = columns;
        _rows = rows;

        IEnumerable<IEntity>? entities = null;
        IEnumerable<IStaticEntity>? tracks = null;
        IEnumerable<Terrain>? terrain = null;
        IEnumerable<IMovable>? trains = null;
        try
        {
            entities = _storage?.ReadEntities();
            terrain = _storage?.ReadTerrain();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load \n{ex}\n{ex.StackTrace}");
        }
        if (entities is not null)
        {
            tracks = entities.OfType<IStaticEntity>();
            trains = entities.OfType<IMovable>();
        }
        _gameStateManager.Load();
        _gameLoopTimer.Start();

        return Task.CompletedTask;
    }

    public void GameLoopStep()
    {
        if (!this.Enabled) return;

        using (_gameUpdateTime.Measure())
        {
            var timeSinceLastTick = _gameLoopTimer?.TimeSinceLastTick ?? 16;
            foreach (var gameStep in _gameSteps)
            {
                gameStep.Update(timeSinceLastTick);
            }
        }
    }

    private void GameLoopTimerElapsed(object? sender, EventArgs e) => GameLoopStep();

    public void ClearAll()
        => _gameStateManager.Reset();

    public void Dispose()
    {
        _gameLoopTimer?.Dispose();
        Save();
    }

    public void Save()
    {
        if (_storage is not null)
        {
            _storage.WriteEntities(GetAllEntities());
            _storage.WriteTerrain(_terrainMap);
        }
    }

    private IEnumerable<IEntity> GetAllEntities()
    {
        foreach (var entity in _layout)
        {
            yield return entity;
        }
        foreach (var entity in _movables)
        {
            yield return entity;
        }
    }

    public IMovable? GetMovableAt(int column, int row)
    {
        foreach (var movable in _movables)
        {
            if (movable is not Train train)
            {
                continue;
            }

            if (train.Column == column && train.Row == row)
            {
                return train;
            }

            var fakeTrain = train.Clone();

            for (var i = 0; i < train.Carriages; i++)
            {
                var steps = GetNextSteps(fakeTrain, 1.0f);
                foreach (var step in steps)
                {
                    fakeTrain.ApplyStep(step);

                    if (fakeTrain.Column == column && fakeTrain.Row == row)
                    {
                        return train;
                    }
                }
            }
        }
        return null;
    }
}
