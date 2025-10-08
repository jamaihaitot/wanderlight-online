# Login Contract

**Integration Point**: MainMenuUI ↔ PlayerManager  
**Purpose**: Define the interface for player authentication and game join process  
**Status**: Active  
**Version**: 1.0.0

---

## Overview

This contract specifies the interaction between the MainMenu UI (client) and the PlayerManager (backend) for adding a new player to the game session.

---

## Interface Definition

### Client Component

**Class**: `MainMenuController.cs`  
**Scene**: `MainMenu.tscn`  
**Responsibility**: Collect display name, validate input, trigger join action

### Backend Component

**Class**: `PlayerManager.cs` (Phase 3 - already implemented)  
**Responsibility**: Validate uniqueness, add player to SpacetimeDB, return player ID

---

## Method Signatures

### Primary Method: TryAddPlayer

```csharp
// Backend (PlayerManager.cs)
public Result<PlayerId> TryAddPlayer(string displayName)
{
    // Validate uniqueness (case-insensitive)
    // Add to SpacetimeDB player table
    // Return player ID or error message
}
```

**Parameters**:

- `displayName` (string): Player's chosen display name
  - Pre-validated by client (3-20 chars, alphanumeric + spaces)
  - Backend validates uniqueness

**Returns**:

- `Result<PlayerId>`: Success with PlayerId, or Error with message

**Success Response**:

```csharp
Result.Success(new PlayerId { Value = 12345 })
```

**Error Responses**:
| Error Message | Cause | Client Action |
|---------------|-------|---------------|
| "Name already taken" | Display name exists (case-insensitive) | Show error, allow retry |
| "Invalid name format" | Name failed backend validation | Show error, allow retry |
| "Database connection error" | SpacetimeDB unavailable | Show "Connection failed", retry later |

---

## Events

### OnPlayerAdded (Success Path)

```csharp
// Backend event (triggered after successful DB insert)
public event Action<PlayerId, string, float, float> OnPlayerAdded;

// Fired when: Player successfully added to SpacetimeDB
// Parameters:
//   - playerId: Unique identifier
//   - displayName: Validated display name
//   - spawnX: World spawn X coordinate
//   - spawnY: World spawn Y coordinate
```

**Client Handler**:

```csharp
// GameManager.cs
DatabaseManager.Instance.OnPlayerAdded += (playerId, displayName, x, y) =>
{
    if (playerId == LocalPlayerId)
    {
        SpawnLocalPlayer(playerId, displayName, new Vector2(x, y));
        TransitionToGameWorld();
    }
    else
    {
        SpawnRemotePlayer(playerId, displayName, new Vector2(x, y));
    }
};
```

### OnPlayerAddFailed (Error Path)

```csharp
// Backend event (if available, otherwise handled via Result<T>)
public event Action<string> OnPlayerAddFailed;

// Fired when: Player add operation fails
// Parameters:
//   - errorMessage: User-friendly error description
```

**Client Handler**:

```csharp
// MainMenuController.cs
private void OnJoinButtonPressed()
{
    var result = PlayerManager.Instance.TryAddPlayer(displayName);

    if (!result.IsSuccess)
    {
        ShowError(result.ErrorMessage);
    }
    else
    {
        // Wait for OnPlayerAdded event
    }
}
```

---

## Data Flow

### Happy Path (Successful Join)

```
1. User enters "Alice" → LineEdit
2. User clicks "Join Game" → Button.Pressed event
3. MainMenuController validates locally:
   - Length: 3-20 chars ✓
   - Characters: Alphanumeric + spaces ✓
4. MainMenuController calls PlayerManager.TryAddPlayer("Alice")
5. PlayerManager validates uniqueness in SpacetimeDB
6. SpacetimeDB inserts player record (atomic operation)
7. PlayerManager returns Result.Success(PlayerId=123)
8. OnPlayerAdded event fires with (123, "Alice", 400, 300)
9. GameManager spawns LocalPlayer at (400, 300)
10. GameManager transitions from MainMenu to GameWorld scene
```

### Error Path (Duplicate Name)

```
1. User enters "Bob" (already exists)
2. User clicks "Join Game"
3. MainMenuController validates locally ✓
4. MainMenuController calls PlayerManager.TryAddPlayer("Bob")
5. PlayerManager checks SpacetimeDB: "Bob" exists (case-insensitive)
6. PlayerManager returns Result.Error("Name already taken")
7. MainMenuController displays error label
8. User can retry with different name
```

### Error Path (Connection Failure)

```
1. User enters "Charlie"
2. User clicks "Join Game"
3. MainMenuController validates locally ✓
4. MainMenuController calls PlayerManager.TryAddPlayer("Charlie")
5. PlayerManager attempts SpacetimeDB connection
6. Connection timeout (network issue)
7. PlayerManager returns Result.Error("Connection failed. Please try again.")
8. MainMenuController displays error with retry button
```

---

## Validation Rules

### Client-Side Validation (Pre-Flight)

Performed **before** calling backend:

| Rule                  | Check                                      | Error Message                                                             |
| --------------------- | ------------------------------------------ | ------------------------------------------------------------------------- |
| Not Empty             | `!string.IsNullOrWhiteSpace(name)`         | "Display name cannot be empty"                                            |
| Min Length            | `name.Length >= 3`                         | "Display name must be at least 3 characters"                              |
| Max Length            | `name.Length <= 20`                        | "Display name must be at most 20 characters"                              |
| Valid Characters      | `Regex.IsMatch(name, @"^[a-zA-Z0-9_ ]+$")` | "Display name can only contain letters, numbers, spaces, and underscores" |
| No Consecutive Spaces | `!name.Contains("  ")`                     | "Display name cannot contain consecutive spaces"                          |

### Backend Validation

Performed **by PlayerManager**:

| Rule               | Check                                | Error Message         |
| ------------------ | ------------------------------------ | --------------------- |
| Unique Name        | Query SpacetimeDB (case-insensitive) | "Name already taken"  |
| Trimmed            | Remove leading/trailing spaces       | N/A (automatic)       |
| Re-validate Format | Same as client rules                 | "Invalid name format" |

**Why Double Validation?**

- Client: Immediate feedback, better UX
- Backend: Security (never trust client), authoritative

---

## Error Handling

### Client Responsibilities

- **Display Errors**: Show user-friendly message in UI
- **Allow Retry**: Keep user on MainMenu, clear error on new input
- **Log Errors**: Console.WriteLine for debugging (browser dev tools)

### Backend Responsibilities

- **Return Errors**: Use Result<T> pattern, never throw exceptions to client
- **Log Errors**: Structured logging with context (attempted name, timestamp)
- **Monitor**: Track failed join attempts for operational visibility

---

## Testing Scenarios

### Manual Test 1: Successful Join

```
GIVEN MainMenu is displayed
WHEN user enters "Alice" (unique name)
  AND clicks "Join Game"
THEN OnPlayerAdded event fires
  AND GameWorld scene loads
  AND player spawns at designated spawn point
  AND display name "Alice" appears above character
```

### Manual Test 2: Duplicate Name

```
GIVEN player "Bob" is already in game
WHEN new user enters "Bob" (case-insensitive match)
  AND clicks "Join Game"
THEN error label displays "Name already taken"
  AND user remains on MainMenu
  AND can enter different name
```

### Manual Test 3: Invalid Characters

```
GIVEN MainMenu is displayed
WHEN user enters "Test@123" (contains @)
  AND clicks "Join Game"
THEN error label displays "Display name can only contain letters, numbers, spaces, and underscores"
  AND backend is NOT called (client-side validation)
```

### Manual Test 4: Name Too Short

```
GIVEN MainMenu is displayed
WHEN user enters "AB" (2 characters)
  AND clicks "Join Game"
THEN error label displays "Display name must be at least 3 characters"
  AND backend is NOT called
```

### Manual Test 5: Connection Failure

```
GIVEN SpacetimeDB server is stopped
WHEN user enters valid name "Charlie"
  AND clicks "Join Game"
THEN error label displays "Connection failed. Please try again."
  AND user can retry (server might recover)
```

---

## Performance Requirements

| Metric             | Target | Measurement                   |
| ------------------ | ------ | ----------------------------- |
| Client Validation  | <10ms  | Regex check time              |
| Backend Validation | <50ms  | Database query time           |
| Total Join Time    | <200ms | Button click → GameWorld load |
| Error Display      | <50ms  | Result → Error label visible  |

---

## Dependencies

### Client Dependencies

- `DisplayNameValidator.cs`: Static validation logic
- `MainMenuController.cs`: UI controller
- `GameManager.cs`: Scene transition coordinator

### Backend Dependencies

- `PlayerManager.cs`: Player lifecycle management
- `DatabaseManager.cs`: SpacetimeDB connection
- `SpacetimeDB player table`: Persistent storage

---

## Example Implementation

### Client (MainMenuController.cs)

```csharp
public partial class MainMenuController : Control
{
    private LineEdit displayNameInput;
    private Button joinButton;
    private Label errorLabel;

    private void OnJoinButtonPressed()
    {
        string name = displayNameInput.Text;

        // Client-side validation
        var validation = DisplayNameValidator.Validate(name);
        if (!validation.IsSuccess)
        {
            ShowError(validation.ErrorMessage);
            return;
        }

        // Clear previous error
        errorLabel.Visible = false;

        // Disable button during backend call
        joinButton.Disabled = true;

        // Call backend (synchronous for simplicity - Godot main thread)
        var result = PlayerManager.Instance.TryAddPlayer(name);

        if (!result.IsSuccess)
        {
            ShowError(result.ErrorMessage);
            joinButton.Disabled = false;
        }
        else
        {
            // Success handled by OnPlayerAdded event
            // GameManager will transition scenes
        }
    }

    private void ShowError(string message)
    {
        errorLabel.Text = message;
        errorLabel.Visible = true;
    }
}
```

### Backend (PlayerManager.cs - Already Implemented)

```csharp
// Phase 3 implementation (reference only)
public Result<PlayerId> TryAddPlayer(string displayName)
{
    // Trim and validate format
    displayName = displayName.Trim();
    if (!IsValidDisplayName(displayName))
    {
        return Result.Error("Invalid name format");
    }

    // Check uniqueness (case-insensitive)
    if (IsNameTaken(displayName))
    {
        return Result.Error("Name already taken");
    }

    try
    {
        // Add to SpacetimeDB
        var playerId = DatabaseManager.Instance.AddPlayer(displayName);

        // Fire event (triggers OnPlayerAdded subscription)
        OnPlayerAdded?.Invoke(playerId, displayName, spawnX, spawnY);

        return Result.Success(playerId);
    }
    catch (DatabaseException ex)
    {
        LogError("Failed to add player", ex);
        return Result.Error("Connection failed. Please try again.");
    }
}
```

---

## Versioning

**Version**: 1.0.0  
**Breaking Changes**: None (initial version)  
**Migration**: N/A

**Future Enhancements** (out of scope for MVP):

- Email/password authentication
- OAuth integration (Steam, Discord)
- Display name reservation system
- Profanity filter for display names

---

## Status

- [x] Contract defined
- [x] Backend implementation exists (Phase 3)
- [ ] Client implementation pending (Phase 4.0)
- [ ] Manual testing pending (Phase 4.0)

---

**Last Updated**: October 8, 2025  
**Reviewed By**: Phase 4.0 Planning Process
