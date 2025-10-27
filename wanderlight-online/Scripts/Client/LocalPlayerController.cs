// <copyright file="LocalPlayerController.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using Godot;

    /// <summary>
    /// LocalPlayerController: Handles local player input, movement, and synchronization.
    /// </summary>
    public partial class LocalPlayerController : CharacterBody2D
    {
        private const float MoveSpeed = 200.0f;
        private const float NetworkSyncInterval = 0.05f; // 20Hz
        
        private Label displayNameLabel;
        private NetworkManager networkManager;
        private float networkSyncTimer = 0.0f;
        private Godot.Vector2 lastSyncPosition = Godot.Vector2.Zero;

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            this.displayNameLabel = GetNode<Label>("DisplayNameLabel");
            
            // Set display name from GameManager
            string displayName = GameManager.Instance.PlayerDisplayName;
            this.displayNameLabel.Text = displayName;

            // Initialize network manager
            this.networkManager = NetworkManager.Instance;

            GD.Print($"[LocalPlayer] Initialized for player: {displayName}");
        }

        /// <summary>
        /// Called every frame for input processing.
        /// </summary>
        /// <param name="delta">Time elapsed since last frame.</param>
        public override void _Process(double delta)
        {
            this.HandleInput(delta);
            this.SyncPositionToNetwork((float)delta);
        }

        private void HandleInput(double delta)
        {
            // Get input direction
            Godot.Vector2 inputDirection = Godot.Vector2.Zero;

            if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D))
            {
                inputDirection.X += 1;
            }

            if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A))
            {
                inputDirection.X -= 1;
            }

            if (Input.IsActionPressed("ui_down") || Input.IsKeyPressed(Key.S))
            {
                inputDirection.Y += 1;
            }

            if (Input.IsActionPressed("ui_up") || Input.IsKeyPressed(Key.W))
            {
                inputDirection.Y -= 1;
            }

            // Normalize diagonal movement
            if (inputDirection.Length() > 0)
            {
                inputDirection = inputDirection.Normalized();
            }

            // Calculate velocity
            Velocity = inputDirection * MoveSpeed;

            // Move the character
            this.MoveAndSlide();
        }

        private void SyncPositionToNetwork(float delta)
        {
            this.networkSyncTimer += delta;

            // Send position updates at 20Hz (every 0.05 seconds)
            if (this.networkSyncTimer >= NetworkSyncInterval)
            {
                Godot.Vector2 currentPos = this.GlobalPosition;
                
                // Only sync if position changed significantly
                if (currentPos.DistanceTo(this.lastSyncPosition) > 0.1f)
                {
                    var position = new WanderlightOnline.Vector2(currentPos.X, currentPos.Y);
                    this.networkManager.SendPositionUpdate(GameManager.Instance.PlayerDisplayName, position);
                    this.lastSyncPosition = currentPos;
                }

                this.networkSyncTimer = 0.0f;
            }
        }
    }
}
