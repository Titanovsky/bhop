using Sandbox;
using System.Xml.Linq;

public sealed class Map : Component
{
    [Property] public MapInstance MapInstance { get; set; } 
    [Property] public bool RemoveLightEnv { get; set; } = true;
    [Property] public bool RemoveEnvSky { get; set; } = false;

    private GameObject GetEntity(string name)
    {
        if (MapInstance == null) return null;

        GameObject ent = MapInstance.GameObject.Children.First(gameObj => gameObj.Name.StartsWith(name));

        return ent;
    }

    private void DisableSourceMapEntity(GameObject obj = null)
    {
        if (!obj.IsValid()) return;

        obj.Enabled = false;

        Log.Info($"[Player] Disable {obj} on {MapInstance.MapName}");
    }

    protected override void OnStart()
    {
        if (RemoveLightEnv)
            DisableSourceMapEntity(GetEntity("light_environment"));

        if (RemoveEnvSky)
            DisableSourceMapEntity(GetEntity("env_sky"));
    }
}
