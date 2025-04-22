using Sandbox;

public class Segment
{
	public int Id { get; set; }
	public float TimeDone { get; set; } = 0f;
	public float TimeDonePrevious { get; set; } = 0f;
	public float TimeStart { get; set; } = 0f;
	public bool IsNow { get; set; } = false;

	private static int MaxId = 0;

	public Segment(int id)
	{
		if ( id >= MaxId ) Log.Warning( $"[Segment] Id {id} is alive" );

		Id = id;
		MaxId = id;
	}

	public bool IsDone() => (TimeDone > 0f);
	public bool IsPlus() => (TimeDone > TimeDonePrevious);

	public void Start()
	{
		IsNow = true;

		TimeStart = Time.Now;
	}

	public void Finish()
	{
		IsNow = false;

		TimeDone = Time.Now - TimeStart;
	}
}
