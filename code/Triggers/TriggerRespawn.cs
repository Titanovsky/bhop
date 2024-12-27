using Sandbox;

public sealed class TriggerRespawn : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		if ( ply.State == PlayerStateEnum.Finished )
			ply.ResetProgress();

		ply.Respawn();
	}
}
