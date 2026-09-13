# Squid Basketball

A 3D arcade basketball shooter built in Unity 6 (URP): aim and power a shot from anywhere on the
court against a single hoop, balancing realism against arcade feel.
Desktop-only, single-player.

- Editor version: **6000.0.58f2** (see `ProjectSettings/ProjectVersion.txt`)
- Render pipeline: URP
- See `CLAUDE.md` for CLI-driven project tooling (opening, building, testing, package management),
  `CONTEXT.md` for the project's domain vocabulary, and `docs/adr/` for the reasoning behind the
  design decisions summarized below.

## Game loop

There is no end condition — no timer, shot limit, or lives. A **Session** is a single continuous
play period: the player shoots indefinitely and **Score** accumulates for as long as they keep
playing. Score resets to zero only on relaunch or via **Restart**, never automatically.

A **Shot** can be released from anywhere on the court — there are no designated shooting spots.
Where you release from determines its **Zone**:

- **Two-point Zone** — inside the three-point arc
- **Three-point Zone** — on or beyond the arc, with no outer boundary; every point past the arc
  counts

Whether a Shot is a **Make** or a **Miss** is decided purely by ball physics against the hoop —
there's no hidden accuracy or "sweet spot" assist. A Miss carries no scoring penalty. Consecutive
makes build a **Streak** (shown as feedback, no bonus points), reset to zero by a Miss.

After any Shot (Make or Miss), the ball comes to rest somewhere on the court and must be brought
back into play, either by:

- **Retrieval** — walking over to the loose ball to pick it up, or
- **Reset** — a shortcut that instantly relocates the player to the free-throw line with the ball
  in hand (Walking only — see below)

This manual-retrieval pacing is deliberate (see `docs/adr/0001-endless-freeform-loop.md`), not
something to "fix" by auto-returning the ball after a make.

## Core mechanics

### Player states: Walking and Shooting

The player is always in exactly one of two states:

**Walking** (default)
- Third-person camera
- WASD/left stick moves the player relative to the camera; mouse/right-stick look orbits the
  camera
- The player carries the ball
- Jump is available (see below)
- Reset and Restart are only usable here

**Shooting** — entered by holding the Shoot input
- The camera blends briefly to first-person
- **Bullet Time** engages: a time-scale slowdown affecting player momentum, physics, and
  mouse-look/aim sensitivity alike
- Movement input is locked, but whatever horizontal momentum the player had on entry keeps
  carrying them, undecayed (its real-world drift only slows as a side effect of Bullet Time)
- The **Power Bar** appears and starts looping
- Shot direction combines the camera's forward direction with an upward arc component scaled by
  the current Power, for a realistic throwing arc
- Releasing the Shoot input fires the Shot at the Power Bar's current value and returns to
  Walking

See `docs/adr/0002-shot-mechanic.md` for the reasoning behind Bullet Time, the Power Bar, and
pure-physics outcomes.

### Power Bar and Shot Timeout

The Power Bar continuously ping-pongs between a minimum and maximum value for as long as Shoot is
held — it does not fill once and hold. It ticks at regular speed, exempt from Bullet Time. If it
completes **2.5 loops** without Shoot being released, a **Shot Timeout** forcibly exits Shooting
with no Shot fired and the ball lost (requiring Retrieval, same as after a real Shot). A
same-frame release and timeout favors the release — Shot Timeout is a cost of holding too long,
not a way to steal an intentionally-released shot from the player. There is no player-initiated
way to cancel a Shot once Shooting has begun.

### Jump

A vertical hop available only while Walking (grounded required, so it can't be re-triggered
mid-air). Horizontal momentum freezes at whatever it was the instant Jump started and only
unfreezes on landing — there's no air control and no double-jump. Jump is implemented as an
action inside Walking rather than a third player state, since it doesn't change camera
perspective, input mode, or ball attachment (see `docs/adr/0004-jump-is-not-a-player-state.md`).

## Controls

Keyboard & mouse and gamepad are both supported (`Assets/InputSystem_Actions.inputactions`).

| Action  | Keyboard & Mouse       | Gamepad              |
|---------|-------------------------|-----------------------|
| Move    | WASD / Arrow keys       | Left stick           |
| Look    | Mouse                   | Right stick          |
| Jump    | Space                   | South button (A/Cross) |
| Shoot (hold) | Left mouse button / Enter | West button (X/Square) |
| Reset   | Right mouse button      | North button (Y/Triangle) |
| Restart | Backspace               | Right shoulder       |
| Pause menu | Esc                  | Start                |

The pause menu (Esc) offers Restart, a camera sensitivity slider, a trajectory-preview toggle,
and Quit; opening it disables Move/Look/Shoot/Reset/Restart until closed.

## Architecture

No scene has gameplay wired in from the stock template except the runtime scripts under
`Assets/Scripts/Runtime/`:

- `Player/` — `PlayerStateController` (state-object FSM context), `WalkingState`/`ShootingState`,
  `PlayerFsm` (pure transition rules), `PlayerMovement`, `PowerBarOscillator`
- `Camera/` — hand-rolled `ThirdPersonCamera`/`FirstPersonCamera` driving one shared `Camera`
  (a deliberate deviation from using Cinemachine — see `docs/adr/0003-player-architecture.md`)
- `Gameplay/` — `Ball` (Held/InFlight/Loose state, Retrieval, Shot release)
- `Scoring/` — `HoopTrigger`, `GameEvents`, `ScoreState`/`ScoreSystem`, `ShotCalculator`, `Zone`
- `Input/` — `PlayerInputSource` (Input System wrapper), `GamepadLookScaler`
- `UI/` — `ScoreHud`, `MenuWindow`

Scoring is event-driven: `HoopTrigger` detects a pass-through and asks the `Ball` (which recorded
its own release Zone/points) to raise a make event via `GameEvents`; `ScoreSystem` is a dumb
subscriber that accumulates Score/Streak and separately owns Restart.

EditMode tests for this logic live under `Assets/Scripts/Tests/EditMode/`.
