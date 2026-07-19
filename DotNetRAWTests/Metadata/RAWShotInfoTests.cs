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
using System.Text;
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="RAWShotInfo"/>, including the reciprocal-shutter
/// formatting and the timestamp handling.
/// </summary>
public class RAWShotInfoTests
{
    /// <summary>
    /// A sub-second shutter speed is rendered as a rounded reciprocal fraction, and
    /// the full exposure summary lists ISO, shutter, aperture and focal length.
    /// </summary>
    [ Fact ]
    public void DescriptionRendersReciprocalShutter()
    {
        RAWShotInfo shot = new RAWShotInfo( 100.0f, 0.004f, 2.8f, 50.0f, null, 3, "", "", "" );

        Assert.Equal( "ISO 100, 1/250s, f/2.8, 50mm", shot.ToString() );
    }

    /// <summary>
    /// A shutter speed of a second or longer is rendered in whole seconds rather than
    /// as a reciprocal.
    /// </summary>
    [ Fact ]
    public void DescriptionRendersSlowShutterInSeconds()
    {
        RAWShotInfo shot = new RAWShotInfo( 200.0f, 2.0f, 0.0f, 0.0f, null, 0, "", "", "" );

        Assert.Equal( "ISO 200, 2s", shot.ToString() );
    }

    /// <summary>
    /// Unrecorded (zero) exposure fields are omitted from the summary.
    /// </summary>
    [ Fact ]
    public void DescriptionOmitsMissingFields()
    {
        RAWShotInfo shot = new RAWShotInfo( 400.0f, 0.0f, 8.0f, 0.0f, null, 0, "", "", "" );

        Assert.Equal( "ISO 400, f/8", shot.ToString() );
    }

    /// <summary>
    /// A shot with no recorded exposure values reports a placeholder rather than an
    /// empty string.
    /// </summary>
    [ Fact ]
    public void DescriptionReportsNoExposureData()
    {
        RAWShotInfo shot = new RAWShotInfo( 0.0f, 0.0f, 0.0f, 0.0f, null, 0, "", "", "" );

        Assert.Equal( "no exposure data", shot.ToString() );
    }

    /// <summary>
    /// A non-finite shutter speed is tolerated without crashing.
    /// </summary>
    [ Fact ]
    public void DescriptionToleratesNonFiniteShutter()
    {
        RAWShotInfo nan      = new RAWShotInfo( 100.0f, float.NaN, 2.8f, 0.0f, null, 0, "", "", "" );
        RAWShotInfo infinity = new RAWShotInfo( 100.0f, float.PositiveInfinity, 2.8f, 0.0f, null, 0, "", "", "" );

        Assert.Equal( "ISO 100, f/2.8", nan.ToString() );
        Assert.NotNull( infinity.ToString() );
    }

    /// <summary>
    /// The exposure summary is culture-invariant: fractional aperture and focal values
    /// keep a period as the decimal separator under a non-invariant current culture.
    /// </summary>
    [ Fact ]
    public void DescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            RAWShotInfo shot = new RAWShotInfo( 100.0f, 0.0f, 2.8f, 10.5f, null, 0, "", "", "" );

            Assert.Equal( "ISO 100, f/2.8, 10.5mm", shot.ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The from-native constructor copies the exposure fields, converts the timestamp,
    /// and decodes the description, artist and body serial.
    /// </summary>
    [ Fact ]
    public void FromNativeMapsFields()
    {
        LibRawImgOther other = new LibRawImgOther
        {
            IsoSpeed    = 100.0f,
            Shutter     = 0.004f,
            Aperture    = 2.8f,
            FocalLength = 50.0f,
            Timestamp   = 1700000000,
            ShotOrder   = 3,
            Description = Fixed( "A landscape", 512 ),
            Artist      = Fixed( "Jane Doe", 64 ),
        };

        LibRawShootingInfo shooting = new LibRawShootingInfo
        {
            BodySerial = Fixed( "SN123456", 64 ),
        };

        RAWShotInfo shot = new RAWShotInfo( other, shooting );

        Assert.Equal( 100.0f,       shot.IsoSpeed );
        Assert.Equal( 0.004f,       shot.ShutterSpeed );
        Assert.Equal( 2.8f,         shot.Aperture );
        Assert.Equal( 50.0f,        shot.FocalLength );
        Assert.Equal( DateTimeOffset.FromUnixTimeSeconds( 1700000000 ), shot.Timestamp );
        Assert.Equal( 3L,           shot.ShotOrder );
        Assert.Equal( "A landscape", shot.ImageDescription );
        Assert.Equal( "Jane Doe",   shot.Artist );
        Assert.Equal( "SN123456",   shot.BodySerial );
    }

    /// <summary>
    /// A zero native timestamp maps to a null capture time.
    /// </summary>
    [ Fact ]
    public void FromNativeTreatsZeroTimestampAsNull()
    {
        LibRawImgOther other = new LibRawImgOther
        {
            Timestamp   = 0,
            Description = Fixed( "", 512 ),
            Artist      = Fixed( "", 64 ),
        };

        LibRawShootingInfo shooting = new LibRawShootingInfo
        {
            BodySerial = Fixed( "", 64 ),
        };

        RAWShotInfo shot = new RAWShotInfo( other, shooting );

        Assert.Null( shot.Timestamp );
    }

    /// <summary>
    /// A native timestamp outside the representable date range is treated as no
    /// capture time rather than throwing.
    /// </summary>
    [ Fact ]
    public void FromNativeTreatsOutOfRangeTimestampAsNull()
    {
        LibRawImgOther other = new LibRawImgOther
        {
            Timestamp   = long.MaxValue,
            Description = Fixed( "", 512 ),
            Artist      = Fixed( "", 64 ),
        };

        LibRawShootingInfo shooting = new LibRawShootingInfo
        {
            BodySerial = Fixed( "", 64 ),
        };

        RAWShotInfo shot = new RAWShotInfo( other, shooting );

        Assert.Null( shot.Timestamp );
    }

    /// <summary>
    /// Builds a fixed-size <c>byte</c> buffer holding the UTF-8 bytes of a value,
    /// NUL-padded to <paramref name="size"/>, as the interop marshaller would.
    /// </summary>
    /// <param name="value">The text to encode.</param>
    /// <param name="size">The fixed buffer size.</param>
    /// <returns>The NUL-padded buffer.</returns>
    private static byte[] Fixed( string value, int size )
    {
        byte[] buffer = new byte[ size ];

        Encoding.UTF8.GetBytes( value ).CopyTo( buffer, 0 );

        return buffer;
    }
}
