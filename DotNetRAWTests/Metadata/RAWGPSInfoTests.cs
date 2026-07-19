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
using System.Globalization;
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="RAWGPSInfo"/> coordinate conversion, the failable
/// factory, and the description, plus a fixture-driven check that reading GPS from a
/// real RAW sample is always safe.
/// </summary>
public class RAWGPSInfoTests
{
    /// <summary>
    /// A northern-hemisphere degrees/minutes/seconds triple converts to positive
    /// decimal degrees.
    /// </summary>
    [ Fact ]
    public void DecimalDegreesNorth()
    {
        double value = RAWGPSInfo.DecimalDegrees( [ 48.0f, 51.0f, 30.0f ], false );

        Assert.True( Math.Abs( value - 48.858333 ) < 0.0001 );
    }

    /// <summary>
    /// A southern/western coordinate is negated.
    /// </summary>
    [ Fact ]
    public void DecimalDegreesNegated()
    {
        double value = RAWGPSInfo.DecimalDegrees( [ 33.0f, 51.0f, 54.0f ], true );

        Assert.True( value < 0 );
        Assert.True( Math.Abs( value - ( -33.865 ) ) < 0.001 );
    }

    /// <summary>
    /// Missing degrees/minutes/seconds components are treated as zero rather than
    /// crashing.
    /// </summary>
    [ Fact ]
    public void DecimalDegreesHandlesShortInput()
    {
        Assert.Equal( 10.0, RAWGPSInfo.DecimalDegrees( [ 10.0f ], false ) );
        Assert.Equal( 0.0,  RAWGPSInfo.DecimalDegrees( [], false ) );
    }

    /// <summary>
    /// A file with no parsed GPS data yields no <see cref="RAWGPSInfo"/>.
    /// </summary>
    [ Fact ]
    public void FromNativeReturnsNullWhenNotParsed()
    {
        LibRawGpsInfo gps = new LibRawGpsInfo
        {
            GpsParsed = 0,
        };

        Assert.Null( RAWGPSInfo.FromNative( gps ) );
    }

    /// <summary>
    /// Parsed GPS data resolves to signed decimal degrees, an altitude, and the raw
    /// reference markers.
    /// </summary>
    [ Fact ]
    public void FromNativeParsesCoordinates()
    {
        LibRawGpsInfo gps = new LibRawGpsInfo
        {
            Latitude  = [ 48.0f, 51.0f, 30.0f ],
            Longitude = [ 2.0f, 20.0f, 56.0f ],
            Altitude  = 35.0f,
            AltRef    = 0,
            LatRef    = ( sbyte )'N',
            LongRef   = ( sbyte )'E',
            GpsParsed = 1,
        };

        RAWGPSInfo? info = RAWGPSInfo.FromNative( gps );

        Assert.NotNull( info );
        Assert.True( Math.Abs( info.Latitude - 48.858333 ) < 0.0001 );
        Assert.True( Math.Abs( info.Longitude - 2.348889 ) < 0.0001 );
        Assert.Equal( 35.0, info.Altitude );
        Assert.Equal( 'N', info.LatitudeRef );
        Assert.Equal( 'E', info.LongitudeRef );
        Assert.Equal( 0, info.AltitudeRef );
    }

    /// <summary>
    /// Southern/western hemispheres negate the coordinates, and a below-sea-level
    /// reference negates the altitude.
    /// </summary>
    [ Fact ]
    public void FromNativeNegatesSouthWestAndBelowSeaLevel()
    {
        LibRawGpsInfo gps = new LibRawGpsInfo
        {
            Latitude  = [ 33.0f, 51.0f, 54.0f ],
            Longitude = [ 151.0f, 12.0f, 36.0f ],
            Altitude  = 10.0f,
            AltRef    = 1,
            LatRef    = ( sbyte )'S',
            LongRef   = ( sbyte )'W',
            GpsParsed = 1,
        };

        RAWGPSInfo? info = RAWGPSInfo.FromNative( gps );

        Assert.NotNull( info );
        Assert.True( info.Latitude < 0 );
        Assert.True( info.Longitude < 0 );
        Assert.Equal( -10.0, info.Altitude );
        Assert.Equal( 'S', info.LatitudeRef );
        Assert.Equal( 'W', info.LongitudeRef );
        Assert.Equal( 1, info.AltitudeRef );
    }

    /// <summary>
    /// A non-printing reference byte maps to a null reference character.
    /// </summary>
    [ Fact ]
    public void FromNativeLeavesReferenceNullWhenAbsent()
    {
        LibRawGpsInfo gps = new LibRawGpsInfo
        {
            Latitude  = [ 10.0f, 0.0f, 0.0f ],
            Longitude = [ 20.0f, 0.0f, 0.0f ],
            Altitude  = 0.0f,
            AltRef    = 0,
            LatRef    = 0,
            LongRef   = 0,
            GpsParsed = 1,
        };

        RAWGPSInfo? info = RAWGPSInfo.FromNative( gps );

        Assert.NotNull( info );
        Assert.Null( info.LatitudeRef );
        Assert.Null( info.LongitudeRef );
    }

    /// <summary>
    /// The description lists latitude, longitude and altitude.
    /// </summary>
    [ Fact ]
    public void DescriptionListsCoordinates()
    {
        RAWGPSInfo info = new RAWGPSInfo( 48.8583, 2.2945, 35.0, 'N', 'E', 0 );

        Assert.Equal( "48.8583, 2.2945, 35m", info.ToString() );
    }

    /// <summary>
    /// The description is culture-invariant: the coordinate decimals keep a period as
    /// the separator under a non-invariant current culture.
    /// </summary>
    [ Fact ]
    public void DescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            RAWGPSInfo info = new RAWGPSInfo( 48.8583, 2.2945, 35.0, 'N', 'E', 0 );

            Assert.Equal( "48.8583, 2.2945, 35m", info.ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// Reading GPS from a real file never crashes and is absent when the file records no
    /// GPS; when present, the coordinates fall within their valid ranges.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void GpsInfoIsSafeToRead( string path )
    {
        using RAWFile file = new RAWFile( path );

        if( file.GpsInfo is RAWGPSInfo gps )
        {
            Assert.InRange( gps.Latitude, -90.0, 90.0 );
            Assert.InRange( gps.Longitude, -180.0, 180.0 );
        }
    }
}
