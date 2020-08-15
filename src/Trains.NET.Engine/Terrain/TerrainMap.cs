﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Trains.NET.Engine
{
    public class TerrainMap : ITerrainMap
    {
        private ImmutableDictionary<(int, int), Terrain> _terrainMap = ImmutableDictionary<(int, int), Terrain>.Empty;

        public void SetTerrainHeight(int column, int row, int height)
        {
            GetOrAdd(column, row).Height = height;
        }

        public void SetTerrainType(int column, int row, TerrainType type)
        {
            GetOrAdd(column, row).TerrainType = type;
        }

        public void Set(IEnumerable<Terrain> terrainList)
        {
            _terrainMap = terrainList.ToImmutableDictionary(t => (t.Column, t.Row));
        }

        private Terrain GetOrAdd(int column, int row)
        {
            return ImmutableInterlocked.GetOrAdd(ref _terrainMap, (column, row), key => new Terrain { Column = key.Item1, Row = key.Item2 });
        }

        public IEnumerator<Terrain> GetEnumerator()
        {
            return _terrainMap.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Terrain GetAdjacentTerrainUp(Terrain terrain)
        {
            return GetTerrainOrDefault(terrain.Column, terrain.Row - 1);
        }

         public Terrain GetAdjacentTerrainDown(Terrain terrain)
        {
            return GetTerrainOrDefault(terrain.Column, terrain.Row + 1);
        }

        public Terrain GetAdjacentTerrainLeft(Terrain terrain)
        {
            return GetTerrainOrDefault(terrain.Column - 1, terrain.Row);
        }

        public Terrain GetAdjacentTerrainRight(Terrain terrain)
        {
            return GetTerrainOrDefault(terrain.Column + 1, terrain.Row);
        }

        private Terrain GetTerrainOrDefault(int column, int row)
        {
            if (_terrainMap.TryGetValue((column, row), out Terrain? adjacentTerrain))
            {
                return adjacentTerrain;
            }

            return new Terrain
            {
                Row = row,
                Column = column,
                Height = 0
            };
        }
    }
}
