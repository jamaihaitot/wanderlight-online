// <copyright file="RemotePlayerRenderer.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using Godot;
    using System.Collections.Generic;

    /// <summary>
    /// RemotePlayerRenderer: Manages rendering and synchronization of remote players.
    /// </summary>
    public partial class RemotePlayerRenderer : Node2D
    {
        private const float InterpolationSpeed = 10.0f;
        
        private Dictionary<string, Node2D> remotePlayers = new Dictionary<string, Node2D>();
        private Dictionary<string, Godot.Vector2> targetPositions = new Dictionary<string, Godot.Vector2>();
        private PackedScene remotePlayerScene;
        private NetworkManager networkManager;

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            this.remotePlayerScene = GD.Load<PackedScene>("res://Scenes/RemotePlayer.tscn");
            this.networkManager = NetworkManager.Instance;

            // Subscribe to network player updates
            this.networkManager.OnPlayerJoined += this.OnPlayerJoined;
            this.networkManager.OnPlayerLeft += this.OnPlayerLeft;
            this.networkManager.OnPlayerMoved += this.OnPlayerMoved;

            GD.Print("[RemotePlayerRenderer] Initialized");
        }

        /// <summary>
        /// Called every frame for rendering.
        /// </summary>
        /// <param name="delta">Time elapsed since last frame.</param>
        public override void _Process(double delta)
        {
            // Interpolate all remote players towards their target positions
            foreach (var kvp in this.remotePlayers)
            {
                string displayName = kvp.Key;
                Node2D playerNode = kvp.Value;

                if (this.targetPositions.TryGetValue(displayName, out Godot.Vector2 target))
                {
                    // Smooth interpolation
                    var currentPos = playerNode.GlobalPosition;
                    var newPos = currentPos.Lerp(target, InterpolationSpeed * (float)delta);
                    playerNode.GlobalPosition = newPos;
                }
            }
        }

        private void OnPlayerJoined(string displayName, float x, float y)
        {
            // Don't render our own player
            if (displayName == GameManager.Instance.PlayerDisplayName)
            {
                return;
            }

            if (!this.remotePlayers.ContainsKey(displayName))
            {
                // Spawn new remote player
                Node2D remotePlayer = this.remotePlayerScene.Instantiate<Node2D>();
                remotePlayer.GlobalPosition = new Godot.Vector2(x, y);
                
                // Set display name
                Label nameLabel = remotePlayer.GetNode<Label>("DisplayNameLabel");
                nameLabel.Text = displayName;

                this.AddChild(remotePlayer);
                this.remotePlayers[displayName] = remotePlayer;
                this.targetPositions[displayName] = new Godot.Vector2(x, y);

                GD.Print($"[RemotePlayerRenderer] Player '{displayName}' joined at ({x}, {y})");
            }
        }

        private void OnPlayerLeft(string displayName)
        {
            if (this.remotePlayers.TryGetValue(displayName, out Node2D playerNode))
            {
                playerNode.QueueFree();
                this.remotePlayers.Remove(displayName);
                this.targetPositions.Remove(displayName);

                GD.Print($"[RemotePlayerRenderer] Player '{displayName}' left");
            }
        }

        private void OnPlayerMoved(string displayName, float x, float y)
        {
            // Don't update our own player
            if (displayName == GameManager.Instance.PlayerDisplayName)
            {
                return;
            }

            // Update target position for interpolation
            this.targetPositions[displayName] = new Godot.Vector2(x, y);
        }

        /// <summary>
        /// Called when the node is removed from the scene tree.
        /// </summary>
        public override void _ExitTree()
        {
            // Unsubscribe from events
            if (this.networkManager != null)
            {
                this.networkManager.OnPlayerJoined -= this.OnPlayerJoined;
                this.networkManager.OnPlayerLeft -= this.OnPlayerLeft;
                this.networkManager.OnPlayerMoved -= this.OnPlayerMoved;
            }
        }
    }
}
