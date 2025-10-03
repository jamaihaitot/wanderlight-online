// <copyright file="PlayerManager.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>
// PlayerManager.cs
namespace WanderlightOnline
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// PlayerManager: Handles player authentication, connection, state, and contract enforcement.
    /// </summary>
    public class PlayerManager
    {
        private HashSet<string> activeDisplayNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, Player> players = new Dictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

        // Name reservation: display name -> reservation expiry
        private Dictionary<string, DateTime> reservedNames = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        // State restoration: display name -> last known Player state
        private Dictionary<string, Player> disconnectedPlayerStates = new Dictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Attempts to add a player with a display name.
        /// </summary>
        /// <param name="displayName">The display name to add.</param>
        /// <param name="error">Error message if add fails.</param>
        /// <returns>True if player added, false otherwise.</returns>
        public bool TryAddPlayer(string displayName, out string? error)
        {
            displayName = displayName.Trim();
            if (!this.IsValidDisplayName(displayName))
            {
                Console.WriteLine($"[PlayerManager] TryAddPlayer: Invalid display name '{displayName}'");
                error = "Invalid display name.";
                return false;
            }

            // Check active names
            if (this.activeDisplayNames.Contains(displayName))
            {
                Console.WriteLine($"[PlayerManager] TryAddPlayer: Display name already taken '{displayName}'");
                error = "Display name already taken.";
                return false;
            }

            // Check reserved names
            if (this.reservedNames.TryGetValue(displayName, out var expiry))
            {
                if (DateTime.UtcNow < expiry)
                {
                    Console.WriteLine($"[PlayerManager] TryAddPlayer: Display name reserved '{displayName}'");
                    error = "Display name is reserved. Please try again later.";
                    return false;
                }
                else
                {
                    // Reservation expired, remove
                    this.reservedNames.Remove(displayName);
                    this.disconnectedPlayerStates.Remove(displayName);
                }
            }

            this.activeDisplayNames.Add(displayName);

            // Restore state if available
            if (this.disconnectedPlayerStates.TryGetValue(displayName, out var savedPlayer))
            {
                Console.WriteLine($"[PlayerManager] TryAddPlayer: Restoring player state for '{displayName}'");
                // Restore all properties
                var restored = new Player(displayName)
                {
                    Position = savedPlayer.Position,
                    Inventory = savedPlayer.Inventory ?? new Inventory(),
                    State = savedPlayer.State,
                };
                this.players[displayName] = restored;
                this.disconnectedPlayerStates.Remove(displayName);
            }
            else
            {
                Console.WriteLine($"[PlayerManager] TryAddPlayer: Creating new player '{displayName}'");
                // Always initialize inventory for atomicity
                var newPlayer = new Player(displayName)
                {
                    Inventory = new Inventory(),
                };
                this.players[displayName] = newPlayer;
            }

            error = null;
            Console.WriteLine($"[PlayerManager] TryAddPlayer: Success for '{displayName}'");
            return true;
        }

        /// <summary>
        /// Removes a player (disconnect).
        /// </summary>
        /// <param name="displayName">The display name to remove.</param>
        public void RemovePlayer(string displayName)
        {
            this.activeDisplayNames.Remove(displayName);

            // Save state for restoration
            if (this.players.TryGetValue(displayName, out var player))
            {
                this.disconnectedPlayerStates[displayName] = player;

                // Reserve name for 2 minutes
                this.reservedNames[displayName] = DateTime.UtcNow.AddMinutes(2);
            }

            this.players.Remove(displayName);
        }

        /// <summary>
        /// Checks display name validity (format, profanity, length).
        /// </summary>
        /// <param name="displayName">The display name to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValidDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return false;
            }

            if (displayName.Length < 3 || displayName.Length > 16)
            {
                return false;
            }

            foreach (char c in displayName)
            {
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                {
                    return false;
                }
            }

            // TODO: Add profanity filter
            return true;
        }

        /// <summary>
        /// Gets player by display name.
        /// </summary>
        /// <param name="displayName">The display name to look up.</param>
        /// <returns>The Player object, or null if not found.</returns>
        public Player? GetPlayer(string displayName)
        {
            this.players.TryGetValue(displayName, out var player);
            return player;
        }
    }
}
