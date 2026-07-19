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
/// LibRAW's <c>libraw_makernotes_lens_t</c> (<c>lens.makernotes</c>): the lens
/// identity decoded from a camera's maker-note block.
/// </summary>
/// <remarks>
/// Fully modelled sequentially so the marshaller computes each field offset - the
/// layout guard test then validates the transcription instead of echoing declared
/// offsets. The many fields the port does not surface (mount and camera codes,
/// per-focal apertures, and the teleconverter/adapter/attachment identities and
/// names) are present only to preserve the layout, including the 8-byte alignment
/// padding the <c>UINT64</c> fields introduce. <see cref="FocalType"/> is a signed
/// <c>short</c> using <c>-1</c> for unknown; <see cref="FocalLengthIn35mmFormat"/>
/// is a <c>float</c> here, unlike the <c>ushort</c> of the same name in the EXIF
/// lens structure.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawMakerNotesLens
{
    /// <summary>The LibRAW lens identifier (<c>LensID</c>).</summary>
    internal ulong LensID;

    /// <summary>The lens model, NUL-terminated (<c>Lens</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] Lens;

    /// <summary>The lens image-circle format code (<c>LensFormat</c>). Unused.</summary>
    internal ushort LensFormat;

    /// <summary>The lens mount code (<c>LensMount</c>). Unused.</summary>
    internal ushort LensMount;

    /// <summary>The camera identifier (<c>CamID</c>). Unused.</summary>
    internal ulong CamID;

    /// <summary>The camera sensor-format code (<c>CameraFormat</c>). Unused.</summary>
    internal ushort CameraFormat;

    /// <summary>The camera mount code (<c>CameraMount</c>). Unused.</summary>
    internal ushort CameraMount;

    /// <summary>The camera body descriptor (<c>body</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] Body;

    /// <summary>The focal-type code: <c>-1</c>/<c>0</c> unknown, <c>1</c> fixed, <c>2</c> zoom (<c>FocalType</c>).</summary>
    internal short FocalType;

    /// <summary>The prefix of the lens feature string (<c>LensFeatures_pre</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 ) ]
    internal byte[] LensFeaturesPre;

    /// <summary>The suffix of the lens feature string (<c>LensFeatures_suf</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 ) ]
    internal byte[] LensFeaturesSuf;

    /// <summary>The minimum focal length, in millimetres (<c>MinFocal</c>).</summary>
    internal float MinFocal;

    /// <summary>The maximum focal length, in millimetres (<c>MaxFocal</c>).</summary>
    internal float MaxFocal;

    /// <summary>The maximum aperture at the minimum focal length (<c>MaxAp4MinFocal</c>).</summary>
    internal float MaxAp4MinFocal;

    /// <summary>The maximum aperture at the maximum focal length (<c>MaxAp4MaxFocal</c>).</summary>
    internal float MaxAp4MaxFocal;

    /// <summary>The minimum aperture at the minimum focal length (<c>MinAp4MinFocal</c>). Unused.</summary>
    internal float MinAp4MinFocal;

    /// <summary>The minimum aperture at the maximum focal length (<c>MinAp4MaxFocal</c>). Unused.</summary>
    internal float MinAp4MaxFocal;

    /// <summary>The maximum aperture of the lens (<c>MaxAp</c>).</summary>
    internal float MaxAp;

    /// <summary>The minimum aperture of the lens (<c>MinAp</c>).</summary>
    internal float MinAp;

    /// <summary>The current focal length (<c>CurFocal</c>). Unused.</summary>
    internal float CurFocal;

    /// <summary>The current aperture (<c>CurAp</c>). Unused.</summary>
    internal float CurAp;

    /// <summary>The maximum aperture at the current focal length (<c>MaxAp4CurFocal</c>). Unused.</summary>
    internal float MaxAp4CurFocal;

    /// <summary>The minimum aperture at the current focal length (<c>MinAp4CurFocal</c>). Unused.</summary>
    internal float MinAp4CurFocal;

    /// <summary>The minimum focus distance (<c>MinFocusDistance</c>).</summary>
    internal float MinFocusDistance;

    /// <summary>The focus-range index (<c>FocusRangeIndex</c>). Unused.</summary>
    internal float FocusRangeIndex;

    /// <summary>The maximum aperture range, in f-stops (<c>LensFStops</c>).</summary>
    internal float LensFStops;

    /// <summary>The teleconverter identifier (<c>TeleconverterID</c>). Unused.</summary>
    internal ulong TeleconverterID;

    /// <summary>The teleconverter model (<c>Teleconverter</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] Teleconverter;

    /// <summary>The adapter identifier (<c>AdapterID</c>). Unused.</summary>
    internal ulong AdapterID;

    /// <summary>The adapter model (<c>Adapter</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] Adapter;

    /// <summary>The attachment identifier (<c>AttachmentID</c>). Unused.</summary>
    internal ulong AttachmentID;

    /// <summary>The attachment model (<c>Attachment</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] Attachment;

    /// <summary>The focal-length unit divisor (<c>FocalUnits</c>). Unused.</summary>
    internal ushort FocalUnits;

    /// <summary>The 35&#160;mm-equivalent focal length (<c>FocalLengthIn35mmFormat</c>).</summary>
    internal float FocalLengthIn35mmFormat;
}
