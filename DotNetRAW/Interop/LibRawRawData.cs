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

using System;
using System.Runtime.InteropServices;

// Interop struct fields are populated by the marshaller from native memory, not
// assigned in C# code.
#pragma warning disable CS0649

namespace DotNetRAW;

/// <summary>
/// The leading fields of LibRAW's <c>libraw_rawdata_t</c> (<c>rawdata</c>): the
/// allocation pointer and the six typed views of the unpacked sensor data, exactly
/// one of which is non-null after a successful unpack.
/// </summary>
/// <remarks>
/// Modelled sequentially through <see cref="Float4Image"/>; the trailing Phase One
/// black-level pointers and the nested copies of the parameter, size and colour
/// structures are unused and left off. Which typed pointer is non-null determines
/// the sensor layout; the pixel buffer is read through <see cref="RawImage"/>
/// together with the geometry from <see cref="LibRawImageSizes"/>. A sequential
/// layout lets the marshaller compute the pointer offsets, so the layout guard test
/// validates them rather than echoing them.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawRawData
{
    /// <summary>The backing allocation for the buffers (<c>raw_alloc</c>). Unused.</summary>
    internal IntPtr RawAlloc;

    /// <summary>The 16-bit single-channel (Bayer) buffer, or null (<c>raw_image</c>).</summary>
    internal IntPtr RawImage;

    /// <summary>The 16-bit four-channel buffer, or null (<c>color4_image</c>).</summary>
    internal IntPtr Color4Image;

    /// <summary>The 16-bit three-channel buffer, or null (<c>color3_image</c>).</summary>
    internal IntPtr Color3Image;

    /// <summary>The floating-point single-channel (Bayer) buffer, or null (<c>float_image</c>).</summary>
    internal IntPtr FloatImage;

    /// <summary>The floating-point three-channel buffer, or null (<c>float3_image</c>).</summary>
    internal IntPtr Float3Image;

    /// <summary>The floating-point four-channel buffer, or null (<c>float4_image</c>).</summary>
    internal IntPtr Float4Image;
}
