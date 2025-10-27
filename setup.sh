#!/bin/bash
# Quick setup script for Wanderlight Online Phase 4.0

set -e

echo "=== Wanderlight Online Phase 4.0 Setup ==="
echo ""

# Check prerequisites
echo "Checking prerequisites..."

# Check .NET
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8.0 SDK."
    echo "   Download from: https://dotnet.microsoft.com/download"
    exit 1
fi
echo "✅ .NET SDK found: $(dotnet --version)"

# Check Godot
if ! command -v godot &> /dev/null; then
    echo "⚠️  Godot not found in PATH."
    echo "   Make sure Godot 4.4+ with C# support is installed."
    echo "   You can still open the project manually."
else
    echo "✅ Godot found: $(godot --version | head -1)"
fi

# Check SpacetimeDB
if ! command -v spacetime &> /dev/null; then
    echo "⚠️  SpacetimeDB not found."
    echo "   For multiplayer, install from: https://spacetimedb.com/install"
else
    echo "✅ SpacetimeDB found: $(spacetime version)"
fi

echo ""
echo "=== Building C# Project ==="
cd "$(dirname "$0")/wanderlight-online"
dotnet build "Wanderlight Online.csproj"

echo ""
echo "=== Running Tests ==="
dotnet test "Wanderlight Online.csproj" --no-build

echo ""
echo "✅ Setup complete!"
echo ""
echo "Next steps:"
echo "1. Start SpacetimeDB: spacetime start"
echo "2. Open project in Godot: godot --path wanderlight-online"
echo "3. Press F5 to play"
echo ""
echo "See PLAYING.md for detailed instructions."
