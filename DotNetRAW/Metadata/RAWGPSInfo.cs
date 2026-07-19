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

using System.Collections.Generic;
using System.Globalization;

namespace DotNetRAW;

/// <summary>
/// The GPS location recorded with a RAW file.
/// </summary>
/// <remarks>
/// Mirrors LibRAW's <c>libraw_gps_info_t</c>. LibRAW stores coordinates as
/// degrees/minutes/seconds with separate hemisphere references; this type resolves
/// them into signed decimal degrees (negative for south/west) and a signed altitude
/// (negative below sea level), while still exposing the raw reference markers.
/// </remarks>
public sealed record RAWGPSInfo
{
    /// <summary>The latitude in signed decimal degrees (negative for the southern hemisphere).</summary>
    public double Latitude { get; }

    /// <summary>The longitude in signed decimal degrees (negative for the western hemisphere).</summary>
    public double Longitude { get; }

    /// <summary>The altitude in metres (negative below sea level).</summary>
    public double Altitude { get; }

    /// <summary>The latitude hemisphere reference (<c>'N'</c> or <c>'S'</c>), if present.</summary>
    public char? LatitudeRef { get; }

    /// <summary>The longitude hemisphere reference (<c>'E'</c> or <c>'W'</c>), if present.</summary>
    public char? LongitudeRef { get; }

    /// <summary>The altitude reference: <c>0</c> above sea level, <c>1</c> below.</summary>
    public int AltitudeRef { get; }

    /// <summary>
    /// Creates GPS information from explicit values.
    /// </summary>
    /// <param name="latitude">The latitude in signed decimal degrees.</param>
    /// <param name="longitude">The longitude in signed decimal degrees.</param>
    /// <param name="altitude">The altitude in metres.</param>
    /// <param name="latitudeRef">The latitude hemisphere reference.</param>
    /// <param name="longitudeRef">The longitude hemisphere reference.</param>
    /// <param name="altitudeRef">The altitude reference (<c>0</c> or <c>1</c>).</param>
    public RAWGPSInfo( double latitude, double longitude, double altitude, char? latitudeRef, char? longitudeRef, int altitudeRef )
    {
        this.Latitude     = latitude;
        this.Longitude    = longitude;
        this.Altitude     = altitude;
        this.LatitudeRef  = latitudeRef;
        this.LongitudeRef = longitudeRef;
        this.AltitudeRef  = altitudeRef;
    }

    /// <summary>
    /// Creates GPS information from a marshaled LibRAW GPS structure, or
    /// <see langword="null"/> when the file carries no parsed GPS data.
    /// </summary>
    /// <param name="gps">The marshaled <c>libraw_gps_info_t</c> fields.</param>
    /// <returns>
    /// The resolved GPS information, or <see langword="null"/> if
    /// <c>gpsparsed</c> is zero.
    /// </returns>
    internal static RAWGPSInfo? FromNative( LibRawGpsInfo gps )
    {
        if( gps.GpsParsed == 0 )
        {
            return null;
        }

        char? latitudeRef  = CharacterFromByte( gps.LatRef );
        char? longitudeRef = CharacterFromByte( gps.LongRef );

        return new RAWGPSInfo(
            DecimalDegrees( gps.Latitude, latitudeRef == 'S' ),
            DecimalDegrees( gps.Longitude, longitudeRef == 'W' ),
            gps.AltRef == 1 ? -( double )gps.Altitude : gps.Altitude,
            latitudeRef,
            longitudeRef,
            gps.AltRef
        );
    }

    /// <summary>
    /// Converts a degrees/minutes/seconds triple to signed decimal degrees.
    /// </summary>
    /// <param name="dms">
    /// A <c>[degrees, minutes, seconds]</c> triple. Missing components are treated as
    /// zero.
    /// </param>
    /// <param name="isNegative">
    /// Whether the coordinate is in the southern/western hemisphere and should be
    /// negated.
    /// </param>
    /// <returns>The coordinate in signed decimal degrees.</returns>
    internal static double DecimalDegrees( IReadOnlyList< float > dms, bool isNegative )
    {
        double degrees = dms.Count > 0 ? ( double )dms[ 0 ] : 0.0;
        double minutes = dms.Count > 1 ? ( double )dms[ 1 ] : 0.0;
        double seconds = dms.Count > 2 ? ( double )dms[ 2 ] : 0.0;
        double value   = degrees + ( minutes / 60.0 ) + ( seconds / 3600.0 );

        return isNegative ? -value : value;
    }

    /// <summary>
    /// Returns a compact summary of the coordinates.
    /// </summary>
    /// <returns>A summary of the form <c>"latitude, longitude, altitudem"</c>.</returns>
    public override string ToString()
    {
        return string.Create( CultureInfo.InvariantCulture, $"{ this.Latitude }, { this.Longitude }, { this.Altitude.CompactDescription() }m" );
    }

    /// <summary>
    /// Converts a LibRAW reference byte to a printable character.
    /// </summary>
    /// <param name="value">A signed <c>char</c> reference marker (e.g. <c>latref</c>).</param>
    /// <returns>The character, or <see langword="null"/> if the byte is empty or non-printing.</returns>
    private static char? CharacterFromByte( sbyte value )
    {
        if( value <= 0 )
        {
            return null;
        }

        return ( char )value;
    }
}
