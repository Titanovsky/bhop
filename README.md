# BHOP

Bunny hop игра на s&box (движок Source 2 от Facepunch). Сингловый таймтриал-проект с CS2-подобной механикой движения, чекпоинтами-сегментами, сохранением личных рекордов и Steam-лидербордами.

## Стек и базовая конфигурация

- **Движок:** s&box, C# (`code/`), Razor UI (`code/UI/`).
- **Манифест:** `bhop.sbproj` — `MaxPlayers=1`, `TickRate=50`, `GameNetworkType=Multiplayer` (несмотря на 1 игрока), `StartupScene=scenes/start.scene`.
- **Контент:** карты в `Assets/scenes/map.bhop_*.scene` (aqueous, atom, colorshit, nuke, paper, rally, swooloe), префабы игрока и партиклов сегмента, иконки/звуки.

## Системы

### Движение — `code/SauceMovement/SauceController.cs`
Кастомный character controller в стиле Source/CS2: friction, ground/air accelerate, `MaxAirWishSpeed`, авто-бхоп (`AutoBunnyhopping`), приседание с проверкой потолка, `TryUnstuck`, `CategorizePosition`, terminal velocity 3500, кастомные FOV/гравитация, опциональный camera roll. Все параметры экспонированы и пробрасываются из `GameConfig`. Есть закомментированная stamina-система (заготовка).

`Footsteps.cs` — звук шагов через `OnFootstepEvent` + поверхностный трейс.

### Игрок и прогресс — `code/Player/Player.cs`
Синглтон `Player.Instance`. Состояния: `Starting / Walkthrough / Finished`. Управляет:
- стартовой/чекпоинтной позицией и `Respawn` / `ResetProgress` (хоткеи `Reload`, `Respawn`);
- таймером прохождения (`TimeUntil _timeWalkthrough`);
- финишем (`FinishProgress`) с сохранением сегментов и пушем статов/ачивок;
- применением `GameConfig` к контроллеру (`SetupGameConfig`) и проверкой "честного" конфига (`CheckConfig` → блокирует статистику при изменённых параметрах).

### Сегменты (чекпоинты) — `code/Segment/`
- `Segment` — данные одного чекпоинта (Id, TimeDone, TimeDonePrevious, Delta, IsBest).
- `SegmentHandler` — синглтон-список, собирает все `TriggerSegment` на сцене в `OnStart`.
- `SegmentLoader` — сохранение/загрузка JSON в `FileSystem.Data` по имени карты (`{mapName}.json`), переносит личные рекорды между сессиями.

### Триггеры — `code/Triggers/`
- `TriggerSegment` — авто-регистрация по `Segment.MaxId+1` при `SegmentId == 0`.
- `TriggerFinish` — вызывает `FinishProgress`.
- `TriggerRespawn` — респаун; если финиш — ещё и `ResetProgress`.
- `TriggerTeleport` — телепорт с опциональным сбросом скорости.
- `TriggerPunch` — импульс игроку (`positionForPunch * force`).

### Карта — `code/Map.cs`
Обёртка над `MapInstance`: по флагам выключает `light_environment` / `env_sky` / `skybox_reference` из исходной Source-карты.

### Конфиг — `code/GameConfig.cs`
Статический класс с `[ConVar(..., ConVarFlags.Saved)]` для всех параметров движения + `bhop_auto_bunnyhop`. Хранит дефолты, имеет `Reset()` и пушит значения в активного `Player`.

### UI — `code/UI/`
- `StartMenu.razor` — главное меню: Play (грид карт с иконками и кастомным скроллбаром), Options (`Game.Overlay.ShowSettingsModal`), Reset Config, **Publish Config** (создаёт `Storage` entry с `config.json` и сгенерированной обложкой через `Bitmap`/`DrawText`), Exit.
- `HUD.razor` — спидометр, общий таймер, время последнего сегмента + дельта (зелёный/красный), индикатор «Stopped (reset config)» если параметры изменены, метка `Final`.
- `LeaderboardHud.razor` — топ-10 по `Leaderboards.GetFromStat($"{mapName}_time")`, ascending/min.
- `MainMenuHandler.cs` — открытие/закрытие меню по `Score` / Esc.
- `UI/Test/` — переиспользуемые `Checkbox` / `Slider` / `TestPanelUI`.

### Сервисы Steam
- **Stats:** `Stats.Increment("checkpoints", 1)`, `Stats.SetValue("{mapName}_time", finalTime)`.
- **Achievements:** `first_checkpoint`, `the_final`, плюс per-map (`win_aqueous`, `win_swooloe`, `win_nuke`, `win_rally`, `win_atom`, `win_colorshit`, `win_paper`).
- Статистика/ачивки начисляются только при `CheckConfig() == true` и `!Game.IsEditor` (`CanCompleteStats`).

## Фичи

- CS2-подобная физика с автобхопом и тонкой настройкой через ConVar-ы (сохраняется между запусками).
- Чекпоинт-сегментная система с дельтами и личными рекордами по каждой карте.
- Steam-лидерборды и ачивки на карту, anti-cheese через сверку конфига с дефолтами.
- Меню выбора карт с иконками и кастомным скроллбаром.
- Публикация своих конфигов в s&box `Storage` с автогенерируемой превью-картинкой.
- Авторегистрация сегмент-триггеров (можно не проставлять `SegmentId` вручную).
- Camera roll, кастомный FOV/гравитация, нормальные звуки шагов по поверхностям.
- Триггеры-помощники для маппинга: respawn / teleport / punch / finish / segment.

## Проблемы и тех-долг

### Архитектура / надёжность
- **Глобальные синглтоны** (`Player.Instance`, `SegmentHandler.Instance`) + прямые обращения из `Segment` (`Player.Instance.GetTime()`) → жёсткая связность, тяжело тестировать и переносить на мультиплеер.
- В манифесте `GameNetworkType=Multiplayer` и в `SauceController` есть `[Sync]`/`IsProxy`, но `MaxPlayers=1` и весь геймплей-код (таймер, сегменты, сейв) написан как сингловый. Несогласованность.
- `Segment.IsValid => this is not null` — бессмысленная проверка, всегда `true` для существующего объекта; реальные баги маскируются `seg.IsValid()`.
- `SegmentHandler.OnStart` собирает триггеры через `Scene.GetAllComponents<TriggerSegment>()` **без сортировки по `SegmentId`** — порядок зависит от обхода сцены, легко получить рассинхрон id.
- В `OnAwake` добавляется «нулевой» сегмент, но `ResetMaxId()` вызывается только при загрузке — потенциальная путаница `MaxId` между сценами.

### Геймплейная логика
- `Player.GetTime()` возвращает `_timeWalkthrough * -1` для `TimeUntil` — рабочий, но крайне нечитаемый трюк (отрицательный отсчёт от 0).
- `SetupSegment` сравнивает `segment == trigger.Segment || segment.Id > trigger.Segment.Id` — при равенстве id всё равно выполняется код ниже из-за `||` приоритета… на деле OK, но условие хрупкое и стоит переписать.
- `ResetProgress` обнуляет `TimeDone` только для уже сделанных сегментов и не трогает `IsNow`/`TimeStart` остальных — при перезапуске после частичного прохода возможны призрачные значения.
- `Teleport` отключает `CollisionBox.Enabled` синхронно и сразу включает обратно — на следующий тик ещё не применится, фикс «бага с касанием других коллайдеров» работает не всегда.
- `Map.GetEntity` падает на `First(...)` если сущности нет (нет `FirstOrDefault`).
- Хардкод имён карт и ачивок в `switch` в двух местах (`FinishProgress`, `CallMapTimeStats`) — добавление карты требует правок в нескольких файлах. Сам автор пометил `//todo clean this fucking shit` в `GameConfig`.

### UI
- `HUD.BuildHash` и `StartMenu.BuildHash` = `HashCode.Combine(Time.Now)` → панель ребилдится **каждый кадр**, лишний GC и нагрузка.
- `LeaderboardHud.SetStatistic` цикл `for (i; i<10)` без проверки `_entries.Count` → `IndexOutOfRangeException`, если в борде меньше 10 записей (есть try/catch, но это маскировка).
- `StartMenu` — закомментированная новогодняя кнопка `//todo remove after the holidays`.
- `Score`/Esc обработка в `MainMenuHandler` затирает `Input.EscapePressed = false` глобально — может ломать другие UI.

### Контроллер
- Закомментированный stamina-блок раскидан по файлу — мёртвый код.
- `Camera.Components.GetOrCreate<HUD>()` дергается **каждый FixedUpdate** в двух ветках — лучше кэшировать.
- `ProjectSettings.Physics.FixedUpdateFrequency = 64` в `OnAwake` контроллера — глобальный сайд-эффект из компонента игрока, конфликтует с `TickRate=50` в `.sbproj`.
- `Velocity *= GameObject.WorldScale; ... Velocity /= GameObject.WorldScale;` — рискованно при нулевом scale.

### Сохранения/сервисы
- `SegmentLoader` пишет в файл по `Player.Instance.mapName`; если `mapName == ""` (нет `Map`-компонента) — сейв уходит в `".json"`.
- `Publish Config` публикует storage entry без UI-фидбэка и обработки ошибок.

## Возможные направления

- Вытащить список карт + ачивок + stat-имён в один data-asset (`MapEntry`), убрать дублирующиеся `switch`.
- Нормальный `IValid` у `Segment`, сортировка `Segments` по `Id`, явная инициализация.
- Заменить `BuildHash = Time.Now` на хеш реально меняющихся данных.
- Кэшировать `HUD` в `SauceController`, развести «player tick» и «movement tick».
- Доделать или удалить stamina и мультиплеерные `[Sync]`-следы.
- Прятать кнопки меню от состояния (например, Publish Config только при `CheckConfig`).
