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
/// Unit tests for <see cref="FloatingPointExtensions.CompactDescription"/>, the
/// invariant-culture compact float formatter used in descriptions.
/// </summary>
public class FloatingPointExtensionsTests
{
    /// <summary>
    /// Whole values drop their fractional part and render as an integer.
    /// </summary>
    [ Fact ]
    public void CompactDescriptionRendersWholeValuesAsIntegers()
    {
        Assert.Equal( "50", ( 50.0f ).CompactDescription() );
        Assert.Equal( "70", ( 70.0 ).CompactDescription() );
        Assert.Equal( "0",  ( 0.0f ).CompactDescription() );
    }

    /// <summary>
    /// Fractional values keep their shortest round-tripping representation.
    /// </summary>
    [ Fact ]
    public void CompactDescriptionKeepsFractionalValues()
    {
        Assert.Equal( "2.8", ( 2.8f ).CompactDescription() );
        Assert.Equal( "0.5", ( 0.5 ).CompactDescription() );
    }

    /// <summary>
    /// Negative values keep their sign in both the whole and fractional forms.
    /// </summary>
    [ Fact ]
    public void CompactDescriptionHandlesNegativeValues()
    {
        Assert.Equal( "-70",  ( -70.0f ).CompactDescription() );
        Assert.Equal( "-0.5", ( -0.5f ).CompactDescription() );
    }

    /// <summary>
    /// Non-finite values fall back to their default representation rather than
    /// being coerced to an integer.
    /// </summary>
    [ Fact ]
    public void CompactDescriptionFallsBackForNonFiniteValues()
    {
        Assert.Equal( "NaN", ( float.NaN ).CompactDescription() );
    }

    /// <summary>
    /// The formatting is culture-invariant: under a non-invariant current culture
    /// the decimal separator stays a period, not the locale's separator.
    /// </summary>
    [ Fact ]
    public void CompactDescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            Assert.Equal( "2.8", ( 2.8f ).CompactDescription() );
            Assert.Equal( "0.5", ( 0.5 ).CompactDescription() );
            Assert.Equal( "50",  ( 50.0f ).CompactDescription() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
