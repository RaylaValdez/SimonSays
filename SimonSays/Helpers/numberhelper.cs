using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Game.ClientState.Objects.Types;

namespace SimonSays.Helpers
{
    public struct Angle
    {
        public const float RadToDeg = 180 / MathF.PI;
        public const float DegToRad = MathF.PI / 180;

        public float Rad;
        public float Deg => Rad * RadToDeg;

        public Angle(float Radians = 0) { Rad = Radians; }

        public static Angle FromDirection(Vector2 dir) => FromDirection(dir.X, dir.Y);
        public static Angle FromDirection(float x, float z) => new(MathF.Atan2(x, z));
        public Vector2 ToDirection() => new(Sin(), Cos());

        public static Angle operator +(Angle a, Angle b) => new(a.Rad + b.Rad);
        public static Angle operator -(Angle a, Angle b) => new(a.Rad - b.Rad);
        public static Angle operator -(Angle a) => new(-a.Rad);
        public static Angle operator *(Angle a, float b) => new(a.Rad * b);
        public static Angle operator *(float a, Angle b) => new(a * b.Rad);
        public static Angle operator /(Angle a, float b) => new(a.Rad / b);
        public Angle Abs() => new(Math.Abs(Rad));
        public float Sin() => MathF.Sin(Rad);
        public float Cos() => MathF.Cos(Rad);
        public float Tan() => MathF.Tan(Rad);
        public static Angle Asin(float x) => new(MathF.Asin(x));
        public static Angle Acos(float x) => new(MathF.Acos(x));

        public Angle Normalized()
        {
            var r = Rad;
            while (r < -MathF.PI)
                r += 2 * MathF.PI;
            while (r > MathF.PI)
                r -= 2 * MathF.PI;
            return new(r);
        }

        public bool AlmostEqual(Angle other, float epsRad)
        {
            var delta = Math.Abs(Rad - other.Rad);
            return delta <= epsRad || delta >= 2 * MathF.PI - epsRad;
        }

        public static bool operator ==(Angle l, Angle r) => l.Rad == r.Rad;
        public static bool operator !=(Angle l, Angle r) => l.Rad != r.Rad;
        public override bool Equals(object? obj) => obj is Angle && this == (Angle)obj;
        public override int GetHashCode() => Rad.GetHashCode();
        public override string ToString() => Deg.ToString("f0");
    }

    public static class AngleExtensions
    {
        public static Angle Radians(this float Radians) => new(Radians);
        public static Angle Degrees(this float Degrees) => new(Degrees * Angle.DegToRad);
        public static Angle Degrees(this int Degrees) => new(Degrees * Angle.DegToRad);
    }
}
