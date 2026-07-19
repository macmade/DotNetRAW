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

using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="RAWCFAPattern"/> colour lookups and equality, plus a
/// fixture-driven consistency check against real RAW samples.
/// </summary>
public class RAWCFAPatternTests
{
    /// <summary>
    /// The 6×6 X-Trans grid used across the X-Trans tests.
    /// </summary>
    private static readonly int[][] XTransGrid =
    [
        [ 1, 1, 0, 1, 1, 2 ],
        [ 1, 1, 2, 1, 1, 0 ],
        [ 2, 0, 1, 0, 2, 1 ],
        [ 1, 1, 2, 1, 1, 0 ],
        [ 1, 1, 0, 1, 1, 2 ],
        [ 0, 2, 1, 2, 0, 1 ],
    ];

    /// <summary>
    /// The canonical RGGB Bayer mosaic resolves to the expected per-pixel colours
    /// and channels across the repeating 2×2 cell.
    /// </summary>
    [ Fact ]
    public void BayerRGGB()
    {
        RAWCFAPattern pattern = new RAWCFAPattern( 0x94949494u, "RGBG" );

        Assert.Equal( RAWCFAPattern.Kind.Bayer, pattern.PatternKind );

        Assert.Equal( 0, pattern.Color( 0, 0 ) );
        Assert.Equal( 1, pattern.Color( 0, 1 ) );
        Assert.Equal( 1, pattern.Color( 1, 0 ) );
        Assert.Equal( 2, pattern.Color( 1, 1 ) );

        Assert.Equal( 'R', pattern.Channel( 0, 0 ) );
        Assert.Equal( 'G', pattern.Channel( 0, 1 ) );
        Assert.Equal( 'G', pattern.Channel( 1, 0 ) );
        Assert.Equal( 'B', pattern.Channel( 1, 1 ) );
    }

    /// <summary>
    /// The Bayer pattern repeats every two pixels in each direction.
    /// </summary>
    [ Fact ]
    public void BayerIsPeriodic()
    {
        RAWCFAPattern pattern = new RAWCFAPattern( 0x94949494u, "RGBG" );

        Assert.Equal( pattern.Color( 0, 0 ), pattern.Color( 2, 2 ) );
        Assert.Equal( pattern.Color( 1, 1 ), pattern.Color( 3, 3 ) );
    }

    /// <summary>
    /// A <c>filters</c> value of <c>0</c> is a sensor without a CFA: every pixel
    /// reports the full-colour index and has no single channel.
    /// </summary>
    [ Fact ]
    public void NoColorFilterArray()
    {
        RAWCFAPattern pattern = new RAWCFAPattern( 0, "" );

        Assert.Equal( RAWCFAPattern.Kind.None, pattern.PatternKind );
        Assert.Equal( RAWCFAPattern.FullColorIndex, pattern.Color( 3, 7 ) );
        Assert.Null( pattern.Channel( 3, 7 ) );
    }

    /// <summary>
    /// A <c>filters</c> value of <c>9</c> is X-Trans and looks up the 6×6 grid,
    /// wrapping with the sensor's period.
    /// </summary>
    [ Fact ]
    public void XTransUsesGrid()
    {
        RAWCFAPattern pattern = new RAWCFAPattern( 9, "RGBG", XTransGrid );

        Assert.Equal( RAWCFAPattern.Kind.XTrans, pattern.PatternKind );
        Assert.Equal( 0, pattern.Color( 0, 2 ) );
        Assert.Equal( 2, pattern.Color( 2, 0 ) );
        Assert.Equal( XTransGrid[ 0 ][ 2 ], pattern.Color( 6, 8 ) );
    }

    /// <summary>
    /// The description names the kind and channel descriptor.
    /// </summary>
    [ Fact ]
    public void DescriptionNamesKindAndDescriptor()
    {
        Assert.Equal( "Bayer (RGBG)",   new RAWCFAPattern( 0x94949494u, "RGBG" ).ToString() );
        Assert.Equal( "no CFA",         new RAWCFAPattern( 0, "" ).ToString() );
        Assert.Equal( "X-Trans (RGBG)", new RAWCFAPattern( 9, "RGBG", XTransGrid ).ToString() );
    }

    /// <summary>
    /// The from-native constructor reads the filters, colour descriptor and, for
    /// X-Trans, reshapes the flat <c>xtrans</c> grid row-major.
    /// </summary>
    [ Fact ]
    public void FromNativeReadsFiltersDescriptorAndGrid()
    {
        sbyte[] xtrans = new sbyte[ 36 ];

        for( int index = 0; index < xtrans.Length; index++ )
        {
            xtrans[ index ] = ( sbyte )index;
        }

        LibRawIParams idata = new LibRawIParams
        {
            Filters = 9,
            Cdesc   = [ ( byte )'R', ( byte )'G', ( byte )'B', ( byte )'G', 0 ],
            XTrans  = xtrans,
        };

        RAWCFAPattern pattern = new RAWCFAPattern( idata );

        Assert.Equal( RAWCFAPattern.Kind.XTrans, pattern.PatternKind );
        Assert.Equal( 9u, pattern.Filters );
        Assert.Equal( "RGBG", pattern.ColorDescription );
        Assert.NotNull( pattern.XTransPattern );
        Assert.Equal( new int[] { 0, 1, 2, 3, 4, 5 },       pattern.XTransPattern[ 0 ] );
        Assert.Equal( new int[] { 30, 31, 32, 33, 34, 35 }, pattern.XTransPattern[ 5 ] );
    }

    /// <summary>
    /// A non-X-Trans file leaves the grid absent.
    /// </summary>
    [ Fact ]
    public void FromNativeLeavesGridAbsentForBayer()
    {
        LibRawIParams idata = new LibRawIParams
        {
            Filters = 0x94949494u,
            Cdesc   = [ ( byte )'R', ( byte )'G', ( byte )'B', ( byte )'G', 0 ],
        };

        RAWCFAPattern pattern = new RAWCFAPattern( idata );

        Assert.Equal( RAWCFAPattern.Kind.Bayer, pattern.PatternKind );
        Assert.Null( pattern.XTransPattern );
    }

    /// <summary>
    /// Equality compares the X-Trans grid structurally, not by reference.
    /// </summary>
    [ Fact ]
    public void EqualityComparesGridStructurally()
    {
        int[][] sameGrid =
        [
            [ 1, 1, 0, 1, 1, 2 ],
            [ 1, 1, 2, 1, 1, 0 ],
            [ 2, 0, 1, 0, 2, 1 ],
            [ 1, 1, 2, 1, 1, 0 ],
            [ 1, 1, 0, 1, 1, 2 ],
            [ 0, 2, 1, 2, 0, 1 ],
        ];

        RAWCFAPattern first  = new RAWCFAPattern( 9, "RGBG", XTransGrid );
        RAWCFAPattern second = new RAWCFAPattern( 9, "RGBG", sameGrid );

        Assert.Equal( first, second );
        Assert.Equal( first.GetHashCode(), second.GetHashCode() );

        int[][] otherGrid =
        [
            [ 0, 0, 0, 0, 0, 0 ],
            [ 0, 0, 0, 0, 0, 0 ],
            [ 0, 0, 0, 0, 0, 0 ],
            [ 0, 0, 0, 0, 0, 0 ],
            [ 0, 0, 0, 0, 0, 0 ],
            [ 0, 0, 0, 0, 0, 0 ],
        ];

        Assert.NotEqual( first, new RAWCFAPattern( 9, "RGBG", otherGrid ) );
    }

    /// <summary>
    /// Two Bayer patterns with the same filters and descriptor are equal.
    /// </summary>
    [ Fact ]
    public void EqualityComparesBayerFields()
    {
        Assert.Equal( new RAWCFAPattern( 0x94949494u, "RGBG" ), new RAWCFAPattern( 0x94949494u, "RGBG" ) );
        Assert.NotEqual( new RAWCFAPattern( 0x94949494u, "RGBG" ), new RAWCFAPattern( 0x61616161u, "RGBG" ) );
    }

    /// <summary>
    /// The CFA pattern read off a real file is consistent with the raw identification
    /// metadata it is derived from.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void CfaPatternMatchesImageInfo( string path )
    {
        using RAWFile file = new RAWFile( path );
        RAWCFAPattern cfa  = file.CfaPattern;

        Assert.Equal( file.ImageInfo.Filters, cfa.Filters );
        Assert.Equal( file.ImageInfo.ColorDescription, cfa.ColorDescription );
    }
}
