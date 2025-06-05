public sealed class TriggerPunch : Component, Component.ITriggerListener
{
	[Property, Description("Can use as Direction, if the values will be only 0f - 1f, and you will manipulate with Force")] public Vector3 positionForPunch = Vector3.Zero;
	[Property, Description("Just not press 0f or a minus value")] public float force = 1f;

	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		var vel = positionForPunch * force;
		ply.sauceController.Punch( vel );
	}
}
