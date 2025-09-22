namespace WanderlightOnline
{
    /// <summary>
    /// Simple 2D vector for player position.
    /// </summary>
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Vector2(float x = 0, float y = 0)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
