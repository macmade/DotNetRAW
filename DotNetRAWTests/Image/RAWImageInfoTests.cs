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
/// Unit tests for <see cref="RAWImageInfo"/>, plus a fixture-driven check that the
/// camera identity is populated for real RAW samples.
/// </summary>
public class RAWImageInfoTests
{
    /// <summary>
    /// The description combines the camera identity with the colour-filter summary.
    /// </summary>
    [ Fact ]
    public void DescriptionReflectsCameraAndColors()
    {
        RAWImageInfo info = new RAWImageInfo( "Canon", "EOS R5", "1.0.0", "Canon", "Canon EOS R5", 3, 0x94949494u, "RGBG", 1, 0, false );

        Assert.Equal( "Canon EOS R5 — 3 colors (RGBG)", info.ToString() );
    }

    /// <summary>
    /// The description is culture-invariant: the colour count never picks up
    /// culture-specific formatting under a non-invariant current culture.
    /// </summary>
    [ Fact ]
    public void DescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            RAWImageInfo info = new RAWImageInfo( "Canon", "EOS R5", "1.0.0", "Canon", "Canon EOS R5", 3, 0x94949494u, "RGBG", 1, 0, false );

            Assert.Equal( "Canon EOS R5 — 3 colors (RGBG)", info.ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// An empty make and model fall back to "Unknown camera".
    /// </summary>
    [ Fact ]
    public void DescriptionUsesUnknownCameraWhenIdentityIsEmpty()
    {
        RAWImageInfo info = new RAWImageInfo( "", "", "", "", "", 3, 0, "RGBG", 0, 0, false );

        Assert.Equal( "Unknown camera — 3 colors (RGBG)", info.ToString() );
    }

    /// <summary>
    /// A present make with a missing model trims the trailing space.
    /// </summary>
    [ Fact ]
    public void DescriptionTrimsMissingModel()
    {
        RAWImageInfo info = new RAWImageInfo( "Canon", "", "", "", "", 3, 0, "RGBG", 0, 0, false );

        Assert.Equal( "Canon — 3 colors (RGBG)", info.ToString() );
    }

    /// <summary>
    /// The from-native constructor decodes every text field to the first NUL and
    /// maps the numeric and flag fields.
    /// </summary>
    [ Fact ]
    public void FromNativeDecodesStringsAndFlags()
    {
        LibRawIParams native = new LibRawIParams
        {
            Make            = Fixed( "Canon", 64 ),
            Model           = Fixed( "EOS R5", 64 ),
            Software        = Fixed( "1.0.0", 64 ),
            NormalizedMake  = Fixed( "Canon", 64 ),
            NormalizedModel = Fixed( "Canon EOS R5", 64 ),
            Cdesc           = Fixed( "RGBG", 5 ),
            Colors          = 3,
            Filters         = 0x94949494u,
            RawCount        = 2,
            DngVersion      = 0,
            IsFoveon        = 0,
        };

        RAWImageInfo info = new RAWImageInfo( native );

        Assert.Equal( "Canon",        info.Make );
        Assert.Equal( "EOS R5",       info.Model );
        Assert.Equal( "1.0.0",        info.Software );
        Assert.Equal( "Canon",        info.NormalizedMake );
        Assert.Equal( "Canon EOS R5", info.NormalizedModel );
        Assert.Equal( 3,              info.Colors );
        Assert.Equal( 0x94949494u,    info.Filters );
        Assert.Equal( "RGBG",         info.ColorDescription );
        Assert.Equal( 2,              info.RawCount );
        Assert.Equal( 0,              info.DngVersion );
        Assert.False( info.IsFoveon );
    }

    /// <summary>
    /// A non-zero <c>is_foveon</c> field maps to <see langword="true"/>.
    /// </summary>
    [ Fact ]
    public void FromNativeReadsFoveonFlag()
    {
        LibRawIParams native = new LibRawIParams
        {
            Make            = Fixed( "Sigma", 64 ),
            Model           = Fixed( "sd Quattro", 64 ),
            Software        = Fixed( "", 64 ),
            NormalizedMake  = Fixed( "Sigma", 64 ),
            NormalizedModel = Fixed( "Sigma sd Quattro", 64 ),
            Cdesc           = Fixed( "RGBG", 5 ),
            Colors          = 3,
            Filters         = 0,
            RawCount        = 1,
            DngVersion      = 0,
            IsFoveon        = 1,
        };

        Assert.True( new RAWImageInfo( native ).IsFoveon );
    }

    /// <summary>
    /// For real samples the camera make, model and colour count are populated.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void ImageInfoIsPopulated( string path )
    {
        using RAWFile file = new RAWFile( path );
        RAWImageInfo  info = file.ImageInfo;

        Assert.False( string.IsNullOrEmpty( info.Make ) );
        Assert.False( string.IsNullOrEmpty( info.Model ) );
        Assert.True( info.Colors > 0 );
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
