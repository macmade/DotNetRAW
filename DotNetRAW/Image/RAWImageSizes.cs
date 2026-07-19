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

namespace DotNetRAW;

/// <summary>
/// The geometry of a RAW image, describing the sensor layout.
/// </summary>
/// <remarks>
/// Mirrors LibRAW's <c>libraw_image_sizes_t</c>. The raw dimensions cover the full
/// unpacked sensor buffer (including any masked/optical-black margins), while the
/// output dimensions describe the visible image after applying those margins.
/// </remarks>
public sealed record RAWImageSizes
{
    /// <summary>The full sensor width, in pixels, including margins.</summary>
    public int RawWidth { get; }

    /// <summary>The full sensor height, in pixels, including margins.</summary>
    public int RawHeight { get; }

    /// <summary>The visible output width, in pixels.</summary>
    public int Width { get; }

    /// <summary>The visible output height, in pixels.</summary>
    public int Height { get; }

    /// <summary>The top margin (masked rows above the visible area), in pixels.</summary>
    public int TopMargin { get; }

    /// <summary>The left margin (masked columns left of the visible area), in pixels.</summary>
    public int LeftMargin { get; }

    /// <summary>The internal image width used by LibRAW, in pixels.</summary>
    public int IWidth { get; }

    /// <summary>The internal image height used by LibRAW, in pixels.</summary>
    public int IHeight { get; }

    /// <summary>The number of bytes per row of the raw buffer.</summary>
    public int RawPitch { get; }

    /// <summary>The pixel aspect ratio (non-square-pixel sensors differ from <c>1.0</c>).</summary>
    public double PixelAspect { get; }

    /// <summary>The orientation flag, as reported by LibRAW (EXIF-style rotation code).</summary>
    public int Flip { get; }

    /// <summary>
    /// Creates image sizes from explicit values.
    /// </summary>
    /// <param name="rawWidth">The full sensor width.</param>
    /// <param name="rawHeight">The full sensor height.</param>
    /// <param name="width">The visible output width.</param>
    /// <param name="height">The visible output height.</param>
    /// <param name="topMargin">The top margin.</param>
    /// <param name="leftMargin">The left margin.</param>
    /// <param name="iWidth">The internal image width.</param>
    /// <param name="iHeight">The internal image height.</param>
    /// <param name="rawPitch">The number of bytes per raw row.</param>
    /// <param name="pixelAspect">The pixel aspect ratio.</param>
    /// <param name="flip">The orientation flag.</param>
    public RAWImageSizes( int rawWidth, int rawHeight, int width, int height, int topMargin, int leftMargin, int iWidth, int iHeight, int rawPitch, double pixelAspect, int flip )
    {
        this.RawWidth    = rawWidth;
        this.RawHeight   = rawHeight;
        this.Width       = width;
        this.Height      = height;
        this.TopMargin   = topMargin;
        this.LeftMargin  = leftMargin;
        this.IWidth      = iWidth;
        this.IHeight     = iHeight;
        this.RawPitch    = rawPitch;
        this.PixelAspect = pixelAspect;
        this.Flip        = flip;
    }

    /// <summary>
    /// Creates image sizes from a marshaled LibRAW image-sizes structure.
    /// </summary>
    /// <param name="sizes">The marshaled <c>libraw_image_sizes_t</c> fields.</param>
    internal RAWImageSizes( LibRawImageSizes sizes ) : this( sizes.RawWidth, sizes.RawHeight, sizes.Width, sizes.Height, sizes.TopMargin, sizes.LeftMargin, sizes.IWidth, sizes.IHeight, ( int )sizes.RawPitch, sizes.PixelAspect, sizes.Flip )
    {}

    /// <summary>
    /// Returns a compact summary of the raw and output dimensions.
    /// </summary>
    /// <returns>A description of the form <c>"raw W×H, output W×H"</c>.</returns>
    public override string ToString() => string.Create( CultureInfo.InvariantCulture, $"raw { this.RawWidth }×{ this.RawHeight }, output { this.Width }×{ this.Height }" );
}
