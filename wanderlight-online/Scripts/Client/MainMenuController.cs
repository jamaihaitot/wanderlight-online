// <copyright file="MainMenuController.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using Godot;

    /// <summary>
    /// MainMenuController: Handles login screen UI and player authentication.
    /// </summary>
    public partial class MainMenuController : Control
    {
        private LineEdit displayNameInput;
        private Button joinButton;
        private Label errorLabel;
        private PlayerManager playerManager;

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            this.displayNameInput = GetNode<LineEdit>("CenterContainer/VBoxContainer/MarginContainer/VBoxContainer2/DisplayNameInput");
            this.joinButton = GetNode<Button>("CenterContainer/VBoxContainer/MarginContainer/VBoxContainer2/JoinButton");
            this.errorLabel = GetNode<Label>("CenterContainer/VBoxContainer/MarginContainer/VBoxContainer2/ErrorLabel");

            this.joinButton.Pressed += this.OnJoinButtonPressed;
            this.displayNameInput.TextSubmitted += this.OnTextSubmitted;

            this.playerManager = new PlayerManager();
        }

        private void OnTextSubmitted(string text)
        {
            this.OnJoinButtonPressed();
        }

        private void OnJoinButtonPressed()
        {
            string displayName = this.displayNameInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                this.ShowError("Please enter a display name.");
                return;
            }

            // Validate display name length (3-20 characters)
            if (displayName.Length < 3 || displayName.Length > 20)
            {
                this.ShowError("Display name must be 3-20 characters.");
                return;
            }

            // Try to add player
            if (this.playerManager.TryAddPlayer(displayName, out string? error))
            {
                GD.Print($"[MainMenu] Player '{displayName}' joined successfully!");
                
                // Store the display name for use in GameWorld
                GameManager.Instance.PlayerDisplayName = displayName;
                
                // Transition to GameWorld scene
                this.GetTree().ChangeSceneToFile("res://Scenes/GameWorld.tscn");
            }
            else
            {
                this.ShowError(error ?? "Failed to join. Please try another name.");
            }
        }

        private void ShowError(string message)
        {
            this.errorLabel.Text = message;
            this.errorLabel.Visible = true;
        }
    }
}
