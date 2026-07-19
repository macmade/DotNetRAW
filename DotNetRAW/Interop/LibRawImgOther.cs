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
/// The leading fields of LibRAW's <c>libraw_imgother_t</c> (<c>other</c>),
/// covering the exposure, timestamp, GPS and descriptive metadata the port reads.
/// </summary>
/// <remarks>
/// Modelled sequentially through <see cref="Artist"/>; the trailing
/// <c>analogbalance</c> is unused and left off. The unused <c>gpsdata</c> block is
/// modelled to preserve the layout, while <see cref="ParsedGps"/> is kept because
/// the port surfaces it. <see cref="Timestamp"/> is a signed 64-bit <c>time_t</c>
/// in seconds since the Unix epoch, zero meaning "no timestamp". A sequential
/// layout is used - rather than an explicit one - because its <c>byte[]</c> fields
/// sit at 4-byte offsets, which the runtime forbids for object references under
/// explicit layout.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawImgOther
{
    /// <summary>The ISO sensitivity (<c>iso_speed</c>).</summary>
    internal float IsoSpeed;

    /// <summary>The shutter speed, in seconds (<c>shutter</c>).</summary>
    internal float Shutter;

    /// <summary>The aperture, as an f-number (<c>aperture</c>).</summary>
    internal float Aperture;

    /// <summary>The focal length, in millimetres (<c>focal_len</c>).</summary>
    internal float FocalLength;

    /// <summary>The capture time as a 64-bit <c>time_t</c>; zero means none (<c>timestamp</c>).</summary>
    internal long Timestamp;

    /// <summary>The shot order within a burst (<c>shot_order</c>).</summary>
    internal uint ShotOrder;

    /// <summary>The raw EXIF GPS tag values (<c>gpsdata</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 ) ]
    internal uint[] GpsData;

    /// <summary>The parsed GPS coordinates (<c>parsed_gps</c>).</summary>
    internal LibRawGpsInfo ParsedGps;

    /// <summary>The image description, NUL-terminated (<c>desc</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 512 ) ]
    internal byte[] Description;

    /// <summary>The artist/author, NUL-terminated (<c>artist</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] Artist;
}
