using System;

public static class GameConfig
{
    //todo clean this fucking shit
    public static float DefaultSpeedMultiplier { get; private set; } = 1.4f;
    public static float DefaultMaxSpeed { get; private set; } = 2000f;
    public static float DefaultMoveSpeed { get; private set; } = 220f;
    public static float DefaultShiftSpeed { get; private set; } = 130f;
    public static float DefaultCrouchSpeed { get; private set; } = 85f;
    public static float DefaultStopSpeed { get; private set; } = 80f;
    public static float DefaultFriction { get; private set; } = 5.2f;
    public static float DefaultAcceleration { get; private set; } = 5.5f;
    public static float DefaultAirAcceleration { get; private set; } = 12f;
    public static float DefaultMaxAirWishSpeed { get; private set; } = 30f;
    public static float DefaultJumpForce { get; private set; } = 301;

    [ConVar("bhop_speed_multiplier", ConVarFlags.Saved), Range(0f, 100f)] public static float SpeedMultiplier { get; set; } = 1.4f;
    [ConVar("bhop_maxspeed", ConVarFlags.Saved), Range(0f, 10000f)] public static float MaxSpeed { get; set; } = 2000f;
	[ConVar("bhop_movespeed", ConVarFlags.Saved), Range(0f, 10000f)] public static float MoveSpeed { get; set; } = 220f;
    [ConVar("bhop_shiftspeed", ConVarFlags.Saved), Range(0f, 2000f)] public static float ShiftSpeed { get; set; } = 130f;
    [ConVar("bhop_crouchspeed", ConVarFlags.Saved), Range(0f, 2000f)] public static float CrouchSpeed { get; set; } = 85f;
	[ConVar("bhop_stopspeed", ConVarFlags.Saved), Range(0f, 2000f)] public static float StopSpeed { get; set; } = 80.0f;
	[ConVar("bhop_friction", ConVarFlags.Saved), Range(0f, 2000f, clamped: false)] public static float Friction { get; set; } = 5.2f;
	[ConVar("bhop_acceleration", ConVarFlags.Saved), Range(0f, 100f, clamped: false)] public static float Acceleration { get; set; } = 5.5f;
	[ConVar("bhop_air_acceleration", ConVarFlags.Saved), Range(0f, 100f, clamped: false)] public static float AirAcceleration { get; set; } = 12f;
	[ConVar("bhop_max_air_wish_speed", ConVarFlags.Saved), Range(0f, 1000f)] public static float MaxAirWishSpeed { get; set; } = 30f;
	[ConVar("bhop_jumpforce", ConVarFlags.Saved), Range(0f, 1000f, clamped: false)] public static float JumpForce { get; set; } = 301;

	[ConVar("bhop_auto_bunnyhop", ConVarFlags.Saved)] public static bool AutoBunnyhopping { get; set; } = true;

    public static void Reset()
    {
        ConsoleSystem.SetValue("bhop_speed_multiplier", DefaultSpeedMultiplier);
        ConsoleSystem.SetValue("bhop_maxspeed", DefaultMaxSpeed);
        ConsoleSystem.SetValue("bhop_movespeed", DefaultMoveSpeed);
        ConsoleSystem.SetValue("bhop_shiftspeed", DefaultShiftSpeed);
        ConsoleSystem.SetValue("bhop_crouchspeed", DefaultCrouchSpeed);
        ConsoleSystem.SetValue("bhop_stopspeed", DefaultStopSpeed);
        ConsoleSystem.SetValue("bhop_friction", DefaultFriction);
        ConsoleSystem.SetValue("bhop_acceleration", DefaultAcceleration);
        ConsoleSystem.SetValue("bhop_air_acceleration", DefaultAirAcceleration);
        ConsoleSystem.SetValue("bhop_max_air_wish_speed", DefaultMaxAirWishSpeed);

        Log.Info("[GameConfig] Reset");

        var ply = Player.Instance;
        if (!ply.IsValid()) return;

        ply.SetupGameConfig();
    }
}
