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
/// Unit tests for <see cref="RAWImageSizes"/>.
/// </summary>
public class RAWImageSizesTests
{
    /// <summary>
    /// The description summarizes the raw and output dimensions.
    /// </summary>
    [ Fact ]
    public void DescriptionReflectsDimensions()
    {
        RAWImageSizes sizes = new RAWImageSizes( 6000, 4000, 5980, 3980, 16, 24, 5980, 3980, 12000, 1.0, 5 );

        Assert.Equal( "raw 6000×4000, output 5980×3980", sizes.ToString() );
    }

    /// <summary>
    /// The description is culture-invariant: the dimensions never pick up
    /// culture-specific formatting under a non-invariant current culture.
    /// </summary>
    [ Fact ]
    public void DescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            RAWImageSizes sizes = new RAWImageSizes( 6000, 4000, 5980, 3980, 16, 24, 5980, 3980, 12000, 1.0, 5 );

            Assert.Equal( "raw 6000×4000, output 5980×3980", sizes.ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The from-native constructor maps every field of the marshaled structure,
    /// including the unsigned pitch and the signed flip.
    /// </summary>
    [ Fact ]
    public void FromNativeMapsAllFields()
    {
        LibRawImageSizes native = new LibRawImageSizes
        {
            RawWidth    = 6000,
            RawHeight   = 4000,
            Width       = 5980,
            Height      = 3980,
            TopMargin   = 16,
            LeftMargin  = 24,
            IWidth      = 5970,
            IHeight     = 3970,
            RawPitch    = 12000,
            PixelAspect = 1.5,
            Flip        = 5,
        };

        RAWImageSizes sizes = new RAWImageSizes( native );

        Assert.Equal( 6000,  sizes.RawWidth );
        Assert.Equal( 4000,  sizes.RawHeight );
        Assert.Equal( 5980,  sizes.Width );
        Assert.Equal( 3980,  sizes.Height );
        Assert.Equal( 16,    sizes.TopMargin );
        Assert.Equal( 24,    sizes.LeftMargin );
        Assert.Equal( 5970,  sizes.IWidth );
        Assert.Equal( 3970,  sizes.IHeight );
        Assert.Equal( 12000, sizes.RawPitch );
        Assert.Equal( 1.5,   sizes.PixelAspect );
        Assert.Equal( 5,     sizes.Flip );
    }
}
