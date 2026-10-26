using Sandbox;

/// <summary>
/// Чекпоинт карты. Сегмент 0 - старт, остальные берутся через TriggerSegment.
/// Split - общее время забега в момент входа в сегмент, Delta - разница со сплитом лучшего забега (PB).
/// </summary>
public class Segment : IValid
{
	public int Id { get; }
	public float Split { get; private set; } = 0f;
	public float? Delta { get; private set; } = null;
	public bool IsValid => true;

	public Segment(int id)
	{
		Id = id;
	}

	public bool IsDone() => (Split > 0f);
	public bool IsBest() => (Delta is null || Delta <= 0f);

	public void Take(float time, MapRecord record)
	{
		Split = time;
		Delta = (record is not null && record.Splits.TryGetValue(Id, out var pbSplit)) ? time - pbSplit : null;

		Log.Info( $"[Segment] Take {Id} ({Split}, delta {Delta})" );
	}

	public void Reset()
	{
		Split = 0f;
		Delta = null;
	}
}
