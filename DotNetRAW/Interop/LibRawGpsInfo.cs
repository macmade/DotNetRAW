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
/// LibRAW's <c>libraw_gps_info_t</c> (<c>other.parsed_gps</c>): the parsed GPS
/// coordinates and their reference markers.
/// </summary>
/// <remarks>
/// Fully modelled. The coordinate arrays are degrees/minutes/seconds triples; the
/// reference fields are signed <c>char</c>. <see cref="GpsParsed"/> is non-zero
/// only when LibRAW actually parsed GPS data.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawGpsInfo
{
    /// <summary>Latitude as degrees, minutes, seconds (<c>latitude</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 ) ]
    internal float[] Latitude;

    /// <summary>Longitude as degrees, minutes, seconds (<c>longitude</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 ) ]
    internal float[] Longitude;

    /// <summary>The GPS timestamp as hours, minutes, seconds (<c>gpstimestamp</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 ) ]
    internal float[] GpsTimestamp;

    /// <summary>The altitude, in metres (<c>altitude</c>).</summary>
    internal float Altitude;

    /// <summary>The altitude reference; non-zero means below sea level (<c>altref</c>).</summary>
    internal sbyte AltRef;

    /// <summary>The latitude reference character, <c>'N'</c> or <c>'S'</c> (<c>latref</c>).</summary>
    internal sbyte LatRef;

    /// <summary>The longitude reference character, <c>'E'</c> or <c>'W'</c> (<c>longref</c>).</summary>
    internal sbyte LongRef;

    /// <summary>The GPS status character (<c>gpsstatus</c>). Unused.</summary>
    internal sbyte GpsStatus;

    /// <summary>Non-zero when GPS data was parsed (<c>gpsparsed</c>).</summary>
    internal sbyte GpsParsed;
}
