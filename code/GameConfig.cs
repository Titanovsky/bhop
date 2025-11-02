public static class GameConfig
{
	[ConVar("sv_maxspeed", ConVarFlags.Saved), Range(0f, 2000f)] public static float MaxSpeed { get; set; } = 320.0f;
	[ConVar("sv_movespeed", ConVarFlags.Saved), Range(0f, 2000f)] public static float MoveSpeed { get; set; } = 250f;
	[ConVar("sv_crouchspeed", ConVarFlags.Saved), Range(0f, 2000f)] public static float CrouchSpeed { get; set; } = 85f;
	[ConVar("sv_stopspeed", ConVarFlags.Saved), Range(0f, 2000f)] public static float StopSpeed { get; set; } = 80.0f;
	[ConVar("sv_friction", ConVarFlags.Saved), Range(0f, 2000f, clamped: false)] public static float Friction { get; set; } = 5.2f;
	[ConVar("sv_acceleration", ConVarFlags.Saved), Range(0f, 100f, clamped: false)] public static float Acceleration { get; set; } = 5.5f;
	[ConVar("sv_air_acceleration", ConVarFlags.Saved), Range(0f, 100f, clamped: false)] public static float AirAcceleration { get; set; } = 12.0f;
	[ConVar("sv_max_air_wish_speed", ConVarFlags.Saved), Range(0f, 1000f)] public static float MaxAirWishSpeed { get; set; } = 10f;
	[ConVar("sv_jumpforce", ConVarFlags.Saved), Range(0f, 1000f, clamped: false)] public static float JumpForce { get; set; } = 301.993378f;
	[ConVar("sv_auto_bunnyhop", ConVarFlags.Saved)] private static bool AutoBunnyhopping { get; set; } = true;
}
