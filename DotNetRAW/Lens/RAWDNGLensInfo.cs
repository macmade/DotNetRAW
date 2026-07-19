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
/// Lens information carried in a DNG file's metadata.
/// </summary>
/// <remarks>
/// Mirrors LibRAW's <c>libraw_dnglens_t</c>: the lens focal/aperture range recorded
/// in the DNG <c>LensInfo</c> tag.
/// </remarks>
public sealed record RAWDNGLensInfo
{
    /// <summary>The minimum focal length, in millimetres.</summary>
    public float MinFocal { get; }

    /// <summary>The maximum focal length, in millimetres.</summary>
    public float MaxFocal { get; }

    /// <summary>The maximum aperture (smallest f-number) at the minimum focal length.</summary>
    public float MaxApertureAtMinFocal { get; }

    /// <summary>The maximum aperture (smallest f-number) at the maximum focal length.</summary>
    public float MaxApertureAtMaxFocal { get; }

    /// <summary>
    /// Creates DNG lens information from explicit values.
    /// </summary>
    /// <param name="minFocal">The minimum focal length.</param>
    /// <param name="maxFocal">The maximum focal length.</param>
    /// <param name="maxApertureAtMinFocal">The maximum aperture at the minimum focal length.</param>
    /// <param name="maxApertureAtMaxFocal">The maximum aperture at the maximum focal length.</param>
    public RAWDNGLensInfo( float minFocal, float maxFocal, float maxApertureAtMinFocal, float maxApertureAtMaxFocal )
    {
        this.MinFocal              = minFocal;
        this.MaxFocal              = maxFocal;
        this.MaxApertureAtMinFocal = maxApertureAtMinFocal;
        this.MaxApertureAtMaxFocal = maxApertureAtMaxFocal;
    }

    /// <summary>
    /// Creates DNG lens information from a marshaled LibRAW DNG-lens structure.
    /// </summary>
    /// <param name="dng">The marshaled <c>libraw_dnglens_t</c> fields.</param>
    internal RAWDNGLensInfo( LibRawDngLens dng ) : this( dng.MinFocal, dng.MaxFocal, dng.MaxAp4MinFocal, dng.MaxAp4MaxFocal )
    {}

    /// <summary>
    /// Returns a compact summary of the focal range.
    /// </summary>
    /// <returns>
    /// A single focal length such as <c>"50mm"</c> when the range is fixed, or a range
    /// such as <c>"24–70mm"</c> (using an en dash) otherwise.
    /// </returns>
    public override string ToString()
    {
        if( this.MinFocal == this.MaxFocal )
        {
            return $"{ this.MinFocal.CompactDescription() }mm";
        }

        return $"{ this.MinFocal.CompactDescription() }–{ this.MaxFocal.CompactDescription() }mm";
    }
}
