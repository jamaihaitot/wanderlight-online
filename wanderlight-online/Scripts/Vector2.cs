namespace WanderlightOnline
{
    /// <summary>
    /// Simple 2D vector for player position.
    /// </summary>
    // <copyright file="Vector2.cs" company="Wanderlight Online"/>
    // Copyright (c) 2025 Wanderlight Online
    // </copyright>
    // Vector2.cs
    using System;

    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x = 0, float y = 0)
        {
            this.X = x;
            this.Y = y;
        }


        public float Length => (float)Math.Sqrt((this.X * this.X) + (this.Y * this.Y));

        public Vector2 Normalized()
        {
            float len = this.Length;
            if (len == 0) { return new Vector2(0, 0); }
            return new Vector2(this.X / len, this.Y / len);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 v, float scalar) => new Vector2(v.X * scalar, v.Y * scalar);
        public static Vector2 operator *(float scalar, Vector2 v) => new Vector2(v.X * scalar, v.Y * scalar);
        public static Vector2 operator /(Vector2 v, float scalar) => new Vector2(v.X / scalar, v.Y / scalar);
    }
}
