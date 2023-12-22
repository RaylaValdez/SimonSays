using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Game.ClientState.Objects.Types;

namespace SimonSays.Helpers
{
    /// <summary>
    /// Represents an angle in both radians and degrees and provides angle manipulation methods.
    /// </summary>
    public struct Angle
    {
            // Conversion constants
            public const float RadToDeg = 180 / MathF.PI;
            public const float DegToRad = MathF.PI / 180;

            // Angle in radians
            public float Rad;

            // Angle in degrees
            public float Deg => Rad * RadToDeg;

            // Constructor
            public Angle(float Radians = 0) { Rad = Radians; }

            // Methods for angle conversion and manipulation
            public static Angle FromDirection(Vector2 dir) => FromDirection(dir.X, dir.Y);
            public static Angle FromDirection(float x, float z) => new Angle(MathF.Atan2(x, z));
            public Vector2 ToDirection() => new Vector2(Sin(), Cos());

            // Operators for angle arithmetic
            public static Angle operator +(Angle a, Angle b) => new Angle(a.Rad + b.Rad);
            public static Angle operator -(Angle a, Angle b) => new Angle(a.Rad - b.Rad);
            public static Angle operator -(Angle a) => new Angle(-a.Rad);
            public static Angle operator *(Angle a, float b) => new Angle(a.Rad * b);
            public static Angle operator *(float a, Angle b) => new Angle(a * b.Rad);
            public static Angle operator /(Angle a, float b) => new Angle(a.Rad / b);

            // Methods for angle functions
            public Angle Abs() => new Angle(Math.Abs(Rad));
            public float Sin() => MathF.Sin(Rad);
            public float Cos() => MathF.Cos(Rad);
            public float Tan() => MathF.Tan(Rad);
            public static Angle Asin(float x) => new Angle(MathF.Asin(x));
            public static Angle Acos(float x) => new Angle(MathF.Acos(x));
        


        /// <summary>
        /// Normalizes the angle to the range of -π to π radians.
        /// </summary>
        /// <returns>The normalized angle.</returns>
        public Angle Normalized()
        {
            var r = Rad;

            // Ensure the angle is within the range of -π to π radians
            while (r < -MathF.PI)
                r += 2 * MathF.PI;
            while (r > MathF.PI)
                r -= 2 * MathF.PI;

            // Return the normalized angle
            return new Angle(r);
        }


        /// <summary>
        /// Checks if the angle is almost equal to another angle within a specified epsilon value.
        /// </summary>
        /// <param name="other">The other angle to compare.</param>
        /// <param name="epsRad">The epsilon value in radians.</param>
        /// <returns>True if the angles are almost equal, otherwise false.</returns>
        public bool AlmostEqual(Angle other, float epsRad)
        {
            // Calculate the absolute difference between the angles
            var delta = Math.Abs(Rad - other.Rad);

            // Check if the angles are almost equal within the specified epsilon value
            return delta <= epsRad || delta >= 2 * MathF.PI - epsRad;
        }


        public static bool operator ==(Angle l, Angle r) => l.Rad == r.Rad;
        public static bool operator !=(Angle l, Angle r) => l.Rad != r.Rad;
        public override bool Equals(object? obj) => obj is Angle && this == (Angle)obj;
        public override int GetHashCode() => Rad.GetHashCode();
        public override string ToString() => Deg.ToString("f0");
    }

    /// <summary>
    /// Provides extension methods for Angle conversions between degrees and radians.
    /// </summary>
    public static class AngleExtensions
    {
        /// <summary>
        /// Converts the specified radians value to an Angle.
        /// </summary>
        /// <param name="Radians">The value in radians.</param>
        /// <returns>An Angle instance.</returns>
        public static Angle Radians(this float Radians) => new Angle(Radians);

        /// <summary>
        /// Converts the specified degrees value to an Angle.
        /// </summary>
        /// <param name="Degrees">The value in degrees.</param>
        /// <returns>An Angle instance.</returns>
        public static Angle Degrees(this float Degrees) => new Angle(Degrees * Angle.DegToRad);

        /// <summary>
        /// Converts the specified degrees value to an Angle.
        /// </summary>
        /// <param name="Degrees">The value in degrees.</param>
        /// <returns>An Angle instance.</returns>
        public static Angle Degrees(this int Degrees) => new Angle(Degrees * Angle.DegToRad);
    }


    /// <summary>
    /// Provides static methods for Angle conversions between degrees and radians.
    /// </summary>
    public static class AngleConversions
    {
        /// <summary>
        /// Converts the specified degrees value to radians.
        /// </summary>
        /// <param name="Degrees">The value in degrees.</param>
        /// <returns>The equivalent value in radians.</returns>
        public static float ToRad(float Degrees)
        {
            return Degrees * (float.Pi / 180);
        }

        /// <summary>
        /// Converts the specified radians value to degrees.
        /// </summary>
        /// <param name="Radians">The value in radians.</param>
        /// <returns>The equivalent value in degrees.</returns>
        public static float ToDeg(float Radians)
        {
            return Radians * (180 / float.Pi);
        }
    }

}
