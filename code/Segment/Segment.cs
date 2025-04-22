using Sandbox;

public class Segment
{
	public int Id { get; set; }
	public float TimeDone { get; set; } = 0f;
	public float TimeDonePrevious { get; set; } = 0f;
	public float TimeStart { get; set; } = 0f;
	public bool IsNow { get; set; } = false;

	public static int MaxId { get; private set; } = 0;

	public Segment(int id)
	{
		if ( MaxId >= id ) Log.Warning( $"[Segment] Id {id} is alive" );

		Id = id;
		MaxId = id;

		Log.Info( $"[Segment] Register: {id}" );
	}

	public bool IsDone() => (TimeDone > 0f);
	public bool IsPlus() => (TimeDone > TimeDonePrevious);

	public void Start()
	{
		if ( IsNow ) return;

		IsNow = true;

		TimeStart = Player.Instance.GetTime();

		Log.Info( $"[Segment] Start {Id} ({TimeStart})" );
	}

	public void Finish()
	{
		if ( !IsNow ) return;

		IsNow = false;

		TimeDone = Player.Instance.GetTime() - TimeStart;

		Log.Info( $"[Segment] Finish {Id} ({TimeDone})" );
	}

	public static void ResetMaxId()
	{
		MaxId = 0;
	}
}
