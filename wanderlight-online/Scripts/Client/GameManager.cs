// <copyright file="GameManager.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using Godot;

    /// <summary>
    /// GameManager: Singleton for managing game state and scene coordination.
    /// </summary>
    public partial class GameManager : Node
    {
        private static GameManager instance;

        /// <summary>
        /// Gets the singleton instance of GameManager.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the player's display name.
        /// </summary>
        public string PlayerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game is connected to SpacetimeDB.
        /// </summary>
        public bool IsConnected { get; set; }

        private GameManager()
        {
            this.PlayerDisplayName = string.Empty;
            this.IsConnected = false;
        }
    }
}
