using Blazored.LocalStorage;
using Trains.NET.Engine;

namespace BlazingTrains;

public class BlazorGameStorage : IGameStorage
{
    public IServiceProvider? AspNetCoreServices { get; set; }

    private ISyncLocalStorageService? ISyncLocalStorageService => this.AspNetCoreServices?.GetService<ISyncLocalStorageService>();

    public IEnumerable<IEntity> ReadEntities()
    {
        if (this.ISyncLocalStorageService is null) yield break;

        var entities = this.ISyncLocalStorageService.GetItem<List<IEntity>>("Entities");
       // return entities;
        foreach (var entity in entities)
        {
            yield return entity;
        }
    }

    public IEnumerable<Terrain> ReadTerrain()
    {
        if (this.LocalStorageService is null) yield break;

        var terrainList = this.LocalStorageService.GetItemAsync<Terrain[]>("Terrain").GetAwaiter().GetResult();
        foreach (var terrain in terrainList)
        {
            yield return terrain;
        }
    }

    public void WriteEntities(IEnumerable<IEntity> entities)
    {
        if (this.LocalStorageService is null) return;

        _ = this.LocalStorageService.SetItemAsync("Entities", entities.ToArray());
    }

    public void WriteTerrain(IEnumerable<Terrain> terrainList)
    {
        if (this.LocalStorageService is null) return;

        _ = this.LocalStorageService.SetItemAsync("Terrain", terrainList.ToArray());
    }
}
