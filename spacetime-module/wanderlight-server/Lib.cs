using SpacetimeDB;

public static partial class Module
{
    // Minimal Player table
    [SpacetimeDB.Table(Name = "player", Public = true)]
    public partial struct Player
    {
        [SpacetimeDB.PrimaryKey]
        public Identity Identity;
        public string DisplayName;
        public float PositionX;
        public float PositionY;
        public bool Online;
    }

    // Minimal Inventory table
    [SpacetimeDB.Table(Name = "inventory", Public = true)]
    public partial struct InventoryItem
    {
        public Identity PlayerId;
        public string ItemType;
        public int Quantity;
        public int SlotIndex;
    }

    // Minimal WorldItem table
    [SpacetimeDB.Table(Name = "world_item", Public = true)]
    public partial struct WorldItem
    {
        [SpacetimeDB.PrimaryKey]
        public uint ItemId;
        public string ItemType;
        public float PositionX;
        public float PositionY;
        public int StackSize;
    }

    // Player join/leave reducers
    [SpacetimeDB.Reducer(ReducerKind.ClientConnected)]
    public static void PlayerConnect(ReducerContext ctx)
    {
        var existing = ctx.Db.player.Identity.Find(ctx.Sender);
        if (existing != null)
        {
            var updatedPlayer = existing.Value;
            updatedPlayer.Online = true;
            ctx.Db.player.Identity.Update(updatedPlayer);
            Log.Info($"Player {updatedPlayer.DisplayName} reconnected");
        }
        else
        {
            var newPlayer = ctx.Db.player.Insert(new Player
            {
                Identity = ctx.Sender,
                DisplayName = "",
                PositionX = 0,
                PositionY = 0,
                Online = true
            });
            Log.Info($"New player connected: {newPlayer.Identity}");
        }
    }

    [SpacetimeDB.Reducer(ReducerKind.ClientDisconnected)]
    public static void PlayerDisconnect(ReducerContext ctx)
    {
        var player = ctx.Db.player.Identity.Find(ctx.Sender);
        if (player != null)
        {
            var updatedPlayer = player.Value;
            updatedPlayer.Online = false;
            ctx.Db.player.Identity.Update(updatedPlayer);
            Log.Info($"Player {updatedPlayer.DisplayName} disconnected");
        }
    }

    // Basic reducers
    [SpacetimeDB.Reducer]
    public static void SetDisplayName(ReducerContext ctx, string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 16)
        {
            throw new ArgumentException("Invalid display name: must be 1-16 characters");
        }

        var player = ctx.Db.player.Identity.Find(ctx.Sender);
        if (player != null)
        {
            var updatedPlayer = player.Value;
            updatedPlayer.DisplayName = name;
            ctx.Db.player.Identity.Update(updatedPlayer);
            Log.Info($"Player {ctx.Sender} set name to: {name}");
        }
        else
        {
            throw new InvalidOperationException("Player not found");
        }
    }

    [SpacetimeDB.Reducer]
    public static void UpdatePosition(ReducerContext ctx, float x, float y)
    {
        var player = ctx.Db.player.Identity.Find(ctx.Sender);
        if (player != null)
        {
            var updatedPlayer = player.Value;
            updatedPlayer.PositionX = x;
            updatedPlayer.PositionY = y;
            ctx.Db.player.Identity.Update(updatedPlayer);
            // Log.Info($"Player {updatedPlayer.DisplayName} moved to ({x}, {y})"); // Uncomment for debugging
        }
        else
        {
            throw new InvalidOperationException("Player not found");
        }
    }

    // Simple world reset for testing
    [SpacetimeDB.Reducer]
    public static void ResetWorld(ReducerContext ctx)
    {
        // Clear all world items
        foreach (var item in ctx.Db.world_item.Iter())
        {
            ctx.Db.world_item.ItemId.Delete(item.ItemId);
        }

        // Clear all inventory items
        foreach (var item in ctx.Db.inventory.Iter())
        {
            ctx.Db.inventory.Delete(item);
        }

        Log.Info("World reset completed");
    }
}
