public sealed class TriggerSegment : Component, Component.ITriggerListener
{
	[Property] public int SegmentId { get; set; } = 0;

	public Segment Segment { get; private set; }

	protected override void OnAwake()
	{
		if ( SegmentId == 0 ) SegmentId = Segment.MaxId + 1; // for auto register, but be careful
		
		Segment = new(SegmentId);

		Log.Info( $"[TriggerSegment] Register: {SegmentId}" );
	}

	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		ply.SetupSegment(this);
	}
}
