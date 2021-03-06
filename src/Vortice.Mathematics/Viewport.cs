﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static System.Math;

namespace Vortice.Mathematics
{
    /// <summary>
    /// Represents a floating-point viewport struct.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly struct Viewport : IEquatable<Viewport>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="width">The width of the viewport in pixels.</param>
        /// <param name="height">The height of the viewport in pixels.</param>
        public Viewport(float width, float height)
        {
            X = 0.0f;
            Y = 0.0f;
            Width = width;
            Height = height;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="width">The width of the viewport in pixels.</param>
        /// <param name="height">The height of the viewport in pixels.</param>
        public Viewport(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="width">The width of the viewport in pixels.</param>
        /// <param name="height">The height of the viewport in pixels.</param>
        /// <param name="minDepth">The minimum depth of the clip volume.</param>
        /// <param name="maxDepth">The maximum depth of the clip volume.</param>
        public Viewport(float x, float y, float width, float height, float minDepth, float maxDepth)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">A <see cref="Rectangle"/> that defines the location and size of the viewport in a render target.</param>
        public Viewport(Rectangle bounds)
        {
            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">A <see cref="RectangleF"/> that defines the location and size of the viewport in a render target.</param>
        public Viewport(RectangleF bounds)
        {
            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">A <see cref="Vector4"/> that defines the location and size of the viewport in a render target.</param>
        public Viewport(Vector4 bounds)
        {
            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Z;
            Height = bounds.W;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }

        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the viewport.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the viewport.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Width dimension of the viewport.
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// Height dimension of the viewport.
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// Gets or sets the minimum depth of the clip volume.
        /// </summary>
        public float MinDepth { get; }

        /// <summary>
        /// Gets or sets the maximum depth of the clip volume.
        /// </summary>
        public float MaxDepth { get; }

        /// <summary>
        /// Gets or sets the bounds of the viewport.
        /// </summary>
        /// <value>The bounds.</value>
        public RectangleF Bounds => new RectangleF(X, Y, Width, Height);

        /// <summary>
        /// Gets the aspect ratio used by the viewport.
        /// </summary>
        /// <value>The aspect ratio.</value>
        public float AspectRatio
        {
            get
            {
                if (!MathHelper.IsZero(Height))
                {
                    return Width / Height;
                }

                return 0.0f;
            }
        }

        public Vector3 Project(Vector3 source, Matrix4x4 worldViewProjection)
        {
            float halfViewportWidth = Width * 0.5f;
            float halfViewportHeight = Height * 0.5f;

            Vector3 scale = new Vector3(halfViewportWidth, -halfViewportHeight, MaxDepth - MinDepth);
            Vector3 offset = new Vector3(X + halfViewportWidth, Y + halfViewportHeight, MinDepth);

            var result = Vector3.Transform(source, worldViewProjection);
            result = VectorEx.MultiplyAdd(result, scale, offset);
            return result;
        }

        public Vector3 Project(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            float halfViewportWidth = Width * 0.5f;
            float halfViewportHeight = Height * 0.5f;

            Vector3 scale = new Vector3(halfViewportWidth, -halfViewportHeight, MaxDepth - MinDepth);
            Vector3 offset = new Vector3(X + halfViewportWidth, Y + halfViewportHeight, MinDepth);

            Matrix4x4 transform = Matrix4x4.Multiply(world, view);
            transform = Matrix4x4.Multiply(transform, projection);

            var result = Vector3.Transform(source, transform);
            result = VectorEx.MultiplyAdd(result, scale, offset);
            return result;
        }

        public Vector3 Unproject(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            Vector3 scale = new Vector3(Width * 0.5f, -Height * 0.5f, MaxDepth - MinDepth);
            scale = Vector3.Divide(Vector3.One, scale);

            Vector3 offset = new Vector3(-X, -Y, -MinDepth);
            offset = VectorEx.MultiplyAdd(scale, offset, new Vector3(-1.0f, 1.0f, 0.0f));

            Matrix4x4 transform = Matrix4x4.Multiply(world, view);
            transform = Matrix4x4.Multiply(transform, projection);
            Matrix4x4.Invert(transform, out transform);

            Vector3 result = VectorEx.MultiplyAdd(source, scale, offset);
            return Vector3.Transform(result, transform);
        }

        public static RectangleF ComputeDisplayArea(ViewportScaling scaling, int backBufferWidth, int backBufferHeight, int outputWidth, int outputHeight)
        {
            switch (scaling)
            {
                case ViewportScaling.Stretch:
                    // Output fills the entire window area
                    return new RectangleF(outputWidth, outputHeight);

                case ViewportScaling.AspectRatioStretch:
                    // Output fills the window area but respects the original aspect ratio, using pillar boxing or letter boxing as required
                    // Note: This scaling option is not supported for legacy Win32 windows swap chains
                    {
                        Debug.Assert(backBufferHeight > 0);
                        float aspectRatio = (float)backBufferWidth / backBufferHeight;

                        // Horizontal fill
                        float scaledWidth = outputWidth;
                        float scaledHeight = outputWidth / aspectRatio;
                        if (scaledHeight >= outputHeight)
                        {
                            // Do vertical fill
                            scaledWidth = outputHeight * aspectRatio;
                            scaledHeight = outputHeight;
                        }

                        float offsetX = (outputWidth - scaledWidth) * 0.5f;
                        float offsetY = (outputHeight - scaledHeight) * 0.5f;

                        // Clip to display window
                        return new RectangleF(
                            Max(0, offsetX),
                            Max(0, offsetY),
                            Min(outputWidth, scaledWidth),
                            Min(outputHeight, scaledHeight)
                            );
                    }

                case ViewportScaling.None:
                default:
                    // Output is displayed in the upper left corner of the window area
                    return new RectangleF(Math.Min(backBufferWidth, outputWidth), Math.Min(backBufferHeight, outputHeight));
            }

        }

        public static RectangleF ComputeTitleSafeArea(int backBufferWidth, int backBufferHeight)
        {
            float safew = (backBufferWidth + 19.0f) / 20.0f;
            float safeh = (backBufferHeight + 19.0f) / 20.0f;

            return RectangleF.FromLTRB(
                safew,
                safeh,
                backBufferWidth - safew + 0.5f,
                backBufferHeight - safeh + 0.5f
                );
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Viewport value && Equals(value);

        /// <summary>
        /// Determines whether the specified <see cref="Viewport"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Viewport"/> to compare with this instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Viewport other)
        {
            return MathHelper.NearEqual(X, other.X)
                && MathHelper.NearEqual(Y, other.Y)
                && MathHelper.NearEqual(Width, other.Width)
                && MathHelper.NearEqual(Height, other.Height)
                && MathHelper.NearEqual(MinDepth, other.MinDepth)
                && MathHelper.NearEqual(MaxDepth, other.MaxDepth);
        }

        /// <summary>
        /// Compares two <see cref="Viewport"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Viewport"/> on the left hand of the operand.</param>
        /// <param name="right">The <see cref="Viewport"/> on the right hand of the operand.</param>
        /// <returns>
        /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Viewport left, Viewport right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Viewport"/> objects for inequality.
        /// </summary>
        /// <param name="left">The <see cref="Viewport"/> on the left hand of the operand.</param>
        /// <param name="right">The <see cref="Viewport"/> on the right hand of the operand.</param>
        /// <returns>
        /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Viewport left, Viewport right) => !left.Equals(right);

        /// <inheritdoc/>
		public override int GetHashCode()
        {
            var hashCode = new HashCode();
            {
                hashCode.Add(X);
                hashCode.Add(Y);
                hashCode.Add(Width);
                hashCode.Add(Height);
                hashCode.Add(MinDepth);
                hashCode.Add(MaxDepth);
            }
            return hashCode.ToHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(MinDepth)}: {MinDepth}, {nameof(MaxDepth)}: {MaxDepth}";
        }
    }

    public enum ViewportScaling
    {
        Stretch,
        None,
        AspectRatioStretch
    }
}
