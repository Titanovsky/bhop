public static class GameConfig
{
	[ConVar("sv_maxspeed", ConVarFlags.GameSetting), Range( 0f, 2000f, 1f )] public static float MaxSpeed { get; set; } = 285.98f;
	[ConVar( "sv_movespeed", ConVarFlags.GameSetting ), Range( 0f, 2000f, 1f )]  public static float MoveSpeed { get; set; } = 250f;
	[ConVar( "sv_crouchspeed", ConVarFlags.GameSetting ), Range( 0f, 2000f, 1f )] public static float CrouchSpeed { get; set; } = 85f;
	[ConVar( "sv_stopspeed", ConVarFlags.GameSetting ), Range( 0f, 2000f, 1f )] public static float StopSpeed { get; set; } = 80f;
	[ConVar( "sv_friction", ConVarFlags.GameSetting ), Range( 0f, 2000f, clamped: false )] public static float Friction { get; set; } = 5.2f;
	[ConVar( "sv_acceleration", ConVarFlags.GameSetting ), Range( 0f, 100f, clamped: false )] public static float Acceleration { get; set; } = 5.5f;
	[ConVar( "sv_air_acceleration", ConVarFlags.GameSetting ), Range( 0f, 100f, clamped: false )] public static float AirAcceleration { get; set; } = 12f;
	[ConVar( "sv_max_air_wish_speed", ConVarFlags.GameSetting ), Range( 0f, 1000f, 1f )] public static float MaxAirWishSpeed { get; set; } = 30f;
	[ConVar( "sv_jumpforce", ConVarFlags.GameSetting ), Range( 0f, 1000f, clamped: false )] public static float JumpForce { get; set; } = 301.993378f;
	[ConVar( "sv_auto_bunnyhop", ConVarFlags.GameSetting )] private static bool AutoBunnyhopping { get; set; } = true;
}
