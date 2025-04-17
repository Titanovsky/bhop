using Sandbox;

public sealed class TriggerSegment : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		ply.SetupCheckpoint(GameObject);

		Log.Info( "dsa" );
	}
}
