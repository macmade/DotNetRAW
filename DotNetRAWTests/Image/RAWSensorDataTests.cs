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
using System.Linq;
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="RAWSensorData"/> and the raw-buffer accessors on
/// <see cref="RAWFile"/>, including a fixture-driven geometry check against real
/// RAW samples.
/// </summary>
public class RAWSensorDataTests
{
    /// <summary>
    /// Builds a descriptor with the given layout and placeholder geometry.
    /// </summary>
    /// <param name="layout">The layout to describe.</param>
    /// <returns>The descriptor.</returns>
    private static RAWSensorData SensorData( RAWSensorData.Layout layout ) => new RAWSensorData( 100, 50, 200, layout );

    /// <summary>
    /// The component count is derived from the layout.
    /// </summary>
    [ Fact ]
    public void ComponentCountIsDerivedFromLayout()
    {
        Assert.Equal( 0, SensorData( RAWSensorData.Layout.None ).ComponentCount );
        Assert.Equal( 1, SensorData( RAWSensorData.Layout.Bayer ).ComponentCount );
        Assert.Equal( 3, SensorData( RAWSensorData.Layout.Color3 ).ComponentCount );
        Assert.Equal( 4, SensorData( RAWSensorData.Layout.Color4 ).ComponentCount );
        Assert.Equal( 1, SensorData( RAWSensorData.Layout.BayerFloat ).ComponentCount );
        Assert.Equal( 3, SensorData( RAWSensorData.Layout.Color3Float ).ComponentCount );
        Assert.Equal( 4, SensorData( RAWSensorData.Layout.Color4Float ).ComponentCount );
    }

    /// <summary>
    /// Only the floating-point layouts report floating-point samples.
    /// </summary>
    [ Fact ]
    public void OnlyFloatLayoutsAreFloatingPoint()
    {
        Assert.False( SensorData( RAWSensorData.Layout.Bayer ).IsFloatingPoint );
        Assert.False( SensorData( RAWSensorData.Layout.Color3 ).IsFloatingPoint );
        Assert.False( SensorData( RAWSensorData.Layout.Color4 ).IsFloatingPoint );
        Assert.True( SensorData( RAWSensorData.Layout.BayerFloat ).IsFloatingPoint );
        Assert.True( SensorData( RAWSensorData.Layout.Color3Float ).IsFloatingPoint );
        Assert.True( SensorData( RAWSensorData.Layout.Color4Float ).IsFloatingPoint );
    }

    /// <summary>
    /// Only the "none" layout reports no buffer.
    /// </summary>
    [ Fact ]
    public void OnlyNoneLayoutHasNoBuffer()
    {
        Assert.False( SensorData( RAWSensorData.Layout.None ).HasBuffer );
        Assert.True( SensorData( RAWSensorData.Layout.Bayer ).HasBuffer );
        Assert.True( SensorData( RAWSensorData.Layout.Color4Float ).HasBuffer );
    }

    /// <summary>
    /// The description renders the geometry, layout and component count, with the
    /// component word pluralized.
    /// </summary>
    [ Fact ]
    public void DescriptionRendersGeometryLayoutAndComponents()
    {
        Assert.Equal( "100×50 Bayer (1 component)",       SensorData( RAWSensorData.Layout.Bayer ).ToString() );
        Assert.Equal( "100×50 3-channel (3 components)",  SensorData( RAWSensorData.Layout.Color3 ).ToString() );
        Assert.Equal( "100×50 4-channel float (4 components)", SensorData( RAWSensorData.Layout.Color4Float ).ToString() );
        Assert.Equal( "100×50 none (0 components)",       SensorData( RAWSensorData.Layout.None ).ToString() );
    }

    /// <summary>
    /// The description is culture-invariant: the geometry and component count never
    /// pick up culture-specific formatting under a non-invariant current culture.
    /// </summary>
    [ Fact ]
    public void DescriptionIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            Assert.Equal( "100×50 Bayer (1 component)", SensorData( RAWSensorData.Layout.Bayer ).ToString() );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The from-native constructor picks the layout from the first non-null buffer
    /// pointer, in priority order, and reads the geometry from the sizes.
    /// </summary>
    [ Fact ]
    public void FromNativeSelectsLayoutByFirstNonNullBuffer()
    {
        LibRawImageSizes sizes = new LibRawImageSizes { RawWidth = 6000, RawHeight = 4000, RawPitch = 12000 };

        Assert.Equal( RAWSensorData.Layout.Bayer,       LayoutOf( new LibRawRawData { RawImage    = new IntPtr( 1 ) }, sizes ) );
        Assert.Equal( RAWSensorData.Layout.Color3,      LayoutOf( new LibRawRawData { Color3Image = new IntPtr( 1 ) }, sizes ) );
        Assert.Equal( RAWSensorData.Layout.Color4,      LayoutOf( new LibRawRawData { Color4Image = new IntPtr( 1 ) }, sizes ) );
        Assert.Equal( RAWSensorData.Layout.BayerFloat,  LayoutOf( new LibRawRawData { FloatImage  = new IntPtr( 1 ) }, sizes ) );
        Assert.Equal( RAWSensorData.Layout.Color3Float, LayoutOf( new LibRawRawData { Float3Image = new IntPtr( 1 ) }, sizes ) );
        Assert.Equal( RAWSensorData.Layout.Color4Float, LayoutOf( new LibRawRawData { Float4Image = new IntPtr( 1 ) }, sizes ) );
        Assert.Equal( RAWSensorData.Layout.None,        LayoutOf( new LibRawRawData(), sizes ) );

        // The single-channel Bayer buffer wins over the others when several are set.
        LibRawRawData several = new LibRawRawData { RawImage = new IntPtr( 1 ), Color3Image = new IntPtr( 1 ), Float4Image = new IntPtr( 1 ) };

        Assert.Equal( RAWSensorData.Layout.Bayer, LayoutOf( several, sizes ) );
    }

    /// <summary>
    /// The from-native constructor reads the geometry from the image sizes.
    /// </summary>
    [ Fact ]
    public void FromNativeReadsGeometry()
    {
        LibRawImageSizes sizes  = new LibRawImageSizes { RawWidth = 6000, RawHeight = 4000, RawPitch = 12000 };
        RAWSensorData    sensor = new RAWSensorData( new LibRawRawData { RawImage = new IntPtr( 1 ) }, sizes );

        Assert.Equal( 6000,  sensor.Width );
        Assert.Equal( 4000,  sensor.Height );
        Assert.Equal( 12000, sensor.Pitch );
    }

    /// <summary>
    /// For real Bayer samples the copied buffer and the borrowed zero-copy view agree,
    /// match the sensor geometry, and carry non-trivial data. A non-Bayer layout exposes
    /// no 16-bit Bayer buffer.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void RawImageMatchesGeometry( string path )
    {
        using RAWFile file   = new RAWFile( path );
        RAWSensorData sensor = file.SensorData;

        if( sensor.BufferLayout != RAWSensorData.Layout.Bayer )
        {
            Assert.Null( file.RawImage );

            return;
        }

        ushort[]? image = file.RawImage;

        Assert.NotNull( image );
        Assert.NotEmpty( image );
        Assert.Equal( sensor.Height * ( sensor.Pitch / 2 ), image.Length );
        Assert.True( image.Max() > 0 );

        int viewLength = file.WithRawImage( samples => samples.Length );

        Assert.Equal( image.Length, viewLength );
    }

    /// <summary>
    /// Resolves the layout the from-native constructor derives for a raw-data
    /// structure.
    /// </summary>
    /// <param name="rawData">The raw-data structure.</param>
    /// <param name="sizes">The image sizes.</param>
    /// <returns>The derived layout.</returns>
    private static RAWSensorData.Layout LayoutOf( LibRawRawData rawData, LibRawImageSizes sizes ) => new RAWSensorData( rawData, sizes ).BufferLayout;
}
