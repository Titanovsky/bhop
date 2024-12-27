using Sandbox;

public sealed class TriggerRespawn : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		Log.Error( "Trigger entered with: " + other.GameObject );
	}
}
