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

using System.Globalization;
using System.Text;
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="RAWCameraMetadata"/>.
/// </summary>
public class RAWCameraMetadataTests
{
    /// <summary>
    /// The description names the firmware and colour space when the firmware is known.
    /// </summary>
    [ Fact ]
    public void DescriptionNamesFirmwareAndColorSpace()
    {
        RAWCameraMetadata meta = new RAWCameraMetadata( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1, "1.3.3", 0.0f, 0.0f );

        Assert.Equal( "firmware 1.3.3, color space 1", meta.ToString() );
    }

    /// <summary>
    /// A missing firmware string falls back to a placeholder.
    /// </summary>
    [ Fact ]
    public void DescriptionUsesPlaceholderWhenFirmwareEmpty()
    {
        RAWCameraMetadata meta = new RAWCameraMetadata( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 4, "", 0.0f, 0.0f );

        Assert.Equal( "unknown firmware, color space 4", meta.ToString() );
    }

    /// <summary>
    /// The description is culture-invariant: the colour-space code never picks up
    /// culture-specific formatting under a non-invariant current culture.
    /// </summary>
    [ Fact ]
    public void DescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            RAWCameraMetadata meta = new RAWCameraMetadata( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1, "1.3.3", 0.0f, 0.0f );

            Assert.Equal( "firmware 1.3.3, color space 1", meta.ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The from-native constructor copies every surfaced field, widening the colour
    /// space and decoding the firmware string.
    /// </summary>
    [ Fact ]
    public void FromNativeMapsFields()
    {
        LibRawMetadataCommon common = new LibRawMetadataCommon
        {
            FlashEC            = -1.0f,
            FlashGN            = 12.0f,
            CameraTemperature  = 31.5f,
            SensorTemperature  = 33.0f,
            LensTemperature    = 30.0f,
            AmbientTemperature = 21.0f,
            BatteryTemperature = 25.0f,
            ColorSpace         = 1,
            Firmware           = Fixed( "1.3.3", 128 ),
            RealISO            = 102.0f,
            ExifExposureIndex  = 100.0f,
        };

        RAWCameraMetadata meta = new RAWCameraMetadata( common );

        Assert.Equal( -1.0f, meta.FlashExposureCompensation );
        Assert.Equal( 12.0f, meta.FlashGuideNumber );
        Assert.Equal( 31.5f, meta.CameraTemperature );
        Assert.Equal( 33.0f, meta.SensorTemperature );
        Assert.Equal( 30.0f, meta.LensTemperature );
        Assert.Equal( 21.0f, meta.AmbientTemperature );
        Assert.Equal( 25.0f, meta.BatteryTemperature );
        Assert.Equal( 1,     meta.ColorSpace );
        Assert.Equal( "1.3.3", meta.Firmware );
        Assert.Equal( 102.0f, meta.RealISO );
        Assert.Equal( 100.0f, meta.ExposureIndex );
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
