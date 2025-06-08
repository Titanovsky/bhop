using Sandbox;
using System.Text.Json.Serialization;

public class Segment : IValid
{
	public static int MaxId { get; private set; } = 0;

	public int Id { get; set; }
	public float TimeDonePrevious { get; set; } = 0f;
	[JsonIgnore] public float TimeDone { get; set; } = 0f;
	[JsonIgnore] public float TimeRealDone { get; set; } = 0f;
	[JsonIgnore] public float TimeStart { get; set; } = 0f;
	public float Delta { get; set; } = 0f;
	[JsonIgnore] public bool IsNow { get; set; } = false;
	[JsonIgnore] public bool IsValid => this is not null;

	public Segment(int id)
	{
		Id = id;
		MaxId = id;
	}

	public bool IsDone() => (TimeDone > 0f);
	public bool IsPlus() => (TimeDonePrevious == 0f || TimeDone <= TimeDonePrevious);

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

		var time = Player.Instance.GetTime();

		TimeRealDone = time;
		TimeDone = time - TimeStart;
		
		Delta = TimeDone - TimeDonePrevious;

		Log.Info( $"[Segment] Finish {Id} ({TimeDone})" );
	}

	public static void ResetMaxId()
	{
		MaxId = 0;
	}
}
