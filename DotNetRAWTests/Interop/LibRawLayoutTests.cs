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
using System.Runtime.InteropServices;
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Byte-layout guard tests for the LibRAW marshaling structs.
/// </summary>
/// <remarks>
/// The whole library reads its data through these structs, so a wrong field
/// offset would silently corrupt every value. The sizes and offsets asserted here
/// were taken from the vendored LibRAW 0.22.2 headers with
/// <c>offsetof</c>/<c>sizeof</c> and verified identical on arm64 and x86_64. Every
/// struct is modelled sequentially, so the marshaller computes the field offsets
/// and a mis-transcribed field type would shift them and fail the guard. The
/// offsets of the sub-structures within the context are additionally cross-checked
/// against the <em>loaded</em> native library in
/// <see cref="SubStructureOffsetsMatchTheLoadedLibrary"/>.
/// </remarks>
public class LibRawLayoutTests
{
    /// <summary>
    /// <see cref="LibRawImageSizes"/> places its fields at the native offsets,
    /// including the 4-byte gap the <c>double</c> alignment introduces.
    /// </summary>
    [ Fact ]
    public void ImageSizesLayout()
    {
        Assert.Equal(  0, ( int )Marshal.OffsetOf< LibRawImageSizes >( nameof( LibRawImageSizes.RawHeight ) ) );
        Assert.Equal(  2, ( int )Marshal.OffsetOf< LibRawImageSizes >( nameof( LibRawImageSizes.RawWidth ) ) );
        Assert.Equal( 16, ( int )Marshal.OffsetOf< LibRawImageSizes >( nameof( LibRawImageSizes.RawPitch ) ) );
        Assert.Equal( 24, ( int )Marshal.OffsetOf< LibRawImageSizes >( nameof( LibRawImageSizes.PixelAspect ) ) );
        Assert.Equal( 32, ( int )Marshal.OffsetOf< LibRawImageSizes >( nameof( LibRawImageSizes.Flip ) ) );
    }

    /// <summary>
    /// <see cref="LibRawIParams"/> places its identity, colour and CFA fields at
    /// the native offsets.
    /// </summary>
    [ Fact ]
    public void IParamsLayout()
    {
        Assert.Equal(   4, ( int )Marshal.OffsetOf< LibRawIParams >( nameof( LibRawIParams.Make ) ) );
        Assert.Equal( 260, ( int )Marshal.OffsetOf< LibRawIParams >( nameof( LibRawIParams.NormalizedModel ) ) );
        Assert.Equal( 340, ( int )Marshal.OffsetOf< LibRawIParams >( nameof( LibRawIParams.Colors ) ) );
        Assert.Equal( 344, ( int )Marshal.OffsetOf< LibRawIParams >( nameof( LibRawIParams.Filters ) ) );
        Assert.Equal( 348, ( int )Marshal.OffsetOf< LibRawIParams >( nameof( LibRawIParams.XTrans ) ) );
        Assert.Equal( 420, ( int )Marshal.OffsetOf< LibRawIParams >( nameof( LibRawIParams.Cdesc ) ) );
    }

    /// <summary>
    /// <see cref="LibRawLensInfo"/> places its EXIF lens fields at the native
    /// offsets.
    /// </summary>
    [ Fact ]
    public void LensInfoLayout()
    {
        Assert.Equal(  20, ( int )Marshal.OffsetOf< LibRawLensInfo >( nameof( LibRawLensInfo.LensMake ) ) );
        Assert.Equal( 148, ( int )Marshal.OffsetOf< LibRawLensInfo >( nameof( LibRawLensInfo.Lens ) ) );
        Assert.Equal( 404, ( int )Marshal.OffsetOf< LibRawLensInfo >( nameof( LibRawLensInfo.InternalLensSerial ) ) );
        Assert.Equal( 532, ( int )Marshal.OffsetOf< LibRawLensInfo >( nameof( LibRawLensInfo.FocalLengthIn35mmFormat ) ) );
    }

    /// <summary>
    /// The fully-modelled sequential structs are exactly the size of their native
    /// counterparts, with their key fields at the native offsets.
    /// </summary>
    [ Fact ]
    public void FullyModelledStructsMatchNativeSizes()
    {
        Assert.Equal(  16, Marshal.SizeOf< LibRawDngLens >() );
        Assert.Equal( 142, Marshal.SizeOf< LibRawShootingInfo >() );
        Assert.Equal(  48, Marshal.SizeOf< LibRawGpsInfo >() );

        Assert.Equal( 12, ( int )Marshal.OffsetOf< LibRawDngLens >( nameof( LibRawDngLens.MaxAp4MaxFocal ) ) );
        Assert.Equal( 14, ( int )Marshal.OffsetOf< LibRawShootingInfo >( nameof( LibRawShootingInfo.BodySerial ) ) );
        Assert.Equal( 36, ( int )Marshal.OffsetOf< LibRawGpsInfo >( nameof( LibRawGpsInfo.Altitude ) ) );
        Assert.Equal( 40, ( int )Marshal.OffsetOf< LibRawGpsInfo >( nameof( LibRawGpsInfo.AltRef ) ) );
        Assert.Equal( 44, ( int )Marshal.OffsetOf< LibRawGpsInfo >( nameof( LibRawGpsInfo.GpsParsed ) ) );
    }

    /// <summary>
    /// <see cref="LibRawMetadataCommon"/> places its flash, temperature,
    /// colour-space and firmware fields at the native offsets.
    /// </summary>
    [ Fact ]
    public void MetadataCommonLayout()
    {
        Assert.Equal( 56, ( int )Marshal.OffsetOf< LibRawMetadataCommon >( nameof( LibRawMetadataCommon.RealISO ) ) );
        Assert.Equal( 60, ( int )Marshal.OffsetOf< LibRawMetadataCommon >( nameof( LibRawMetadataCommon.ExifExposureIndex ) ) );
        Assert.Equal( 64, ( int )Marshal.OffsetOf< LibRawMetadataCommon >( nameof( LibRawMetadataCommon.ColorSpace ) ) );
        Assert.Equal( 66, ( int )Marshal.OffsetOf< LibRawMetadataCommon >( nameof( LibRawMetadataCommon.Firmware ) ) );
    }

    /// <summary>
    /// <see cref="LibRawMakerNotesLens"/> and <see cref="LibRawRawData"/> are the
    /// exact size of their native counterparts, with fields - including those the
    /// <c>UINT64</c> alignment padding shifts - at the native offsets. Both are
    /// fully modelled sequentially, so these offsets are computed by the marshaller
    /// and would break under a mis-transcribed field type.
    /// </summary>
    [ Fact ]
    public void MakerNotesLensAndRawDataLayout()
    {
        Assert.Equal( 736, Marshal.SizeOf< LibRawMakerNotesLens >() );
        Assert.Equal(  56, Marshal.SizeOf< LibRawRawData >() );

        Assert.Equal(   8, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.Lens ) ) );
        Assert.Equal( 144, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.CamID ) ) );
        Assert.Equal( 220, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.FocalType ) ) );
        Assert.Equal( 256, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.MinFocal ) ) );
        Assert.Equal( 280, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.MaxAp ) ) );
        Assert.Equal( 304, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.MinFocusDistance ) ) );
        Assert.Equal( 312, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.LensFStops ) ) );
        Assert.Equal( 320, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.TeleconverterID ) ) );
        Assert.Equal( 732, ( int )Marshal.OffsetOf< LibRawMakerNotesLens >( nameof( LibRawMakerNotesLens.FocalLengthIn35mmFormat ) ) );

        Assert.Equal(  8, ( int )Marshal.OffsetOf< LibRawRawData >( nameof( LibRawRawData.RawImage ) ) );
        Assert.Equal( 16, ( int )Marshal.OffsetOf< LibRawRawData >( nameof( LibRawRawData.Color4Image ) ) );
        Assert.Equal( 24, ( int )Marshal.OffsetOf< LibRawRawData >( nameof( LibRawRawData.Color3Image ) ) );
        Assert.Equal( 48, ( int )Marshal.OffsetOf< LibRawRawData >( nameof( LibRawRawData.Float4Image ) ) );
    }

    /// <summary>
    /// <see cref="LibRawImgOther"/> (with its nested GPS) and
    /// <see cref="LibRawColorData"/> place their fields at the native offsets,
    /// including the compact colour block that skips the 128&#160;KB curve.
    /// </summary>
    [ Fact ]
    public void ImgOtherAndColorDataLayout()
    {
        Assert.Equal(  16, ( int )Marshal.OffsetOf< LibRawImgOther >( nameof( LibRawImgOther.Timestamp ) ) );
        Assert.Equal(  24, ( int )Marshal.OffsetOf< LibRawImgOther >( nameof( LibRawImgOther.ShotOrder ) ) );
        Assert.Equal( 156, ( int )Marshal.OffsetOf< LibRawImgOther >( nameof( LibRawImgOther.ParsedGps ) ) );
        Assert.Equal( 204, ( int )Marshal.OffsetOf< LibRawImgOther >( nameof( LibRawImgOther.Description ) ) );
        Assert.Equal( 716, ( int )Marshal.OffsetOf< LibRawImgOther >( nameof( LibRawImgOther.Artist ) ) );

        Assert.Equal( 164, ( int )Marshal.OffsetOf< LibRawColorData >( nameof( LibRawColorData.CameraMultipliers ) ) );
        Assert.Equal( 196, ( int )Marshal.OffsetOf< LibRawColorData >( nameof( LibRawColorData.ColorMatrix ) ) );
        Assert.Equal( 292, ( int )Marshal.OffsetOf< LibRawColorData >( nameof( LibRawColorData.RgbCamera ) ) );
        Assert.Equal( 340, ( int )Marshal.OffsetOf< LibRawColorData >( nameof( LibRawColorData.CameraXyz ) ) );
        Assert.Equal( 784, ( int )Marshal.OffsetOf< LibRawColorData >( nameof( LibRawColorData.ProfileLength ) ) );
    }

    /// <summary>
    /// The sub-structure offsets the port reads from the context match the
    /// <em>loaded</em> native library: the three getter interior-pointers land at
    /// exactly the declared offsets, proving the layout the port assumes is the
    /// layout the bundled binary actually uses.
    /// </summary>
    [ Fact ]
    public void SubStructureOffsetsMatchTheLoadedLibrary()
    {
        using LibRawHandle handle = LibRawHandle.Initialize();

        Assert.False( handle.IsInvalid );

        IntPtr context = handle.DangerousGetHandle();

        long iparams = LibRaw.libraw_get_iparams( context ).ToInt64() - context.ToInt64();
        long lens    = LibRaw.libraw_get_lensinfo( context ).ToInt64() - context.ToInt64();
        long other   = LibRaw.libraw_get_imgother( context ).ToInt64() - context.ToInt64();

        Assert.Equal( LibRawData.IParamsOffset,  iparams );
        Assert.Equal( LibRawData.LensInfoOffset, lens );
        Assert.Equal( LibRawData.ImgOtherOffset, other );
    }
}
