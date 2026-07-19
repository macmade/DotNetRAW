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

namespace DotNetRAW;

/// <summary>
/// A description of the unpacked raw sensor buffer.
/// </summary>
/// <remarks>
/// Mirrors the geometry and layout of LibRAW's <c>libraw_rawdata_t</c>. After a RAW
/// file is unpacked, LibRAW populates exactly one of several buffer variants
/// depending on the camera and decode path; <see cref="BufferLayout"/> reports which
/// one, so a consumer knows what the data looks like even for the less common
/// multi-channel and floating-point variants that have no dedicated accessor.
/// </remarks>
public sealed record RAWSensorData
{
    /// <summary>
    /// The in-memory form of the unpacked sensor data.
    /// </summary>
    public enum Layout
    {
        /// <summary>
        /// No raw buffer is available (the file has not been unpacked, or the
        /// decoder produced none).
        /// </summary>
        None,

        /// <summary>A single-channel 16-bit Bayer/mosaic buffer (<c>raw_image</c>).</summary>
        Bayer,

        /// <summary>A three-channel 16-bit buffer (<c>color3_image</c>).</summary>
        Color3,

        /// <summary>A four-channel 16-bit buffer (<c>color4_image</c>).</summary>
        Color4,

        /// <summary>A single-channel floating-point buffer (<c>float_image</c>).</summary>
        BayerFloat,

        /// <summary>A three-channel floating-point buffer (<c>float3_image</c>).</summary>
        Color3Float,

        /// <summary>A four-channel floating-point buffer (<c>float4_image</c>).</summary>
        Color4Float,
    }

    /// <summary>The full sensor width, in pixels.</summary>
    public int Width { get; }

    /// <summary>The full sensor height, in pixels.</summary>
    public int Height { get; }

    /// <summary>The number of bytes per row of the raw buffer.</summary>
    public int Pitch { get; }

    /// <summary>The in-memory layout LibRAW populated for the unpacked data.</summary>
    public Layout BufferLayout { get; }

    /// <summary>
    /// The number of components per pixel (<c>1</c> for Bayer, <c>3</c>, <c>4</c>, or
    /// <c>0</c> when no buffer is available).
    /// </summary>
    public int ComponentCount
    {
        get
        {
            switch( this.BufferLayout )
            {
                case Layout.Bayer:
                case Layout.BayerFloat:

                    return 1;

                case Layout.Color3:
                case Layout.Color3Float:

                    return 3;

                case Layout.Color4:
                case Layout.Color4Float:

                    return 4;

                default:

                    return 0;
            }
        }
    }

    /// <summary>
    /// Whether the samples are floating-point rather than 16-bit integers.
    /// </summary>
    public bool IsFloatingPoint => this.BufferLayout is Layout.BayerFloat or Layout.Color3Float or Layout.Color4Float;

    /// <summary>
    /// Whether an unpacked raw buffer pointer is present.
    /// </summary>
    /// <remarks>
    /// This reflects buffer presence, not that a 16-bit Bayer view can necessarily
    /// be vended - that additionally requires a valid row pitch.
    /// </remarks>
    public bool HasBuffer => this.BufferLayout != Layout.None;

    /// <summary>
    /// Creates sensor data from explicit values.
    /// </summary>
    /// <param name="width">The full sensor width.</param>
    /// <param name="height">The full sensor height.</param>
    /// <param name="pitch">The number of bytes per raw row.</param>
    /// <param name="layout">The in-memory layout of the unpacked data.</param>
    public RAWSensorData( int width, int height, int pitch, Layout layout )
    {
        this.Width        = width;
        this.Height       = height;
        this.Pitch        = pitch;
        this.BufferLayout = layout;
    }

    /// <summary>
    /// Creates sensor data from LibRAW's raw data and image sizes.
    /// </summary>
    /// <remarks>
    /// The layout is derived from whichever buffer pointer LibRAW populated, checked
    /// in priority order; when none are set the layout is <see cref="Layout.None"/>.
    /// </remarks>
    /// <param name="rawData">The marshaled <c>libraw_rawdata_t</c> fields.</param>
    /// <param name="sizes">The marshaled <c>libraw_image_sizes_t</c> fields.</param>
    internal RAWSensorData( LibRawRawData rawData, LibRawImageSizes sizes ) : this( sizes.RawWidth, sizes.RawHeight, ( int )sizes.RawPitch, SelectLayout( rawData ) )
    {}

    /// <summary>
    /// Returns a compact summary of the buffer geometry and layout.
    /// </summary>
    /// <returns>A description of the form <c>"W×H layout (N component(s))"</c>.</returns>
    public override string ToString()
    {
        string plural = this.ComponentCount == 1 ? "" : "s";

        return string.Create( CultureInfo.InvariantCulture, $"{ this.Width }×{ this.Height } { Describe( this.BufferLayout ) } ({ this.ComponentCount } component{ plural })" );
    }

    /// <summary>
    /// Derives the layout from whichever LibRAW buffer pointer is non-null, in
    /// priority order.
    /// </summary>
    /// <param name="rawData">The marshaled raw-data fields.</param>
    /// <returns>The derived layout, or <see cref="Layout.None"/> when no buffer is set.</returns>
    private static Layout SelectLayout( LibRawRawData rawData )
    {
        if( rawData.RawImage != IntPtr.Zero )
        {
            return Layout.Bayer;
        }

        if( rawData.Color3Image != IntPtr.Zero )
        {
            return Layout.Color3;
        }

        if( rawData.Color4Image != IntPtr.Zero )
        {
            return Layout.Color4;
        }

        if( rawData.FloatImage != IntPtr.Zero )
        {
            return Layout.BayerFloat;
        }

        if( rawData.Float3Image != IntPtr.Zero )
        {
            return Layout.Color3Float;
        }

        if( rawData.Float4Image != IntPtr.Zero )
        {
            return Layout.Color4Float;
        }

        return Layout.None;
    }

    /// <summary>
    /// Returns the short display name of a layout.
    /// </summary>
    /// <param name="layout">The layout to name.</param>
    /// <returns>The layout's short name.</returns>
    private static string Describe( Layout layout )
    {
        return layout switch
        {
            Layout.Bayer       => "Bayer",
            Layout.Color3      => "3-channel",
            Layout.Color4      => "4-channel",
            Layout.BayerFloat  => "Bayer float",
            Layout.Color3Float => "3-channel float",
            Layout.Color4Float => "4-channel float",
            _                  => "none",
        };
    }
}
