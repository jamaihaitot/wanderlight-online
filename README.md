# Wanderlight Online - MMORPG Foundation

A 2D browser-first MMORPG built with Godot and SpacetimeDB.

## Prerequisites

- Godot 4.x with C# support
- .NET 8.0 SDK
- SpacetimeDB CLI

## Setup

### 1. Install Dependencies

#### Godot Testing Framework
```bash
# Install gdUnit4 plugin (v5.0.3+)
# In Godot Editor: Project → Project Settings → Plugins → AssetLib → Search "gdUnit4"
```

### 2. Build & Run Tests

```bash
# Build the test project
dotnet build WanderlightOnline.Tests/WanderlightOnline.Tests.csproj

# Run all tests
dotnet test WanderlightOnline.Tests/WanderlightOnline.Tests.csproj --logger "console;verbosity=normal"
```

### 3. SpacetimeDB Setup

```bash
# Start SpacetimeDB server
spacetime start

# Build and publish the server module
dotnet build spacetime-module/wanderlight-server -c Release
```

## Project Structure

```
wanderlight-online/           # Main Godot project
├── Scripts/                  # C# game scripts
├── Scenes/                   # Godot scene files
├── Assets/                   # Game assets
└── addons/                   # Third-party plugins (not committed)

WanderlightOnline.Tests/      # Test project
├── Contract/                 # Contract/integration tests
├── Model/                    # Unit tests for game models
└── Integration/              # End-to-end integration tests

spacetime-module/             # SpacetimeDB server module
└── wanderlight-server/       # C# server logic
```

## Development Workflow

1. **TDD Approach**: Write tests first, then implement
2. **Phase 3.2**: All tests created ✅
3. **Phase 3.3**: Core implementation (current phase)
4. **Phase 3.4**: Integration with SpacetimeDB
5. **Phase 3.5**: Polish and optimization