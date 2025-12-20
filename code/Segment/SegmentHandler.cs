using System;
using Sandbox;

public class SegmentHandler : Component
{
	public static SegmentHandler Instance { get; set; }
	[Property] public List<Segment> Segments { get; set; } = new();

	protected override void OnAwake()
	{
		Instance = this;

		var segZero = new Segment( 0 );
		Segments.Add( segZero );
	}

	protected override void OnStart()
	{
		foreach (var trigger in Scene.GetAllComponents<TriggerSegment>())
		{
			var seg = trigger.Segment;

			Segments.Add(seg);
		}

		SegmentLoader.Load();
	}
}
