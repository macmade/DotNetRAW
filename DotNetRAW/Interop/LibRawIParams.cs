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
/// The leading fields of LibRAW's <c>libraw_iparams_t</c> (<c>idata</c>),
/// covering the camera identity and colour-filter parameters the port reads.
/// </summary>
/// <remarks>
/// Modelled sequentially through <see cref="Cdesc"/>; the trailing <c>xmplen</c>
/// and <c>xmpdata</c> pointer are unused and left off. The fixed <c>char</c>
/// buffers are decoded as UTF-8 up to the first NUL. <see cref="Filters"/> is
/// unsigned because the CFA decode relies on an unsigned shift, whereas
/// <see cref="Colors"/> is signed. The <c>xtrans</c> grid is signed.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawIParams
{
    /// <summary>A four-byte guard band preceding the identity fields (<c>guard</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 ) ]
    internal byte[] Guard;

    /// <summary>The camera manufacturer, NUL-terminated (<c>make</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] Make;

    /// <summary>The camera model, NUL-terminated (<c>model</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] Model;

    /// <summary>The producing software, NUL-terminated (<c>software</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] Software;

    /// <summary>The normalized manufacturer name, NUL-terminated (<c>normalized_make</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] NormalizedMake;

    /// <summary>The normalized model name, NUL-terminated (<c>normalized_model</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] NormalizedModel;

    /// <summary>The internal camera-maker index (<c>maker_index</c>). Unused.</summary>
    internal uint MakerIndex;

    /// <summary>The number of raw images in the file (<c>raw_count</c>).</summary>
    internal uint RawCount;

    /// <summary>The DNG version, or zero for non-DNG files (<c>dng_version</c>).</summary>
    internal uint DngVersion;

    /// <summary>Non-zero for Foveon sensors (<c>is_foveon</c>).</summary>
    internal uint IsFoveon;

    /// <summary>The number of colour channels (<c>colors</c>).</summary>
    internal int Colors;

    /// <summary>The colour-filter-array pattern bit-field (<c>filters</c>).</summary>
    internal uint Filters;

    /// <summary>The 6&#215;6 X-Trans colour pattern, row-major (<c>xtrans</c>); signed.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 36 ) ]
    internal sbyte[] XTrans;

    /// <summary>The 6&#215;6 absolute X-Trans pattern (<c>xtrans_abs</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 36 ) ]
    internal sbyte[] XTransAbs;

    /// <summary>The colour-channel description, NUL-terminated (<c>cdesc</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 5 ) ]
    internal byte[] Cdesc;
}
