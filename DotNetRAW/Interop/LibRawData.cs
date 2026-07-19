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
/// The byte offsets, within LibRAW's <c>libraw_data_t</c> context, of the
/// sub-structures the port reads.
/// </summary>
/// <remarks>
/// The context is the block that <c>libraw_init</c> returns and the accessors
/// read from. Rather than modelling the whole 372&#160;KB structure - most of
/// which is unused - the port reads each needed sub-structure directly from the
/// base pointer at the offset given here, then marshals it into the matching
/// flat struct.
/// <para>
/// The offsets are taken from the vendored LibRAW 0.22.2 headers via
/// <c>offsetof</c>/<c>sizeof</c> and are identical on every supported 64-bit
/// target: the sub-structures contain no <c>long</c> (whose width differs between
/// Windows LLP64 and Unix LP64) and a 64-bit <c>time_t</c>, so their layout does
/// not vary by platform. The three offsets that a getter also exposes
/// (<see cref="IParamsOffset"/>, <see cref="LensInfoOffset"/>,
/// <see cref="ImgOtherOffset"/>) are additionally cross-checked at run time
/// against the loaded library.
/// </para>
/// </remarks>
internal static class LibRawData
{
    /// <summary>The total size of <c>libraw_data_t</c>, in bytes.</summary>
    internal const int Size = 381576;

    /// <summary>Offset of the image-geometry sub-structure (<c>sizes</c>).</summary>
    internal const int SizesOffset = 8;

    /// <summary>Offset of the image-parameters sub-structure (<c>idata</c>).</summary>
    internal const int IParamsOffset = 192;

    /// <summary>Offset of the lens-information sub-structure (<c>lens</c>).</summary>
    internal const int LensInfoOffset = 632;

    /// <summary>Offset of the DNG lens sub-structure (<c>lens.dng</c>).</summary>
    internal const int DngLensOffset = LensInfoOffset + 544;

    /// <summary>Offset of the maker-note lens sub-structure (<c>lens.makernotes</c>).</summary>
    internal const int MakerNotesLensOffset = LensInfoOffset + 560;

    /// <summary>Offset of the common maker-note metadata (<c>makernotes.common</c>).</summary>
    internal const int MetadataCommonOffset = 4784;

    /// <summary>Offset of the shooting-information sub-structure (<c>shootinginfo</c>).</summary>
    internal const int ShootingInfoOffset = 5088;

    /// <summary>Offset of the colour-data sub-structure (<c>color</c>).</summary>
    internal const int ColorOffset = 5592;

    /// <summary>
    /// Offset, <em>relative to <see cref="ColorOffset"/></em>, of the per-channel
    /// black-level array (<c>cblack</c>); the port reads its first four elements.
    /// Add <see cref="ColorOffset"/> to reach it from the context base.
    /// </summary>
    internal const int ColorCblackRelativeOffset = 131072;

    /// <summary>
    /// Offset, <em>relative to <see cref="ColorOffset"/></em>, of the <c>black</c>
    /// field - the start of the compact block of colour scalars and matrices the
    /// port reads (see <see cref="LibRawColorData"/>), skipping the preceding
    /// 128&#160;KB curve. Add <see cref="ColorOffset"/> to reach it from the
    /// context base.
    /// </summary>
    internal const int ColorBodyRelativeOffset = 147488;

    /// <summary>
    /// Offset of the other-metadata sub-structure (<c>other</c>); its parsed-GPS
    /// data is read through the nested <see cref="LibRawImgOther.ParsedGps"/>.
    /// </summary>
    internal const int ImgOtherOffset = 192680;

    /// <summary>Offset of the unpacked-sensor-data sub-structure (<c>rawdata</c>).</summary>
    internal const int RawDataOffset = 193768;
}
