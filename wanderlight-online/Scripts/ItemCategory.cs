// <copyright file="ItemCategory.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    /// <summary>
    /// Item categories determine stacking rules and behavior.
    /// </summary>
    public enum ItemCategory
    {
        /// <summary>Default category, general items.</summary>
        Generic = 0,

        /// <summary>Consumables/materials stack higher (up to 20).</summary>
        Consumable = 1,

        /// <summary>Equipment does not stack (maximum of 1 per stack).</summary>
        Equipment = 2,
    }
}
