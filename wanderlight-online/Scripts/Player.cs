// <copyright file="Player.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{

    /// <summary>
    /// Represents a player in the Wanderlight Online game.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>Player is connected.</summary>
        Connected,

        /// <summary>Player is ghosted (timed out, but not fully disconnected).</summary>
        Ghosted,

        /// <summary>Player is disconnected.</summary>
        Disconnected,
    }

    /// <summary>
    /// Represents a player entity for contract. Stores display name, position, inventory, and connection state.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class with the specified display name, default position, and inventory.
        /// </summary>
        /// <param name="displayName">The display name for the player.</param>
        public Player(string displayName)
        {
            this.DisplayName = displayName;
            this.Position = new Vector2(0, 0); // Default spawn
            this.Inventory = new WanderlightOnline.Inventory();
            this.State = ConnectionState.Connected;
        }

        /// <summary>
        /// Gets the display name of the player.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets or sets the position of the player in the game world.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the inventory of the player.
        /// </summary>
        public Inventory Inventory { get; set; }

        /// <summary>
        /// Gets or sets the connection state of the player.
        /// </summary>
        public ConnectionState State { get; set; }
    }
}