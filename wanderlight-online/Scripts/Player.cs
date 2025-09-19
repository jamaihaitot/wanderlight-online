// Copyright (c) 2025 Wanderlight Online
// Player.cs
namespace WanderlightOnline
{
    /// <summary>
    /// Minimal Player entity for contract
    /// </summary>
    public class Player
    {
        public string DisplayName { get; }
        public object Position { get; set; } // TODO: Replace with Vector2
        public object Inventory { get; set; } // TODO: Replace with Inventory type

        public Player(string displayName)
        {
            DisplayName = displayName;
        }
    }
}