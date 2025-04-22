using System;
using Sandbox;

public class SegmentHandler : Component
{
	protected override void OnStart()
	{
		Segment.ResetMaxId();
	}
}
