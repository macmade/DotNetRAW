/*******************************************************************************
 * The MIT License (MIT)
 *
 * Copyright (c) 2026, Jean-David Gadina - www.xs-labs.com
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the Software), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 ******************************************************************************/

using System.Runtime.InteropServices;

// Interop struct fields are populated by the marshaller from native memory, not
// assigned in C# code.
#pragma warning disable CS0649

namespace DotNetRAW;

/// <summary>
/// The leading fields of LibRAW's <c>libraw_image_sizes_t</c> (<c>sizes</c>),
/// covering the sensor and output geometry the port reads.
/// </summary>
/// <remarks>
/// Modelled sequentially through <see cref="Flip"/>; the trailing fields
/// (<c>mask</c>, <c>raw_aspect</c>, <c>raw_inset_crops</c>) are unused and left
/// off. All dimensions are non-negative (<c>ushort</c>/<c>unsigned</c>) except
/// <see cref="Flip"/>, which is a signed orientation code.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawImageSizes
{
    /// <summary>The full sensor height, in pixels (<c>raw_height</c>).</summary>
    internal ushort RawHeight;

    /// <summary>The full sensor width, in pixels (<c>raw_width</c>).</summary>
    internal ushort RawWidth;

    /// <summary>The visible (output) image height, in pixels (<c>height</c>).</summary>
    internal ushort Height;

    /// <summary>The visible (output) image width, in pixels (<c>width</c>).</summary>
    internal ushort Width;

    /// <summary>The top margin of the visible area within the sensor (<c>top_margin</c>).</summary>
    internal ushort TopMargin;

    /// <summary>The left margin of the visible area within the sensor (<c>left_margin</c>).</summary>
    internal ushort LeftMargin;

    /// <summary>The internal (processing) image height, in pixels (<c>iheight</c>).</summary>
    internal ushort IHeight;

    /// <summary>The internal (processing) image width, in pixels (<c>iwidth</c>).</summary>
    internal ushort IWidth;

    /// <summary>The row stride of the raw sensor buffer, in bytes (<c>raw_pitch</c>).</summary>
    internal uint RawPitch;

    /// <summary>The pixel aspect ratio (<c>pixel_aspect</c>).</summary>
    internal double PixelAspect;

    /// <summary>The orientation/flip code (<c>flip</c>).</summary>
    internal int Flip;
}
