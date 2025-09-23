using System;

namespace WanderlightOnline
{
    /// <summary>
    /// Represents the category of an item, determining its stacking behavior.
    /// </summary>
    public enum ItemCategory
    {
        /// <summary>
        /// Generic items with default stacking rules (max 20 per stack).
        /// </summary>
        Generic,

        /// <summary>
        /// Consumable items that stack up to 20 per stack.
        /// </summary>
        Consumable,

        /// <summary>
        /// Equipment items that don't stack (max 1 per stack).
        /// </summary>
        Equipment
    }
}