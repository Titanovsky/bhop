using Sandbox;

public sealed class TriggerRespawn : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		ply.Respawn();
	}
}
