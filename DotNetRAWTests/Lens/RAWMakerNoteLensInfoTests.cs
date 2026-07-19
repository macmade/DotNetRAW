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
/// Unit tests for <see cref="RAWMakerNoteLensInfo"/>, including the focal-type mapping.
/// </summary>
public class RAWMakerNoteLensInfoTests
{
    /// <summary>
    /// LibRAW's focal-type codes map to the expected cases; unexpected values fall
    /// back to <see cref="RAWMakerNoteLensInfo.FocalType.Unknown"/>.
    /// </summary>
    [ Fact ]
    public void FocalTypeMapping()
    {
        Assert.Equal( RAWMakerNoteLensInfo.FocalType.Fixed,   RAWMakerNoteLensInfo.FocalTypeFromCode(  1 ) );
        Assert.Equal( RAWMakerNoteLensInfo.FocalType.Zoom,    RAWMakerNoteLensInfo.FocalTypeFromCode(  2 ) );
        Assert.Equal( RAWMakerNoteLensInfo.FocalType.Unknown, RAWMakerNoteLensInfo.FocalTypeFromCode(  0 ) );
        Assert.Equal( RAWMakerNoteLensInfo.FocalType.Unknown, RAWMakerNoteLensInfo.FocalTypeFromCode( -1 ) );
        Assert.Equal( RAWMakerNoteLensInfo.FocalType.Unknown, RAWMakerNoteLensInfo.FocalTypeFromCode( 99 ) );
    }

    /// <summary>
    /// A named lens is described as its model followed by the focal range.
    /// </summary>
    [ Fact ]
    public void DescriptionCombinesModelAndRange()
    {
        RAWMakerNoteLensInfo lens = Sample( "EF24-70mm f/2.8L", 24.0f, 70.0f );

        Assert.Equal( "EF24-70mm f/2.8L (24–70mm)", lens.ToString() );
    }

    /// <summary>
    /// A lens with no recorded model is described by its focal range alone.
    /// </summary>
    [ Fact ]
    public void DescriptionOmitsEmptyModel()
    {
        RAWMakerNoteLensInfo lens = Sample( "", 50.0f, 50.0f );

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

            RAWMakerNoteLensInfo lens = Sample( "", 10.5f, 42.5f );

            Assert.Equal( "10.5–42.5mm", lens.ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The from-native constructor decodes the lens model, maps the focal-type code,
    /// and copies every numeric field.
    /// </summary>
    [ Fact ]
    public void FromNativeMapsFields()
    {
        LibRawMakerNotesLens native = new LibRawMakerNotesLens
        {
            LensID                  = 61182u,
            Lens                    = Fixed( "EF24-70mm f/2.8L II USM", 128 ),
            FocalType               = 2,
            MinFocal                = 24.0f,
            MaxFocal                = 70.0f,
            MaxAp4MinFocal          = 2.8f,
            MaxAp4MaxFocal          = 2.8f,
            MaxAp                   = 2.8f,
            MinAp                   = 22.0f,
            FocalLengthIn35mmFormat = 24.0f,
            LensFStops              = 8.0f,
            MinFocusDistance        = 0.38f,
        };

        RAWMakerNoteLensInfo lens = new RAWMakerNoteLensInfo( native );

        Assert.Equal( 61182u,                              lens.LensID );
        Assert.Equal( "EF24-70mm f/2.8L II USM",           lens.LensModel );
        Assert.Equal( RAWMakerNoteLensInfo.FocalType.Zoom, lens.LensFocalType );
        Assert.Equal( 24.0f,                               lens.MinFocal );
        Assert.Equal( 70.0f,                               lens.MaxFocal );
        Assert.Equal( 2.8f,                                lens.MaxApertureAtMinFocal );
        Assert.Equal( 2.8f,                                lens.MaxApertureAtMaxFocal );
        Assert.Equal( 2.8f,                                lens.MaxAperture );
        Assert.Equal( 22.0f,                               lens.MinAperture );
        Assert.Equal( 24.0f,                               lens.FocalLengthIn35mmFormat );
        Assert.Equal( 8.0f,                                lens.LensFStops );
        Assert.Equal( 0.38f,                               lens.MinFocusDistance );
    }

    /// <summary>
    /// Builds a maker-note lens with a given model and focal range, leaving the other
    /// fields at neutral values, for the description tests.
    /// </summary>
    /// <param name="model">The lens model.</param>
    /// <param name="minFocal">The minimum focal length.</param>
    /// <param name="maxFocal">The maximum focal length.</param>
    /// <returns>The constructed maker-note lens information.</returns>
    private static RAWMakerNoteLensInfo Sample( string model, float minFocal, float maxFocal )
    {
        return new RAWMakerNoteLensInfo(
            lensID:                  0,
            lensModel:               model,
            focalType:               RAWMakerNoteLensInfo.FocalType.Unknown,
            minFocal:                minFocal,
            maxFocal:                maxFocal,
            maxApertureAtMinFocal:   0.0f,
            maxApertureAtMaxFocal:   0.0f,
            maxAperture:             0.0f,
            minAperture:             0.0f,
            focalLengthIn35mmFormat: 0.0f,
            lensFStops:              0.0f,
            minFocusDistance:        0.0f
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
