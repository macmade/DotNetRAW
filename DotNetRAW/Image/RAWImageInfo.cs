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
/// Camera identification and sensor-description metadata for a RAW image.
/// </summary>
/// <remarks>
/// Mirrors LibRAW's <c>libraw_iparams_t</c>. Alongside the human-readable camera
/// make and model, LibRAW also provides normalized identifiers (stable strings it
/// derives across firmware and marketing variations) and the colour-filter
/// description used to interpret the raw samples.
/// </remarks>
public sealed record RAWImageInfo
{
    /// <summary>The camera manufacturer, as recorded in the file.</summary>
    public string Make { get; }

    /// <summary>The camera model, as recorded in the file.</summary>
    public string Model { get; }

    /// <summary>The creating software, as recorded in the file.</summary>
    public string Software { get; }

    /// <summary>The manufacturer, normalized by LibRAW to a canonical spelling.</summary>
    public string NormalizedMake { get; }

    /// <summary>The model, normalized by LibRAW to a canonical spelling.</summary>
    public string NormalizedModel { get; }

    /// <summary>The number of colours in the colour-filter array (e.g. <c>3</c> or <c>4</c>).</summary>
    public int Colors { get; }

    /// <summary>
    /// The packed colour-filter-array pattern, as encoded by LibRAW.
    /// </summary>
    /// <remarks>The <c>RAWCFAPattern</c> type offers a structured interpretation of this value.</remarks>
    public uint Filters { get; }

    /// <summary>
    /// The colour descriptor mapping colour indices to channel letters (e.g. <c>"RGBG"</c>).
    /// </summary>
    public string ColorDescription { get; }

    /// <summary>The number of raw images stored in the file.</summary>
    public int RawCount { get; }

    /// <summary>The DNG version, or <c>0</c> if the file is not a DNG.</summary>
    public int DngVersion { get; }

    /// <summary>
    /// Whether the sensor is a Foveon (layered) sensor, which has no colour-filter
    /// array.
    /// </summary>
    public bool IsFoveon { get; }

    /// <summary>
    /// Creates image information from explicit values.
    /// </summary>
    /// <param name="make">The camera manufacturer.</param>
    /// <param name="model">The camera model.</param>
    /// <param name="software">The creating software.</param>
    /// <param name="normalizedMake">The normalized manufacturer.</param>
    /// <param name="normalizedModel">The normalized model.</param>
    /// <param name="colors">The number of colours in the colour-filter array.</param>
    /// <param name="filters">The packed colour-filter-array pattern.</param>
    /// <param name="colorDescription">The colour descriptor.</param>
    /// <param name="rawCount">The number of raw images in the file.</param>
    /// <param name="dngVersion">The DNG version, or <c>0</c>.</param>
    /// <param name="isFoveon">Whether the sensor is a Foveon sensor.</param>
    public RAWImageInfo( string make, string model, string software, string normalizedMake, string normalizedModel, int colors, uint filters, string colorDescription, int rawCount, int dngVersion, bool isFoveon )
    {
        this.Make             = make;
        this.Model            = model;
        this.Software         = software;
        this.NormalizedMake   = normalizedMake;
        this.NormalizedModel  = normalizedModel;
        this.Colors           = colors;
        this.Filters          = filters;
        this.ColorDescription = colorDescription;
        this.RawCount         = rawCount;
        this.DngVersion       = dngVersion;
        this.IsFoveon         = isFoveon;
    }

    /// <summary>
    /// Creates image information from a marshaled LibRAW image-parameters structure.
    /// </summary>
    /// <param name="idata">The marshaled <c>libraw_iparams_t</c> fields.</param>
    internal RAWImageInfo( LibRawIParams idata ) : this(
        idata.Make.DecodeCString(),
        idata.Model.DecodeCString(),
        idata.Software.DecodeCString(),
        idata.NormalizedMake.DecodeCString(),
        idata.NormalizedModel.DecodeCString(),
        idata.Colors,
        idata.Filters,
        idata.Cdesc.DecodeCString(),
        ( int )idata.RawCount,
        ( int )idata.DngVersion,
        idata.IsFoveon != 0
    )
    {}

    /// <summary>
    /// Returns a compact summary of the camera and colour-filter description.
    /// </summary>
    /// <returns>
    /// A description of the form <c>"Make Model — N colors (RGBG)"</c>, using
    /// <c>"Unknown camera"</c> when neither the make nor the model is recorded.
    /// </returns>
    public override string ToString()
    {
        string camera = $"{ this.Make } { this.Model }".Trim();
        string name   = camera.Length == 0 ? "Unknown camera" : camera;

        return string.Create( CultureInfo.InvariantCulture, $"{ name } — { this.Colors } colors ({ this.ColorDescription })" );
    }
}
