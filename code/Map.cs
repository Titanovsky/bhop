using Sandbox;
using System.Xml.Linq;

public sealed class Map : Component
{
    [Property] public MapInstance MapInstance { get; set; } 
    [Property] public GameObject LightEnvironment { get; set; } // disable light because march 2025 update (new bloom and postproccesing)
    [Property] public GameObject EnvSky { get; set; } // disable on christmas 2025
    [Property] public bool RemoveLightEnv { get; set; } = true;
    [Property] public bool RemoveEnvSky { get; set; } = false;

    private void DisableSourceMapEntity(GameObject obj)
    {
        //GameObject ent = map.GameObject.Children.Where((gameObj) => gameObj.Name == name).First();
        GameObject ent = obj;
        if (!ent.IsValid()) return;

        ent.Enabled = false;

        Log.Info($"[Player] Disable {obj} on {MapInstance.MapName}");
    }

    protected override void OnStart()
    {
        if (RemoveLightEnv)
            DisableSourceMapEntity(LightEnvironment);

        if (RemoveEnvSky)
            DisableSourceMapEntity(EnvSky);
    }
}
