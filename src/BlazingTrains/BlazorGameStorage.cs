using Blazored.LocalStorage;
using Trains.NET.Engine;

namespace BlazingTrains;

public class BlazorGameStorage : IGameStorage
{
    public IServiceProvider? AspNetCoreServices { get; set; }

    private ISyncLocalStorageService? LocalStorageService => this.AspNetCoreServices?.GetService<ISyncLocalStorageService>();

    public IEnumerable<IEntity> ReadEntities()
    {
        if (this.LocalStorageService is null) yield break;

        var entities = this.LocalStorageService.GetItem<List<IEntity>>("Entities");
       // return entities;
        foreach (var entity in entities)
        {
            yield return entity;
        }
    }

    public IEnumerable<Terrain> ReadTerrain()
    {
        if (this.LocalStorageService is null) yield break;

        var terrainList = this.LocalStorageService.GetItem<Terrain[]>("Terrain");
        foreach (var terrain in terrainList)
        {
            yield return terrain;
        }
    }

    public void WriteEntities(IEnumerable<IEntity> entities)
    {
        if (this.LocalStorageService is null) return;

        _ = this.LocalStorageService.SetItem("Entities", entities.ToArray());
    }

    public void WriteTerrain(IEnumerable<Terrain> terrainList)
    {
        if (this.LocalStorageService is null) return;

        _ = this.LocalStorageService.SetItem("Terrain", terrainList.ToArray());
    }
}
