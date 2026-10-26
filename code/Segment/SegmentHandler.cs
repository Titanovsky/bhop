using System;
using Sandbox;

/// <summary>
/// Локальный (на каждом клиенте свой) список чекпоинтов карты. Строится лениво при первом обращении,
/// потому что игрок спавнится кодом и может стартовать раньше OnStart этого компонента.
/// </summary>
public class SegmentHandler : Component
{
	public static SegmentHandler Instance { get; private set; }

	private readonly List<Segment> _segments = new();
	private bool _built = false;

	public IReadOnlyList<Segment> Segments
	{
		get
		{
			Build();
			return _segments;
		}
	}

	public Segment First => Segments.First();

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	private void Build()
	{
		if (_built) return;
		_built = true;

		_segments.Add(new Segment(0));

		var triggers = Scene.GetAllComponents<TriggerSegment>().ToList();
		var maxId = triggers.Count > 0 ? triggers.Max(x => x.SegmentId) : 0;

		foreach (var trigger in triggers.OrderBy(x => x.SegmentId))
		{
			if (trigger.SegmentId == 0) trigger.SegmentId = ++maxId; // for auto register, but be careful

			_segments.Add(trigger.Segment);
		}

		_segments.Sort((a, b) => a.Id.CompareTo(b.Id));

		Log.Info( $"[SegmentHandler] Segments: {string.Join(", ", _segments.Select(x => x.Id))}" );
	}
}
