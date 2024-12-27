using Sandbox;

public sealed class TriggerFinish : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		ply.FinishProgress();
	}
}
