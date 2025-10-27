// <copyright file="WorldItemRenderer.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using Godot;
    using System.Collections.Generic;

    /// <summary>
    /// WorldItemRenderer: Manages rendering and interaction with world items.
    /// </summary>
    public partial class WorldItemRenderer : Node2D
    {
        private const float PickupRange = 50.0f;
        
        private Dictionary<int, Node2D> worldItems = new Dictionary<int, Node2D>();
        private PackedScene worldItemScene;
        private DatabaseManager databaseManager;
        private Inventory playerInventory;
        private Node2D localPlayer;

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            this.worldItemScene = GD.Load<PackedScene>("res://Scenes/WorldItem.tscn");
            this.databaseManager = DatabaseManager.Instance;
            
            // Get local player reference
            this.localPlayer = GetNode<Node2D>("../LocalPlayer");
            
            // Initialize player inventory
            this.playerInventory = new Inventory();

            GD.Print("[WorldItemRenderer] Initialized");
        }

        /// <summary>
        /// Called every frame for input processing.
        /// </summary>
        /// <param name="delta">Time elapsed since last frame.</param>
        public override void _Process(double delta)
        {
            // Check for pickup input
            if (Input.IsActionJustPressed("ui_accept") || Input.IsKeyPressed(Key.E))
            {
                this.TryPickupNearbyItem();
            }
        }

        /// <summary>
        /// Spawns a world item at the specified position.
        /// </summary>
        /// <param name="itemId">Unique item ID.</param>
        /// <param name="itemName">Item name/type.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="quantity">Item quantity.</param>
        public void SpawnWorldItem(int itemId, string itemName, float x, float y, int quantity)
        {
            if (!this.worldItems.ContainsKey(itemId))
            {
                Node2D item = this.worldItemScene.Instantiate<Node2D>();
                item.GlobalPosition = new Godot.Vector2(x, y);
                
                // Set item label
                Label itemLabel = item.GetNode<Label>("ItemLabel");
                itemLabel.Text = $"{itemName} ({quantity})";

                this.AddChild(item);
                this.worldItems[itemId] = item;

                GD.Print($"[WorldItemRenderer] Spawned item {itemId}: {itemName} ({quantity}) at ({x}, {y})");
            }
        }

        /// <summary>
        /// Removes a world item.
        /// </summary>
        /// <param name="itemId">Unique item ID.</param>
        public void RemoveWorldItem(int itemId)
        {
            if (this.worldItems.TryGetValue(itemId, out Node2D item))
            {
                item.QueueFree();
                this.worldItems.Remove(itemId);
                GD.Print($"[WorldItemRenderer] Removed item {itemId}");
            }
        }

        private void TryPickupNearbyItem()
        {
            if (this.localPlayer == null)
            {
                return;
            }

            Godot.Vector2 playerPos = this.localPlayer.GlobalPosition;
            int? nearestItemId = null;
            float nearestDistance = PickupRange;

            // Find nearest item within pickup range
            foreach (var kvp in this.worldItems)
            {
                int itemId = kvp.Key;
                Node2D item = kvp.Value;
                float distance = playerPos.DistanceTo(item.GlobalPosition);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestItemId = itemId;
                }
            }

            if (nearestItemId.HasValue)
            {
                // TODO: Get item details and add to inventory
                // For now, just remove the item
                this.RemoveWorldItem(nearestItemId.Value);
                GD.Print($"[WorldItemRenderer] Picked up item {nearestItemId.Value}");
            }
        }
    }
}
