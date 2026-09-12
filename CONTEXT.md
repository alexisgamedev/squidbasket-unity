# squidbasket-unity

A 3D arcade basketball shooter: aim/power a shot from anywhere on the court against a single hoop, score-attack style, balancing realism against arcade feel.

## Language

**Session**:
A single continuous play period with no end condition — the player shoots indefinitely and Score accumulates for as long as they keep playing.
_Avoid_: Run, round, game (when meaning a bounded play period — there is no bound)

**Shot**:
An attempt to score by releasing the ball toward the hoop from wherever the player is currently standing; the player is free to shoot from any point on the court rather than from designated spots. Worth 2 or 3 points depending on which Zone it's released from (see Zone), with no maximum range.
_Avoid_: Attempt (when referring to the action generically, prefer Shot)

**Zone**:
One of two court regions — worth 2 or 3 points — matching the real-basketball two-point/three-point arc split. Every point on the court beyond the arc counts as the 3-point Zone; there is no outer boundary.
_Avoid_: Spot, tier (these imply designated shooting positions, which this game does not have)

**Miss**:
A Shot that does not go through the hoop. Carries no scoring penalty.

**Reset**:
The player-triggered action (bound to the R key) that relocates the player to the free-throw line and gives them the ball immediately. Only available during Walking — it cannot be used to interrupt Shooting, which preserves there being no player-initiated way to cancel a Shot in progress. Used after any Shot (Make or Miss) as a shortcut past manually retrieving the ball.
_Avoid_: Retrieve, respawn

**Retrieval**:
Walking to the ball where it comes to rest after any Shot (Make or Miss) and picking it up, so play can continue. The alternative to using Reset.

**Score**:
The player's cumulative points from made Shots within a Session. Resets to zero on every app launch — there is no persistence between Sessions (see Progression).

**Streak**:
A count of the player's consecutive made Shots, reset to zero by a Miss, shown to the player as visual feedback. Awards no bonus points.

**Restart**:
A player-triggered action, separate from Reset, that zeros Score back to its starting value without requiring the app to relaunch.
_Avoid_: Reset (Reset relocates the player and ball; Restart zeros the Score — different concepts, don't conflate)

**Progression**:
Persistent advancement carried between Sessions (e.g. unlocks, difficulty tiers). This game has none — every Session starts from the same baseline.

## Player state

The player is always in exactly one of two states, which govern camera perspective and available actions.

**Walking**:
The default state. Third-person camera; WASD moves the player, mouse look orbits the camera; the player carries/dribbles the ball. Entered whenever Shooting ends, whether by a released Shot or a Shot Timeout.
_Avoid_: Movement state, default state

**Shooting**:
Entered from Walking by holding LMB; the camera blends (briefly) to first-person, Bullet Time engages, and the Power Bar appears. The player cannot move with WASD, but keeps whatever momentum they had on entry without it decaying — like everything but the Power Bar, its real-world drift is slowed by Bullet Time. Shot direction combines the camera's forward direction with an Up vector (scaled by Power) to produce a realistic throwing arc. Releasing LMB fires a Shot using the Power Bar's current value and returns to Walking; a Shot Timeout does the same without firing a Shot, and the ball is lost.
_Avoid_: Aiming state (Aiming is not yet a distinct concept from Shooting)

**Bullet Time**:
A time-scale slowdown applied for the duration of Shooting, affecting player momentum, physics, and mouse-look/aim sensitivity alike. The Power Bar is the sole exemption, ticking at regular speed regardless of the slowdown. The effect on aim is an experimental tunable, not a settled decision — likely to be adjusted once it's playable.

**Power**:
The Power Bar's value at the instant LMB is released, determining how far the Shot travels, and scaling the size of the shot's Up-vector arc.

**Power Bar**:
The UI element shown during Shooting. Continuously loops (ping-pongs) between a minimum and maximum value for as long as LMB is held, rather than filling once and holding. Ticks at regular speed, exempt from Bullet Time.
_Avoid_: Meter (Power Bar is the project term)

**Shot Timeout**:
If the Power Bar completes 2.5 loops without LMB being released, Shooting is forcibly exited with no Shot fired and the ball is lost, requiring Retrieval just like after a real Shot.
_Avoid_: Cancel (this is a forced timeout, not a player-initiated cancel — there is no player-initiated cancel)
