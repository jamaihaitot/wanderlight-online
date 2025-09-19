
// Copyright (c) 2025 Wanderlight Online
// PlayerManager.cs
namespace WanderlightOnline
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// PlayerManager: Handles player authentication, connection, state, and contract enforcement
    /// </summary>
    public class PlayerManager
    {
        private HashSet<string> activeDisplayNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, Player> players = new Dictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Attempts to add a player with a display name
        /// </summary>
        public bool TryAddPlayer(string displayName, out string error)
        {
            displayName = displayName.Trim();
            if (!this.IsValidDisplayName(displayName))
            {
                error = "Invalid display name.";
                return false;
            }

            if (this.activeDisplayNames.Contains(displayName))
            {
                error = "Display name already taken.";
                return false;
            }

            this.activeDisplayNames.Add(displayName);
            this.players[displayName] = new Player(displayName);
            error = null;
            return true;
        }

        /// <summary>
        /// Removes a player (disconnect)
        /// </summary>
        public void RemovePlayer(string displayName)
        {
            this.activeDisplayNames.Remove(displayName);
            this.players.Remove(displayName);
        }

        /// <summary>
        /// Checks display name validity (format, profanity, length)
        /// </summary>
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
        /// Gets player by display name
        /// </summary>
        public Player GetPlayer(string displayName)
        {
            this.players.TryGetValue(displayName, out var player);
            return player;
        }
    }
}
