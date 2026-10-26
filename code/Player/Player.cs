using Sandbox.Services;

public sealed class Player : Component
{
    /// <summary>
    /// Игрок этого клиента. У чужих игроков (прокси) логика забега не работает.
    /// </summary>
    public static Player Local { get; private set; }

    // Синхронизируется владельцем, нужно для scoreboard
    [Sync] public PlayerStateEnum State { get; private set; } = PlayerStateEnum.Starting;
    [Sync] public float BestTime { get; private set; } = 0f;
    [Sync] public int SegmentId { get; private set; } = 0;

    [Property] public SauceController sauceController;

    [Property] public GameObject _prefabSegment;
    [Property] public SoundEvent _soundStart;
    [Property] public SoundEvent _soundSegment;

    private Vector3 _startPos = Vector3.Zero;
    private Vector2 _startAng = Vector2.Zero;

    /// <summary>
    /// Последний взятый сегмент (0 - старт).
    /// </summary>
    public Segment segment;
    private Vector3 _segmentPos = Vector3.Zero;
    private Vector2 _segmentAng = Vector2.Zero;

    private TimeSince _timeWalkthrough = 0f;
    private float _finalTime = 0f;
    private MapRecord _record;

    /// <summary>
    /// Разница итогового времени с PB на момент финиша, null если PB не было.
    /// </summary>
    public float? FinalDelta { get; private set; } = null;
    public bool IsNewRecord { get; private set; } = false;

    public string mapName = ""; // for achievement, hardcode

    public void PlaySoundStart()
    {
        Sound.Play(_soundStart);
    }

    public void PlaySoundSegment()
    {
        Sound.Play(_soundSegment);
    }

    public void EmitParticleSegment()
    {
        _prefabSegment.Clone(position: WorldPosition + Scene.Camera.WorldRotation.Forward * 142f);
    }

    public void Teleport(Transform transform, bool resetVelocity = true)
    {
        if (!sauceController.IsValid()) return;

        if (resetVelocity) sauceController.Velocity = 0f;
        sauceController.CollisionBox.Enabled = false; // fix bag with touch the other colliders

        WorldPosition = transform.Position;
        Rotate(new Vector2(transform.Rotation.Pitch(), transform.Rotation.Yaw()));
        GameObject.Network.ClearInterpolation();

        sauceController.CollisionBox.Enabled = true;
    }

    public void Respawn()
    {
        if (!sauceController.IsValid()) return;

        sauceController.Velocity = 0f;
        sauceController.CollisionBox.Enabled = false; // fix bag with touch the other colliders

        var pos = segment.IsValid() ? _segmentPos : _startPos;
        var ang = segment.IsValid() ? _segmentAng : _startAng;

        WorldPosition = pos;
        Rotate(ang);
        GameObject.Network.ClearInterpolation();

        sauceController.CollisionBox.Enabled = true;
    }

    public void SetupGameConfig()
    {
        if (!sauceController.IsValid()) return;

        sauceController.Weight = GameConfig.SpeedMultiplier;
        sauceController.MaxSpeed = GameConfig.MaxSpeed;
        sauceController.MoveSpeed = GameConfig.MoveSpeed;
        sauceController.ShiftSpeed = GameConfig.ShiftSpeed;
        sauceController.CrouchSpeed = GameConfig.CrouchSpeed;
        sauceController.StopSpeed = GameConfig.StopSpeed;
        sauceController.Friction = GameConfig.Friction;
        sauceController.Acceleration = GameConfig.Acceleration;
        sauceController.AirAcceleration = GameConfig.AirAcceleration;
        sauceController.MaxAirWishSpeed = GameConfig.MaxAirWishSpeed;
        sauceController.JumpForce = GameConfig.JumpForce;
        sauceController.AutoBunnyhopping = GameConfig.AutoBunnyhopping;

        Log.Info("[Player] Setup GameConfig");
        Log.Info($"[Player] Check default params = {CheckConfig()}");
    }

    public void FinishProgress()
    {
        if (State != PlayerStateEnum.Walkthrough) return;

        _finalTime = GetTime();
        State = PlayerStateEnum.Finished;

        FinalDelta = (_record is not null) ? _finalTime - _record.Time : null;
        IsNewRecord = false;

        // PB пишем только с дефолтным конфигом и только если забег быстрее
        if (CheckConfig() && (_record is null || _finalTime < _record.Time))
        {
            SaveRecord();
            IsNewRecord = true;
        }

        if (CanCompleteStats())
        {
            CallMapTimeStats(); // лидерборд агрегирует минимум, поэтому отправляем каждый валидный финиш

            // hardcode achivements
            Achievements.Unlock("the_final");

            switch (mapName)
            {
                case "bafkb.bhopaqua":
                    Achievements.Unlock("win_aqueous");
                    break;

                case "obc.bhop_swooloe":
                    Achievements.Unlock("win_swooloe");
                    break;

                case "starblue.bhop_nuke":
                    Achievements.Unlock("win_nuke");
                    break;

                case "gear.bhop_rally":
                    Achievements.Unlock("win_rally");
                    break;

                case "gear.bhop_atom":
                    Achievements.Unlock("win_atom");
                    break;

                case "gear.bhop_colorshit":
                    Achievements.Unlock("win_colorshit");
                    break;

                case "grillus.bhoppapermap":
                    Achievements.Unlock("win_paper");
                    break;

                default:
                    break;
            }
        }

        PlaySoundSegment();

        Log.Info($"[Player] Finish: {_finalTime} (delta {FinalDelta})");
    }

    public void ResetProgress()
    {
        State = PlayerStateEnum.Starting;
        _timeWalkthrough = 0f;
        _finalTime = 0f;
        FinalDelta = null;
        IsNewRecord = false;

        var handler = SegmentHandler.Instance;
        if (handler.IsValid())
        {
            foreach (Segment seg in handler.Segments)
                seg.Reset();

            segment = handler.First;
        }

        SegmentId = 0;
        _segmentPos = _startPos;
        _segmentAng = _startAng;

        PlaySoundStart();

        RefreshLeadboardHud();
    }

    public bool CheckConfig()
    {
        if (sauceController.Weight != GameConfig.DefaultSpeedMultiplier) return false;
        else if (sauceController.MaxSpeed != GameConfig.DefaultMaxSpeed) return false;
        else if (sauceController.MoveSpeed != GameConfig.DefaultMoveSpeed) return false;
        else if (sauceController.ShiftSpeed != GameConfig.DefaultShiftSpeed) return false;
        else if (sauceController.CrouchSpeed != GameConfig.DefaultCrouchSpeed) return false;
        else if (sauceController.StopSpeed != GameConfig.DefaultStopSpeed) return false;
        else if (sauceController.Friction != GameConfig.DefaultFriction) return false;
        else if (sauceController.Acceleration != GameConfig.DefaultAcceleration) return false;
        else if (sauceController.AirAcceleration != GameConfig.DefaultAirAcceleration) return false;
        else if (sauceController.MaxAirWishSpeed != GameConfig.DefaultMaxAirWishSpeed) return false;
        else if (sauceController.JumpForce != GameConfig.DefaultJumpForce) return false;

        return true;
    }

    public bool CanCompleteStats()
    {
        if (!CheckConfig()) return false;
        else if (Game.IsEditor) return false;

        return true;
    }

    public void SetupSegment(Collider collider)
    {
        SetupSegment(collider.GetComponent<TriggerSegment>());
    }

    public void SetupSegment(TriggerSegment trigger)
    {
        if (!trigger.IsValid()) return;
        if (State != PlayerStateEnum.Walkthrough) return; // до старта и после финиша чекпоинты не берутся

        var seg = trigger.Segment;
        if (segment.IsValid() && seg.Id <= segment.Id) return; // уже взят или вернулся назад

        seg.Take(GetTime(), _record);

        segment = seg;
        SegmentId = seg.Id;
        _segmentPos = GetSpawnPos(trigger.GameObject);
        _segmentAng = sauceController.LookAngle;

        if (CanCompleteStats())
        {
            Stats.Increment("checkpoints", 1);
            Achievements.Unlock("first_checkpoint");
        }

        PlaySoundSegment();
        EmitParticleSegment();

        Log.Info($"[Player] Take {trigger.GameObject}");
    }

    public float GetTime()
    {
        if (State == PlayerStateEnum.Finished) return _finalTime;
        if (State == PlayerStateEnum.Starting) return 0f;

        return _timeWalkthrough;
    }

    public bool HasRecord() => _record is not null;

    private void SaveRecord()
    {
        var record = new MapRecord { Time = _finalTime };

        var handler = SegmentHandler.Instance;
        if (handler.IsValid())
        {
            foreach (Segment seg in handler.Segments)
            {
                if (!seg.IsDone()) continue;

                record.Splits[seg.Id] = seg.Split;
            }
        }

        record.Save(mapName);

        _record = record;
        BestTime = record.Time;
    }

    private void LoadRecord()
    {
        _record = MapRecord.Load(mapName);
        BestTime = _record?.Time ?? 0f;
    }

    private void CallMapTimeStats()
    {
        if (!CanCompleteStats()) return;

        switch (mapName)
        {
            case "bafkb.bhopaqua":
                Stats.SetValue("bafkb.bhopaqua_time", _finalTime);
                break;

            case "obc.bhop_swooloe":
                Stats.SetValue("obc.bhop_swooloe_time", _finalTime);
                break;

            case "starblue.bhop_nuke":
                Stats.SetValue("starblue.bhop_nuke_time", _finalTime);
                break;

            case "gear.bhop_rally":
                Stats.SetValue("gear.bhop_rally_time", _finalTime);
                break;

            case "gear.bhop_atom":
                Stats.SetValue("gear.bhop_atom_time", _finalTime);
                break;

            case "gear.bhop_colorshit":
                Stats.SetValue("gear.bhop_colorshit_time", _finalTime);
                break;

            case "grillus.bhoppapermap":
                Stats.SetValue("grillus.bhoppapermap_time", _finalTime);
                break;

            default:
                break;
        }
    }

    private void PrepareAchievements()
    {
        mapName = Scene.Name; // fallback, если на сцене нет MapInstance

        var map = Scene.Directory.FindByName("Map");
        if (map is null) return;
        if (map.Count() == 0) return;

        var gameobj = map.First();

        var instance = gameobj.Components.Get<MapInstance>();
        if (!instance.IsValid()) return;

        mapName = instance.MapName;

        Log.Info($"[Player] Map Instance: {mapName}");
    }

    private void PrepareLookOnStart()
    {
        var rot = WorldRotation;

        Rotate(new Vector2(rot.Pitch(), rot.Yaw()));
    }

    private void RefreshLeadboardHud()
    {
        var leaderboard = Scene.GetAllComponents<LeaderboardHud>().FirstOrDefault();
        if (!leaderboard.IsValid()) return;

        leaderboard.SetStatistic($"{mapName}_time");
        leaderboard.Show = false;
        leaderboard.Show = true;
    }

    private void PrepareControllers()
    {
        if (!sauceController.IsValid())
            sauceController = Components.Get<SauceController>();

        var handler = SegmentHandler.Instance;
        if (handler.IsValid())
            segment = handler.First;
    }

    private void CheckResetButton()
    {
        if (!Input.Pressed("Reload")) return;

        ResetProgress();
        Respawn();
    }

    private void CheckRespawnButton()
    {
        if (!Input.Pressed("Respawn")) return;

        Respawn();
    }

    private void PrepareSpawns()
    {
        _startPos = WorldPosition;
        _startAng = sauceController.LookAngle;

        _segmentPos = _startPos;
        _segmentAng = _startAng;
    }

    /// <summary>
    /// Личный отсчёт начинается с любого движения со спавна: клавиши движения, прыжок или скорость.
    /// Проверяется по Down, а не Pressed, чтобы зажатая при ресете клавиша тоже запускала таймер.
    /// </summary>
    private void CheckChangeStateToWalkthrough()
    {
        if (State != PlayerStateEnum.Starting) return;

        var isMoving = !Input.AnalogMove.IsNearZeroLength
            || Input.Down("Jump")
            || sauceController.Velocity.WithZ(0).Length > 1f;

        if (!isMoving) return;

        State = PlayerStateEnum.Walkthrough;
        _timeWalkthrough = 0f;
    }

    private Vector3 GetSpawnPos(GameObject gameObj)
    {
        return gameObj.GetBounds().Center;
    }

    private void Rotate(Vector2 ang)
    {
        sauceController.LookAngle = ang;
    }

    protected override void OnDestroy()
    {
        if (Local == this)
            Local = null;
    }

    protected override void OnStart()
    {
        if (IsProxy) return;

        Local = this;

        PrepareAchievements();
        PrepareControllers();
        PrepareLookOnStart();
        PrepareSpawns();
        SetupGameConfig();
        LoadRecord();
        PlaySoundStart();
        RefreshLeadboardHud();
    }

    protected override void OnUpdate()
    {
        if (IsProxy) return;

        CheckResetButton();
        CheckRespawnButton();
    }

    protected override void OnFixedUpdate()
    {
        if (IsProxy) return;

        CheckChangeStateToWalkthrough();
    }
}

public enum PlayerStateEnum
{
    Starting = 0,
    Walkthrough = 1,
    Finished = 2,
}
