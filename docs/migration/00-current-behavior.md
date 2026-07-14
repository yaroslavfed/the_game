# Current behavior baseline

This document records the observable behavior that must be preserved while the
application is migrated to MVVM and ReactiveUI. It is based on the current
implementation at commit `d4b586e`. Runtime data and asset directories are not
present in the repository, so the persistence formats below are inferred from
the read and write operations in the source code.

## Application flow

1. `MainWindow` plays the initial background video.
2. Pressing Enter opens the main menu (`start_page`).
3. The main menu either restores the remembered user or displays login and
   registration controls.
4. An authenticated user can open their profile.
5. From the profile the user can open the store, knowledge base, or game mode
   selection.
6. Single-player mode opens the battlefield.
7. A completed battle opens the results screen.
8. Leaving results returns to game mode selection.

The current implementation performs most transitions by creating a new WPF
`Window`, showing it, and closing the previous window. The target design must
preserve the flow while rendering screens inside one shell window.

## Authentication scenarios

### Registration

- Login, password, and password confirmation are required.
- Both password entries must match.
- Login must not already occur in `id.txt`.
- A numeric user ID is allocated from the last entry in `id.txt`.
- A user file, record file, and default avatar are created.
- Registration success or validation failure is displayed to the user.

### Login

- The supplied login is looked up in `id.txt`.
- The supplied password is compared with the stored password.
- Successful login establishes the current user ID.
- If "remember" is enabled, the current implementation writes login,
  password, and user ID to `logged.txt`.
- Logout replaces `logged.txt` with `false`.

The migration must not preserve plaintext password storage. Compatibility code
may read the legacy format only long enough to migrate it to a protected format.

## Player profile scenarios

- Display nickname, level, experience, money, avatar, equipped weapon, and
  equipped armor.
- Maximum health is calculated as `100 + level * 4`.
- Damage boost is unlocked at level 25.
- Healing is unlocked at level 50.
- Owned weapons and armor can be opened as collections.
- Double-clicking an owned item equips it.
- Equipping an item updates the profile and persists it.

## Store scenarios

- Display weapon and armor catalogs.
- Move to previous and next catalog items.
- Display item characteristics, rarity, and price.
- Distinguish unowned, owned, and currently equipped items.
- Prevent purchase when the player has insufficient money.
- Purchasing deducts money and adds the item to the corresponding owned list.
- An owned item can be equipped.

## Battle scenarios

- Load player stats from their profile and equipped items.
- Start at wave 1.
- Spawn up to nine enemies; the number increases with the wave and is capped at
  nine.
- A player must select a living target before attacking.
- Attacking reduces the selected enemy's health using the current battle rules.
- Killing an enemy grants its reward.
- When all enemies are killed, allow or initiate the next wave.
- Between waves, heal the player when current health is at most half of maximum
  health, preserving the existing rule until it is intentionally redesigned.
- Healing and damage-boost abilities respect their level requirements and
  cooldowns.
- Player death ends the battle and produces a result.
- Battle completion updates money, experience, level, and wave record.
- The results screen displays wave, reward, experience, weapon, and armor.

## Keyboard behavior

- Enter leaves the initial splash screen.
- `A` attacks the selected enemy.
- `Z` activates healing.
- `X` activates damage boost.

## Legacy persistence formats

### `id.txt`

Accounts are stored as repeating three-line records:

```text
Login: <login>
Password: <plaintext password>
ID: <numeric id>
```

### `logged.txt`

Logged-out form:

```text
false
```

Remembered-user form:

```text
true
Login: <login>
Password: <plaintext password>
ID: <numeric id>
```

### `users/<id>.txt`

Line positions are part of the current implicit schema:

| Line | Meaning |
| ---: | --- |
| 0 | Nickname |
| 1 | Level |
| 2 | Experience in the current level |
| 3 | Money |
| 4 | Equipped weapon ID |
| 5 | Space-separated owned weapon IDs |
| 6 | Equipped armor ID |
| 7 | Space-separated owned armor IDs |

### `records/<id>.txt`

The first line contains the highest completed wave.

### `inventory/<item-id>.txt`

| Line | Meaning |
| ---: | --- |
| 0 | Damage for a weapon or protection for armor |
| 1 | Price |
| 2 | Numeric rarity |

### `enemies/<rank>.txt`

| Line | Meaning |
| ---: | --- |
| 0 | Rank |
| 1 | Name |
| 2 | Health |
| 3 | Damage |
| 4 | Protection |
| 5 | Reward |

## Migration invariants

- A migrated profile must retain identity, level, experience, money, ownership,
  equipment, and record.
- Navigation must not create a new main application window for each screen.
- Leaving a screen must stop or cancel its timers and background operations.
- Persistence must not be accessed directly by views or view models.
- Domain and application services must not reference WPF controls.
- Existing save files must either be migrated or produce a clear recovery error;
  they must never be silently overwritten after a parse failure.
- Battle rules must have characterization tests before intentional balancing
  changes are made.

## Known baseline risks

- Passwords are stored in plaintext.
- Save schemas depend on physical line numbers.
- Application and mutable user data paths depend on the process working
  directory.
- UI state is synchronized by high-frequency file polling.
- `BackgroundWorker` and blocking sleeps are used for cooldowns and transitions.
- Random generators are repeatedly seeded from the current time.
- Battle state is represented partly by WPF controls rather than domain objects.
- There are no automated tests in the current solution.

