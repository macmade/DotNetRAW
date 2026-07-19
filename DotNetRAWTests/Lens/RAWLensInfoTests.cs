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
/// Unit tests for <see cref="RAWLensInfo"/>, including the composed sub-records.
/// </summary>
public class RAWLensInfoTests
{
    /// <summary>
    /// A named lens is described as its model followed by the focal range.
    /// </summary>
    [ Fact ]
    public void DescriptionCombinesModelAndRange()
    {
        RAWLensInfo lens = Sample( "EF24-70mm f/2.8L", 24.0f, 70.0f );

        Assert.Equal( "EF24-70mm f/2.8L (24–70mm)", lens.ToString() );
    }

    /// <summary>
    /// A lens with no recorded model is described by its focal range alone.
    /// </summary>
    [ Fact ]
    public void DescriptionOmitsEmptyModel()
    {
        RAWLensInfo lens = Sample( "", 50.0f, 50.0f );

        Assert.Equal( "50mm", lens.ToString() );
    }

    /// <summary>
    /// The focal-range description is culture-invariant: fractional focal lengths keep
    /// a period as the decimal separator under a non-invariant current culture.
    /// </summary>
    [ Fact ]
    public void DescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            RAWLensInfo lens = Sample( "", 10.5f, 42.5f );

            Assert.Equal( "10.5–42.5mm", lens.ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The from-native constructor maps the EXIF lens fields, decodes the text fields,
    /// and composes the maker-note and DNG sub-records from their own structures.
    /// </summary>
    [ Fact ]
    public void FromNativeMapsFieldsAndComposesSubRecords()
    {
        LibRawLensInfo lens = new LibRawLensInfo
        {
            MinFocal                = 24.0f,
            MaxFocal                = 70.0f,
            MaxAp4MinFocal          = 2.8f,
            MaxAp4MaxFocal          = 2.8f,
            ExifMaxAp               = 2.8f,
            LensMake                = Fixed( "Canon", 128 ),
            Lens                    = Fixed( "EF24-70mm f/2.8L II USM", 128 ),
            LensSerial              = Fixed( "0000123456", 128 ),
            InternalLensSerial      = Fixed( "ABC", 128 ),
            FocalLengthIn35mmFormat = 50,
        };

        LibRawMakerNotesLens makerNotes = new LibRawMakerNotesLens
        {
            LensID    = 61182u,
            Lens      = Fixed( "EF24-70mm f/2.8L II USM", 128 ),
            FocalType = 2,
            MinFocal  = 24.0f,
            MaxFocal  = 70.0f,
        };

        LibRawDngLens dng = new LibRawDngLens
        {
            MinFocal = 24.0f,
            MaxFocal = 70.0f,
        };

        RAWLensInfo info = new RAWLensInfo( lens, makerNotes, dng );

        Assert.Equal( 24.0f,                               info.MinFocal );
        Assert.Equal( 70.0f,                               info.MaxFocal );
        Assert.Equal( 2.8f,                                info.MaxApertureAtMinFocal );
        Assert.Equal( 2.8f,                                info.MaxApertureAtMaxFocal );
        Assert.Equal( 2.8f,                                info.ExifMaxAperture );
        Assert.Equal( "Canon",                             info.LensMake );
        Assert.Equal( "EF24-70mm f/2.8L II USM",           info.LensModel );
        Assert.Equal( "0000123456",                        info.LensSerial );
        Assert.Equal( "ABC",                               info.InternalLensSerial );
        Assert.Equal( 50,                                  info.FocalLengthIn35mmFormat );

        Assert.Equal( 61182u,                              info.MakerNotes.LensID );
        Assert.Equal( "EF24-70mm f/2.8L II USM",           info.MakerNotes.LensModel );
        Assert.Equal( RAWMakerNoteLensInfo.FocalType.Zoom, info.MakerNotes.LensFocalType );
        Assert.Equal( 24.0f,                               info.Dng.MinFocal );
        Assert.Equal( 70.0f,                               info.Dng.MaxFocal );
    }

    /// <summary>
    /// Builds lens information with a given model and focal range, with neutral
    /// sub-records, for the description tests.
    /// </summary>
    /// <param name="model">The lens model.</param>
    /// <param name="minFocal">The minimum focal length.</param>
    /// <param name="maxFocal">The maximum focal length.</param>
    /// <returns>The constructed lens information.</returns>
    private static RAWLensInfo Sample( string model, float minFocal, float maxFocal )
    {
        RAWMakerNoteLensInfo makerNotes = new RAWMakerNoteLensInfo(
            lensID:                  0,
            lensModel:               "",
            focalType:               RAWMakerNoteLensInfo.FocalType.Unknown,
            minFocal:                0.0f,
            maxFocal:                0.0f,
            maxApertureAtMinFocal:   0.0f,
            maxApertureAtMaxFocal:   0.0f,
            maxAperture:             0.0f,
            minAperture:             0.0f,
            focalLengthIn35mmFormat: 0.0f,
            lensFStops:              0.0f,
            minFocusDistance:        0.0f
        );

        RAWDNGLensInfo dng = new RAWDNGLensInfo( 0.0f, 0.0f, 0.0f, 0.0f );

        return new RAWLensInfo(
            minFocal:                minFocal,
            maxFocal:                maxFocal,
            maxApertureAtMinFocal:   0.0f,
            maxApertureAtMaxFocal:   0.0f,
            exifMaxAperture:         0.0f,
            lensMake:                "",
            lensModel:               model,
            lensSerial:              "",
            internalLensSerial:      "",
            focalLengthIn35mmFormat: 0,
            makerNotes:              makerNotes,
            dng:                     dng
        );
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
