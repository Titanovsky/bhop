using Sandbox;

public sealed class TriggerTeleport : Component, Component.ITriggerListener
{
	[Property] public Transform transform;
	[Property] public bool resetVelocity = true;

	public void OnTriggerEnter( Collider other )
	{
		if ( !transform.IsValid ) return;

		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		ply.Teleport( transform, resetVelocity );
	}
}
