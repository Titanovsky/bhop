using Sandbox;

public static class SegmentLoader
{
	public static void Save()
	{
		var handler = SegmentHandler.Instance;
		if ( handler is null ) return;

		FileSystem.Data.WriteJson( $"{Player.Instance.mapName}.json", handler.Segments );

		Log.Info( "[SegmentLoader] Save" );
	}

	public static void Load()
	{
		var handler = SegmentHandler.Instance;
		if ( handler is null ) return;

		var saved = FileSystem.Data.ReadJson<List<Segment>>( $"{Player.Instance.mapName}.json" );
		if ( saved is null ) return;

		Segment.ResetMaxId();

		foreach ( var seg in handler.Segments )
		{
			if (seg is null) continue;

			var filter = saved.Where(x => x.Id == seg.Id);
            if (filter is null) continue;

			var oldSeg = filter.FirstOrDefault();
			if (oldSeg is null) continue;

			seg.TimeDonePrevious = oldSeg.TimeDonePrevious;
			seg.Delta = oldSeg.Delta;
		}

		Log.Info( "[SegmentLoader] Load" );
	}
}
