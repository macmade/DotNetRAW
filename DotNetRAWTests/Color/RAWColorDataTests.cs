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
/// Unit tests for <see cref="RAWColorData"/>.
/// </summary>
public class RAWColorDataTests
{
    /// <summary>
    /// The description summarizes the black and saturation levels, noting an
    /// embedded ICC profile when present.
    /// </summary>
    [ Fact ]
    public void DescriptionReflectsBlackMaximumAndProfile()
    {
        Assert.Equal( "black 2048, max 16383",              Sample( false ).ToString() );
        Assert.Equal( "black 2048, max 16383, ICC profile", Sample( true ).ToString() );
    }

    /// <summary>
    /// The description is culture-invariant: the levels never pick up
    /// culture-specific formatting under a non-invariant current culture.
    /// </summary>
    [ Fact ]
    public void DescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            Assert.Equal( "black 2048, max 16383", Sample( false ).ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The from-native constructor maps the scalar levels, converts the four
    /// per-channel black levels, and reshapes the flat matrices row-major.
    /// </summary>
    [ Fact ]
    public void FromNativeMapsScalarsArraysAndMatrices()
    {
        LibRawColorData color = new LibRawColorData
        {
            Black             = 2048,
            Maximum           = 16383,
            DataMaximum       = 15000,
            CameraMultipliers = [ 1, 2, 3, 4 ],
            PreMultipliers    = [ 5, 6, 7, 8 ],
            RgbCamera         = [ 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 ],
            CameraXyz         = [ 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41 ],
            ColorMatrix       = [ 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61 ],
            Profile           = new IntPtr( 1 ),
            ProfileLength     = 560,
        };

        RAWColorData data = new RAWColorData( color, [ 100u, 200u, 300u, 400u ] );

        Assert.Equal( 2048,  data.BlackLevel );
        Assert.Equal( 16383, data.Maximum );
        Assert.Equal( 15000, data.DataMaximum );
        Assert.Equal( new int[] { 100, 200, 300, 400 }, data.ChannelBlackLevels );

        Assert.Equal( new float[] { 1, 2, 3, 4 }, data.CameraMultipliers );
        Assert.Equal( new float[] { 5, 6, 7, 8 }, data.PreMultipliers );

        Assert.Equal( 3, data.RgbCamera.Length );
        Assert.All( data.RgbCamera, row => Assert.Equal( 4, row.Length ) );
        Assert.Equal( new float[] { 10, 11, 12, 13 }, data.RgbCamera[ 0 ] );
        Assert.Equal( new float[] { 18, 19, 20, 21 }, data.RgbCamera[ 2 ] );

        Assert.Equal( 4, data.CameraXyz.Length );
        Assert.All( data.CameraXyz, row => Assert.Equal( 3, row.Length ) );
        Assert.Equal( new float[] { 30, 31, 32 }, data.CameraXyz[ 0 ] );
        Assert.Equal( new float[] { 39, 40, 41 }, data.CameraXyz[ 3 ] );

        Assert.Equal( 3, data.ColorMatrix.Length );
        Assert.All( data.ColorMatrix, row => Assert.Equal( 4, row.Length ) );
        Assert.Equal( new float[] { 58, 59, 60, 61 }, data.ColorMatrix[ 2 ] );

        Assert.True( data.HasEmbeddedColorProfile );
        Assert.Equal( 560, data.ColorProfileLength );
    }

    /// <summary>
    /// An embedded colour profile requires both a non-null pointer and a positive
    /// length.
    /// </summary>
    [ Fact ]
    public void FromNativeReadsEmbeddedProfileFlag()
    {
        Assert.True( ColorFrom( new IntPtr( 1 ), 560 ).HasEmbeddedColorProfile );
        Assert.False( ColorFrom( IntPtr.Zero, 560 ).HasEmbeddedColorProfile );
        Assert.False( ColorFrom( new IntPtr( 1 ), 0 ).HasEmbeddedColorProfile );
    }

    /// <summary>
    /// Equality compares the arrays and matrices structurally, not by reference,
    /// and every structural member participates: changing any one breaks equality.
    /// </summary>
    [ Fact ]
    public void EqualityComparesArraysAndMatricesStructurally()
    {
        RAWColorData first  = Sample( true );
        RAWColorData second = Sample( true );

        Assert.Equal( first, second );
        Assert.Equal( first.GetHashCode(), second.GetHashCode() );

        Assert.NotEqual( first, With( first, maximum: 9999 ) );
        Assert.NotEqual( first, With( first, channelBlackLevels: [ 10, 20, 30, 99 ] ) );
        Assert.NotEqual( first, With( first, cameraMultipliers: [ 1, 2, 3, 99 ] ) );
        Assert.NotEqual( first, With( first, rgbCamera: [ [ 10, 11, 12, 13 ], [ 14, 15, 16, 17 ], [ 18, 19, 20, 99 ] ] ) );
        Assert.NotEqual( first, With( first, cameraXyz: [ [ 30, 31, 32 ], [ 33, 34, 35 ], [ 36, 37, 38 ], [ 39, 40, 99 ] ] ) );
    }

    /// <summary>
    /// Copies a colour-data value, overriding one member, to exercise that member's
    /// participation in equality.
    /// </summary>
    /// <param name="source">The value to copy.</param>
    /// <param name="maximum">An overriding saturation level, or <see langword="null"/>.</param>
    /// <param name="channelBlackLevels">Overriding per-channel black levels, or <see langword="null"/>.</param>
    /// <param name="cameraMultipliers">Overriding camera multipliers, or <see langword="null"/>.</param>
    /// <param name="rgbCamera">An overriding camera-to-sRGB matrix, or <see langword="null"/>.</param>
    /// <param name="cameraXyz">An overriding camera-to-XYZ matrix, or <see langword="null"/>.</param>
    /// <returns>The copy with the requested override applied.</returns>
    private static RAWColorData With( RAWColorData source, int? maximum = null, int[]? channelBlackLevels = null, float[]? cameraMultipliers = null, float[][]? rgbCamera = null, float[][]? cameraXyz = null )
    {
        return new RAWColorData(
            source.BlackLevel,
            channelBlackLevels ?? source.ChannelBlackLevels,
            maximum ?? source.Maximum,
            source.DataMaximum,
            cameraMultipliers ?? source.CameraMultipliers,
            source.PreMultipliers,
            rgbCamera ?? source.RgbCamera,
            cameraXyz ?? source.CameraXyz,
            source.ColorMatrix,
            source.HasEmbeddedColorProfile,
            source.ColorProfileLength
        );
    }

    /// <summary>
    /// Builds a fully-populated colour-data value with the given profile presence.
    /// </summary>
    /// <param name="hasEmbeddedColorProfile">Whether an ICC profile is embedded.</param>
    /// <returns>The colour data.</returns>
    private static RAWColorData Sample( bool hasEmbeddedColorProfile )
    {
        return new RAWColorData(
            2048,
            [ 10, 20, 30, 40 ],
            16383,
            15000,
            [ 1, 2, 3, 4 ],
            [ 5, 6, 7, 8 ],
            [ [ 10, 11, 12, 13 ], [ 14, 15, 16, 17 ], [ 18, 19, 20, 21 ] ],
            [ [ 30, 31, 32 ], [ 33, 34, 35 ], [ 36, 37, 38 ], [ 39, 40, 41 ] ],
            [ [ 50, 51, 52, 53 ], [ 54, 55, 56, 57 ], [ 58, 59, 60, 61 ] ],
            hasEmbeddedColorProfile,
            560
        );
    }

    /// <summary>
    /// Builds colour data from a native structure with the given profile pointer
    /// and length.
    /// </summary>
    /// <param name="profile">The embedded-profile pointer.</param>
    /// <param name="profileLength">The embedded-profile length, in bytes.</param>
    /// <returns>The colour data.</returns>
    private static RAWColorData ColorFrom( IntPtr profile, uint profileLength )
    {
        LibRawColorData color = new LibRawColorData
        {
            CameraMultipliers = [ 1, 2, 3, 4 ],
            PreMultipliers    = [ 5, 6, 7, 8 ],
            RgbCamera         = new float[ 12 ],
            CameraXyz         = new float[ 12 ],
            ColorMatrix       = new float[ 12 ],
            Profile           = profile,
            ProfileLength     = profileLength,
        };

        return new RAWColorData( color, [ 0u, 0u, 0u, 0u ] );
    }
}
