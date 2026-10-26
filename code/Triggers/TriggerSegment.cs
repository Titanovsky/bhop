public sealed class TriggerSegment : Component, Component.ITriggerListener
{
	[Property] public int SegmentId { get; set; } = 0;

	private Segment _segment;
	public Segment Segment => _segment ??= new(SegmentId);

	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() || ply.IsProxy ) return;

		ply.SetupSegment(this);
	}
}
