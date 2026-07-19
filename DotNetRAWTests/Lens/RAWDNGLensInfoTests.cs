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
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="RAWDNGLensInfo"/>.
/// </summary>
public class RAWDNGLensInfoTests
{
    /// <summary>
    /// A lens with distinct minimum and maximum focal lengths is described as a range.
    /// </summary>
    [ Fact ]
    public void DescriptionShowsZoomRange()
    {
        RAWDNGLensInfo lens = new RAWDNGLensInfo( 24.0f, 70.0f, 2.8f, 2.8f );

        Assert.Equal( "24–70mm", lens.ToString() );
    }

    /// <summary>
    /// A lens whose minimum and maximum focal lengths match is described as a single
    /// focal length.
    /// </summary>
    [ Fact ]
    public void DescriptionShowsPrimeFocal()
    {
        RAWDNGLensInfo lens = new RAWDNGLensInfo( 50.0f, 50.0f, 1.8f, 1.8f );

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

            RAWDNGLensInfo lens = new RAWDNGLensInfo( 10.5f, 42.5f, 3.5f, 5.6f );

            Assert.Equal( "10.5–42.5mm", lens.ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The from-native constructor maps every field of the marshaled DNG-lens structure.
    /// </summary>
    [ Fact ]
    public void FromNativeMapsFields()
    {
        LibRawDngLens native = new LibRawDngLens
        {
            MinFocal       = 24.0f,
            MaxFocal       = 70.0f,
            MaxAp4MinFocal = 2.8f,
            MaxAp4MaxFocal = 4.0f,
        };

        RAWDNGLensInfo lens = new RAWDNGLensInfo( native );

        Assert.Equal( 24.0f, lens.MinFocal );
        Assert.Equal( 70.0f, lens.MaxFocal );
        Assert.Equal( 2.8f,  lens.MaxApertureAtMinFocal );
        Assert.Equal( 4.0f,  lens.MaxApertureAtMaxFocal );
    }
}
