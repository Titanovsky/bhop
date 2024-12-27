using Sandbox;

public sealed class TriggerCheckpoint : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		ply.SetupCheckpoint(GameObject);
	}
}
