// <copyright file="Inventory.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>
namespace WanderlightOnline
{
    /// <summary>
    /// Inventory for player items. Stores item list and capacity.
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// Gets the maximum number of items the inventory can hold.
        /// </summary>
        public int Capacity { get; } = 12;

        /// <summary>
        /// Gets the list of items in the inventory.
        /// </summary>
        public System.Collections.Generic.List<object> Items { get; } = new System.Collections.Generic.List<object>();

        // Add more inventory logic as needed
    }
}
