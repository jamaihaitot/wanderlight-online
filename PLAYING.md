# Playing Wanderlight Online - Phase 4.0 MVP

## Quick Start Guide

### Prerequisites

1. **Godot Engine 4.4+** with C# support (Mono/dotnet)
   - Download from: https://godotengine.org/download
   - Make sure to get the .NET version

2. **SpacetimeDB** (for multiplayer backend)
   - Download from: https://spacetimedb.com/install
   - Start the server: `spacetime start`

3. **.NET 8.0 SDK**
   - Required for C# compilation
   - Download from: https://dotnet.microsoft.com/download

### Setup Instructions

#### 1. Start SpacetimeDB Server

```bash
# Start the SpacetimeDB server
spacetime start

# In another terminal, deploy the game module
cd spacetime-module/wanderlight-server
dotnet build -c Release
spacetime publish
```

#### 2. Open the Project in Godot

```bash
# Open the project
godot --path wanderlight-online

# Or double-click on project.godot in Godot's project manager
```

#### 3. Build the C# Project

In Godot Editor:
- Click "Build" in the top menu
- Wait for compilation to complete
- Check the output console for any errors

#### 4. Run the Game

**Option A: In Godot Editor**
- Press F5 or click the "Play" button
- The MainMenu scene will launch

**Option B: Export to HTML5**
- Go to Project → Export
- Select "Web (HTML5)" preset
- Click "Export Project"
- Choose output folder (e.g., `builds/html5/`)
- Open `index.html` in a modern browser

## How to Play

### Main Menu
1. Enter a unique display name (3-20 characters)
2. Click "Join Game"
3. If the name is already taken, you'll see an error message

### In-Game Controls

**Movement:**
- W / Up Arrow: Move up
- S / Down Arrow: Move down
- A / Left Arrow: Move left
- D / Right Arrow: Move right

**Inventory:**
- TAB: Open/close inventory panel
- Click an inventory slot to drop an item

**Item Interaction:**
- E: Pick up nearby items (within 50 units)

### Multiplayer Testing

To test multiplayer:

1. Launch the game in Godot editor (press F5)
2. Enter a display name and join (e.g., "Player1")
3. Open a second instance:
   - Export to HTML5 and open in browser
   - OR run from command line: `godot --path wanderlight-online`
4. Enter a different display name (e.g., "Player2")
5. Both players should see each other moving in real-time

### What Works in Phase 4.0 MVP

✅ **Implemented:**
- Login with unique display name validation
- Player movement with WASD/Arrow keys
- Real-time position synchronization
- Remote player rendering with interpolation
- Basic inventory UI (12 slots)
- World item spawning and interaction
- Connection status display
- Camera following player

⚠️ **Limited/Stub:**
- SpacetimeDB integration (events need wiring)
- Actual item pickup/drop (backend exists, needs UI connection)
- World item persistence (backend exists)
- Player state restoration (backend exists)

❌ **Not Implemented (Out of Scope for Phase 4.0):**
- Combat system
- NPCs and enemies
- Quests or objectives
- Trading between players
- Chat system
- Animations
- Sound effects
- Mobile controls

## Troubleshooting

### Build Errors

**"Missing Godot assemblies"**
- Solution: Click "Build" in Godot editor first
- The project uses Godot 4.4 with C# support

**"Missing SpacetimeDB"**
- Solution: Ensure SpacetimeDB is installed and running
- Check: `spacetime version`

### Runtime Issues

**"Player name already taken" error**
- Solution: Use a different display name
- Each player needs a unique name

**"Cannot see other players"**
- Check: Is SpacetimeDB server running?
- Check: Are both clients connected to the same server?
- Check: Network events may need manual triggering (Phase 4.0 limitation)

**"Inventory doesn't open"**
- Press TAB key (not ESC)
- Make sure the game window has focus

### Performance Issues

**Low FPS in browser:**
- Try a different browser (Chrome/Firefox recommended)
- Disable browser extensions
- Close other tabs

**High latency:**
- Check network connection
- SpacetimeDB should be running locally for testing
- Target: <200ms latency for smooth gameplay

## Development Notes

### Architecture

**Scenes:**
- `MainMenu.tscn` - Login screen
- `GameWorld.tscn` - Main game scene
- `Player.tscn` - Local player character
- `RemotePlayer.tscn` - Remote player instances
- `WorldItem.tscn` - Items on the ground
- `UI/InventoryPanel.tscn` - Inventory UI
- `UI/HUD.tscn` - Connection status and instructions

**Scripts:**
- `Client/MainMenuController.cs` - Login logic
- `Client/GameManager.cs` - Singleton for game state
- `Client/LocalPlayerController.cs` - Player input and movement
- `Client/RemotePlayerRenderer.cs` - Remote player sync
- `Client/WorldItemRenderer.cs` - World item management
- `Client/InventoryUI.cs` - Inventory UI logic

### Network Architecture

**Update Rate:** 20Hz (50ms intervals)
**Protocol:** WebSocket via SpacetimeDB
**Interpolation:** Linear interpolation for smooth remote player movement

### Known Limitations (Phase 4.0)

1. **Network Events**: SpacetimeDB subscriptions need manual wiring
2. **Item Persistence**: Backend exists but not fully connected to UI
3. **State Restoration**: Implemented in backend but not in client flow
4. **Testing**: Manual testing only (no automated UI tests)

## Next Steps (Phase 5.0+)

- Complete SpacetimeDB integration
- Add item persistence and synchronization
- Implement player state restoration on reconnect
- Add sound effects and music
- Create more world content (maps, items, etc.)
- Implement combat system
- Add NPC interactions
- Mobile-friendly controls

## Testing Checklist

Use this checklist for manual testing:

- [ ] Launch game successfully
- [ ] Enter display name and join
- [ ] Display name validation works (reject duplicates)
- [ ] Player appears in game world
- [ ] Can move with WASD/Arrow keys
- [ ] Camera follows player
- [ ] Open inventory with TAB
- [ ] Launch second instance with different name
- [ ] Second player appears on first client
- [ ] Both players can see each other moving
- [ ] Movement is smooth (interpolated)
- [ ] Pick up items with E key (if items exist)
- [ ] Items appear in inventory
- [ ] Click inventory slot to drop item

## Support

For issues or questions:
- Check the MANUAL_TESTING_GUIDE.md for detailed test scenarios
- Review README.md for project overview
- See specs/001-wanderlight-online-mmorpg/ for technical specifications
