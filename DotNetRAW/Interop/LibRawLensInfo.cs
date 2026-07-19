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

using System.Runtime.InteropServices;

// Interop struct fields are populated by the marshaller from native memory, not
// assigned in C# code.
#pragma warning disable CS0649

namespace DotNetRAW;

/// <summary>
/// The leading fields of LibRAW's <c>libraw_lensinfo_t</c> (<c>lens</c>),
/// covering the EXIF-level lens identity the port reads.
/// </summary>
/// <remarks>
/// Modelled sequentially through <see cref="FocalLengthIn35mmFormat"/>. The
/// nested <c>nikon</c>, <c>dng</c> and <c>makernotes</c> sub-structures follow in
/// the native layout but are read separately from their own offsets (see
/// <see cref="LibRawDngLens"/> and <see cref="LibRawMakerNotesLens"/>), so they
/// are left off here. Note that <see cref="FocalLengthIn35mmFormat"/> is a
/// <c>ushort</c> here, unlike the <c>float</c> of the same name in the maker-note
/// lens structure.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawLensInfo
{
    /// <summary>The minimum focal length, in millimetres (<c>MinFocal</c>).</summary>
    internal float MinFocal;

    /// <summary>The maximum focal length, in millimetres (<c>MaxFocal</c>).</summary>
    internal float MaxFocal;

    /// <summary>The maximum aperture at the minimum focal length (<c>MaxAp4MinFocal</c>).</summary>
    internal float MaxAp4MinFocal;

    /// <summary>The maximum aperture at the maximum focal length (<c>MaxAp4MaxFocal</c>).</summary>
    internal float MaxAp4MaxFocal;

    /// <summary>The maximum aperture recorded in EXIF (<c>EXIF_MaxAp</c>).</summary>
    internal float ExifMaxAp;

    /// <summary>The lens manufacturer, NUL-terminated (<c>LensMake</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] LensMake;

    /// <summary>The lens model, NUL-terminated (<c>Lens</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] Lens;

    /// <summary>The lens serial number, NUL-terminated (<c>LensSerial</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] LensSerial;

    /// <summary>The internal lens serial number, NUL-terminated (<c>InternalLensSerial</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] InternalLensSerial;

    /// <summary>The 35&#160;mm-equivalent focal length (<c>FocalLengthIn35mmFormat</c>); non-negative.</summary>
    internal ushort FocalLengthIn35mmFormat;
}
