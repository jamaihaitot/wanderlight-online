// <copyright file="InventoryUI.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using Godot;
    using System.Collections.Generic;

    /// <summary>
    /// InventoryUI: Manages inventory panel UI and interactions.
    /// </summary>
    public partial class InventoryUI : Panel
    {
        private const int InventorySlots = 12;
        
        private List<Button> slotButtons = new List<Button>();
        private Inventory inventory;
        private GridContainer gridContainer;

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            this.gridContainer = GetNode<GridContainer>("VBoxContainer/GridContainer");
            
            // Get all slot buttons
            for (int i = 0; i < InventorySlots; i++)
            {
                Button slotButton = this.gridContainer.GetChild<Button>(i);
                int slotIndex = i;
                slotButton.Pressed += () => this.OnSlotClicked(slotIndex);
                this.slotButtons.Add(slotButton);
            }

            // Initialize inventory
            this.inventory = new Inventory();

            this.Visible = false;
            GD.Print("[InventoryUI] Initialized");
        }

        /// <summary>
        /// Called every frame for input processing.
        /// </summary>
        /// <param name="delta">Time elapsed since last frame.</param>
        public override void _Process(double delta)
        {
            // Toggle inventory with TAB key
            if (Input.IsKeyPressed(Key.Tab))
            {
                this.Visible = !this.Visible;
            }
        }

        /// <summary>
        /// Updates the UI to reflect current inventory state.
        /// </summary>
        public void RefreshInventory()
        {
            var slots = this.inventory.Slots;
            
            for (int i = 0; i < InventorySlots; i++)
            {
                if (i < slots.Count && slots[i] != null)
                {
                    ItemStack item = slots[i];
                    this.slotButtons[i].Text = $"{item.ItemType}\n({item.Quantity})";
                }
                else
                {
                    this.slotButtons[i].Text = "Empty";
                }
            }
        }

        private void OnSlotClicked(int slotIndex)
        {
            var slot = this.inventory.GetSlot(slotIndex);
            
            if (slot != null)
            {
                GD.Print($"[InventoryUI] Clicked slot {slotIndex}: {slot.ItemType} x{slot.Quantity}");
                
                // TODO: Implement drop to world functionality
                // For now, just remove from inventory
                if (this.inventory.TryRemove(slot.ItemType, 1))
                {
                    this.RefreshInventory();
                    GD.Print($"[InventoryUI] Removed 1x {slot.ItemType}");
                }
            }
        }
    }
}
