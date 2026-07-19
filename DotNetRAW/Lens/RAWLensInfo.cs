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
/// Lens identification and characteristics for a RAW capture.
/// </summary>
/// <remarks>
/// Mirrors LibRAW's <c>libraw_lensinfo_t</c>: the focal/aperture range and lens
/// identity parsed from EXIF, with the maker-note (<see cref="MakerNotes"/>) and DNG
/// (<see cref="Dng"/>) lens substructures surfaced separately. Per-vendor lens
/// substructures are intentionally out of scope.
/// </remarks>
public sealed record RAWLensInfo
{
    /// <summary>The minimum focal length, in millimetres.</summary>
    public float MinFocal { get; }

    /// <summary>The maximum focal length, in millimetres.</summary>
    public float MaxFocal { get; }

    /// <summary>The maximum aperture (smallest f-number) at the minimum focal length.</summary>
    public float MaxApertureAtMinFocal { get; }

    /// <summary>The maximum aperture (smallest f-number) at the maximum focal length.</summary>
    public float MaxApertureAtMaxFocal { get; }

    /// <summary>The maximum aperture recorded in EXIF.</summary>
    public float ExifMaxAperture { get; }

    /// <summary>The lens manufacturer.</summary>
    public string LensMake { get; }

    /// <summary>The lens model name.</summary>
    public string LensModel { get; }

    /// <summary>The lens serial number.</summary>
    public string LensSerial { get; }

    /// <summary>The internal lens serial number.</summary>
    public string InternalLensSerial { get; }

    /// <summary>The focal length in 35&#160;mm-equivalent millimetres.</summary>
    public int FocalLengthIn35mmFormat { get; }

    /// <summary>Curated lens information from the file's maker notes.</summary>
    public RAWMakerNoteLensInfo MakerNotes { get; }

    /// <summary>Lens information from the DNG <c>LensInfo</c> tag.</summary>
    public RAWDNGLensInfo Dng { get; }

    /// <summary>
    /// Creates lens information from explicit values.
    /// </summary>
    /// <param name="minFocal">The minimum focal length.</param>
    /// <param name="maxFocal">The maximum focal length.</param>
    /// <param name="maxApertureAtMinFocal">The maximum aperture at the minimum focal length.</param>
    /// <param name="maxApertureAtMaxFocal">The maximum aperture at the maximum focal length.</param>
    /// <param name="exifMaxAperture">The maximum aperture recorded in EXIF.</param>
    /// <param name="lensMake">The lens manufacturer.</param>
    /// <param name="lensModel">The lens model name.</param>
    /// <param name="lensSerial">The lens serial number.</param>
    /// <param name="internalLensSerial">The internal lens serial number.</param>
    /// <param name="focalLengthIn35mmFormat">The 35&#160;mm-equivalent focal length.</param>
    /// <param name="makerNotes">The maker-note lens information.</param>
    /// <param name="dng">The DNG lens information.</param>
    public RAWLensInfo( float minFocal, float maxFocal, float maxApertureAtMinFocal, float maxApertureAtMaxFocal, float exifMaxAperture, string lensMake, string lensModel, string lensSerial, string internalLensSerial, int focalLengthIn35mmFormat, RAWMakerNoteLensInfo makerNotes, RAWDNGLensInfo dng )
    {
        this.MinFocal                = minFocal;
        this.MaxFocal                = maxFocal;
        this.MaxApertureAtMinFocal   = maxApertureAtMinFocal;
        this.MaxApertureAtMaxFocal   = maxApertureAtMaxFocal;
        this.ExifMaxAperture         = exifMaxAperture;
        this.LensMake                = lensMake;
        this.LensModel               = lensModel;
        this.LensSerial              = lensSerial;
        this.InternalLensSerial      = internalLensSerial;
        this.FocalLengthIn35mmFormat = focalLengthIn35mmFormat;
        this.MakerNotes              = makerNotes;
        this.Dng                     = dng;
    }

    /// <summary>
    /// Creates lens information from the marshaled LibRAW lens structures.
    /// </summary>
    /// <remarks>
    /// The EXIF-level fields come from <paramref name="lens"/>; the maker-note and DNG
    /// substructures are read from their own offsets in the context and composed here
    /// (see <see cref="LibRawMakerNotesLens"/> and <see cref="LibRawDngLens"/>).
    /// </remarks>
    /// <param name="lens">The marshaled leading fields of <c>libraw_lensinfo_t</c>.</param>
    /// <param name="makerNotes">The marshaled maker-note lens sub-structure.</param>
    /// <param name="dng">The marshaled DNG lens sub-structure.</param>
    internal RAWLensInfo( LibRawLensInfo lens, LibRawMakerNotesLens makerNotes, LibRawDngLens dng ) : this(
        lens.MinFocal,
        lens.MaxFocal,
        lens.MaxAp4MinFocal,
        lens.MaxAp4MaxFocal,
        lens.ExifMaxAp,
        lens.LensMake.DecodeCString(),
        lens.Lens.DecodeCString(),
        lens.LensSerial.DecodeCString(),
        lens.InternalLensSerial.DecodeCString(),
        lens.FocalLengthIn35mmFormat,
        new RAWMakerNoteLensInfo( makerNotes ),
        new RAWDNGLensInfo( dng )
    )
    {}

    /// <summary>
    /// Returns a compact summary of the lens: its name (when known) and focal range.
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
