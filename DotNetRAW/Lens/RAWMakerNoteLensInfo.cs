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

namespace DotNetRAW;

/// <summary>
/// A curated, cross-vendor subset of the lens information found in a file's maker
/// notes.
/// </summary>
/// <remarks>
/// Mirrors the broadly useful fields of LibRAW's <c>libraw_makernotes_lens_t</c>.
/// Camera-specific fields (mount/format codes, accessory identifiers, per-vendor lens
/// substructures) are intentionally out of scope. The focal-type property is named
/// <see cref="LensFocalType"/> rather than <c>FocalType</c> so it does not collide
/// with the nested <see cref="FocalType"/> enum.
/// </remarks>
public sealed record RAWMakerNoteLensInfo
{
    /// <summary>
    /// Whether a lens has a fixed or a variable focal length.
    /// </summary>
    public enum FocalType
    {
        /// <summary>The focal type is unknown.</summary>
        Unknown,

        /// <summary>A fixed-focal-length (prime) lens.</summary>
        Fixed,

        /// <summary>A variable-focal-length (zoom) lens.</summary>
        Zoom,
    }

    /// <summary>The LibRAW lens identifier.</summary>
    public ulong LensID { get; }

    /// <summary>The lens name recorded in the maker notes.</summary>
    public string LensModel { get; }

    /// <summary>Whether the lens is a prime or a zoom.</summary>
    public FocalType LensFocalType { get; }

    /// <summary>The minimum focal length, in millimetres.</summary>
    public float MinFocal { get; }

    /// <summary>The maximum focal length, in millimetres.</summary>
    public float MaxFocal { get; }

    /// <summary>The maximum aperture (smallest f-number) at the minimum focal length.</summary>
    public float MaxApertureAtMinFocal { get; }

    /// <summary>The maximum aperture (smallest f-number) at the maximum focal length.</summary>
    public float MaxApertureAtMaxFocal { get; }

    /// <summary>The widest maximum aperture (smallest f-number) of the lens.</summary>
    public float MaxAperture { get; }

    /// <summary>The smallest minimum aperture (largest f-number) of the lens.</summary>
    public float MinAperture { get; }

    /// <summary>The focal length in 35&#160;mm-equivalent millimetres.</summary>
    public float FocalLengthIn35mmFormat { get; }

    /// <summary>The aperture range of the lens, in f-stops.</summary>
    public float LensFStops { get; }

    /// <summary>The minimum focus distance, in metres.</summary>
    public float MinFocusDistance { get; }

    /// <summary>
    /// Creates maker-note lens information from explicit values.
    /// </summary>
    /// <param name="lensID">The LibRAW lens identifier.</param>
    /// <param name="lensModel">The lens name.</param>
    /// <param name="focalType">Whether the lens is a prime or a zoom.</param>
    /// <param name="minFocal">The minimum focal length.</param>
    /// <param name="maxFocal">The maximum focal length.</param>
    /// <param name="maxApertureAtMinFocal">The maximum aperture at the minimum focal length.</param>
    /// <param name="maxApertureAtMaxFocal">The maximum aperture at the maximum focal length.</param>
    /// <param name="maxAperture">The widest maximum aperture.</param>
    /// <param name="minAperture">The smallest minimum aperture.</param>
    /// <param name="focalLengthIn35mmFormat">The 35&#160;mm-equivalent focal length.</param>
    /// <param name="lensFStops">The aperture range in f-stops.</param>
    /// <param name="minFocusDistance">The minimum focus distance.</param>
    public RAWMakerNoteLensInfo( ulong lensID, string lensModel, FocalType focalType, float minFocal, float maxFocal, float maxApertureAtMinFocal, float maxApertureAtMaxFocal, float maxAperture, float minAperture, float focalLengthIn35mmFormat, float lensFStops, float minFocusDistance )
    {
        this.LensID                  = lensID;
        this.LensModel               = lensModel;
        this.LensFocalType           = focalType;
        this.MinFocal                = minFocal;
        this.MaxFocal                = maxFocal;
        this.MaxApertureAtMinFocal   = maxApertureAtMinFocal;
        this.MaxApertureAtMaxFocal   = maxApertureAtMaxFocal;
        this.MaxAperture             = maxAperture;
        this.MinAperture             = minAperture;
        this.FocalLengthIn35mmFormat = focalLengthIn35mmFormat;
        this.LensFStops              = lensFStops;
        this.MinFocusDistance        = minFocusDistance;
    }

    /// <summary>
    /// Creates maker-note lens information from a marshaled LibRAW maker-note lens
    /// structure.
    /// </summary>
    /// <param name="makerNotes">The marshaled <c>libraw_makernotes_lens_t</c> fields.</param>
    internal RAWMakerNoteLensInfo( LibRawMakerNotesLens makerNotes ) : this(
        makerNotes.LensID,
        makerNotes.Lens.DecodeCString(),
        FocalTypeFromCode( makerNotes.FocalType ),
        makerNotes.MinFocal,
        makerNotes.MaxFocal,
        makerNotes.MaxAp4MinFocal,
        makerNotes.MaxAp4MaxFocal,
        makerNotes.MaxAp,
        makerNotes.MinAp,
        makerNotes.FocalLengthIn35mmFormat,
        makerNotes.LensFStops,
        makerNotes.MinFocusDistance
    )
    {}

    /// <summary>
    /// Maps LibRAW's <c>FocalType</c> code to a <see cref="FocalType"/>.
    /// </summary>
    /// <param name="code">
    /// The LibRAW focal-type code: <c>1</c> is fixed, <c>2</c> is zoom, and anything
    /// else (including <c>-1</c> and <c>0</c>) is unknown.
    /// </param>
    /// <returns>The corresponding <see cref="FocalType"/>.</returns>
    internal static FocalType FocalTypeFromCode( int code )
    {
        return code switch
        {
            1 => FocalType.Fixed,
            2 => FocalType.Zoom,
            _ => FocalType.Unknown,
        };
    }

    /// <summary>
    /// Returns a compact summary of the lens name and focal range.
    /// </summary>
    /// <returns>
    /// The lens model followed by its focal range in parentheses when a model is
    /// recorded (e.g. <c>"EF24-70mm f/2.8L (24–70mm)"</c>), or the focal range alone
    /// otherwise.
    /// </returns>
    public override string ToString()
    {
        string range = this.MinFocal == this.MaxFocal
            ? $"{ this.MinFocal.CompactDescription() }mm"
            : $"{ this.MinFocal.CompactDescription() }–{ this.MaxFocal.CompactDescription() }mm";

        return this.LensModel.Length == 0 ? range : $"{ this.LensModel } ({ range })";
    }
}
