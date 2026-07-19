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

// Interop struct fields are populated by the marshaller from native memory, not
// assigned in C# code.
#pragma warning disable CS0649

namespace DotNetRAW;

/// <summary>
/// The colour-calibration fields of LibRAW's <c>libraw_colordata_t</c>
/// (<c>color</c>) that the port reads, from the <c>black</c> field onward.
/// </summary>
/// <remarks>
/// LibRAW's <c>color</c> is roughly 183&#160;KB, dominated by a leading 128&#160;KB
/// tone curve and a 16&#160;KB <c>cblack</c> array the port does not need. To avoid
/// modelling and copying all of it, this struct starts at <c>black</c> - read from
/// an offset that skips the curve and <c>cblack</c> (see
/// <see cref="LibRawData.ColorBodyRelativeOffset"/>) - and models sequentially
/// through <see cref="ProfileLength"/>. The four <c>cblack</c> values the port
/// surfaces are read separately from <see cref="LibRawData.ColorCblackRelativeOffset"/>.
/// The interspersed
/// fields the port does not use are modelled to preserve the layout. The matrices
/// are flat row-major buffers reshaped by the caller. A sequential layout is used
/// so the object-reference (array) fields, which sit at 4-byte offsets, are placed
/// by the runtime rather than rejected as they would be under an explicit layout.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawColorData
{
    /// <summary>The global black level (<c>black</c>).</summary>
    internal uint Black;

    /// <summary>The maximum value seen in the data (<c>data_maximum</c>).</summary>
    internal uint DataMaximum;

    /// <summary>The white (saturation) level (<c>maximum</c>).</summary>
    internal uint Maximum;

    /// <summary>The per-channel linear maxima (<c>linear_max</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 ) ]
    internal uint[] LinearMax;

    /// <summary>The floating-point maximum (<c>fmaximum</c>). Unused.</summary>
    internal float FMaximum;

    /// <summary>The floating-point normalization factor (<c>fnorm</c>). Unused.</summary>
    internal float FNorm;

    /// <summary>The 8&#215;8 white-level grid (<c>white</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal ushort[] White;

    /// <summary>The as-shot white-balance multipliers, per channel (<c>cam_mul</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 ) ]
    internal float[] CameraMultipliers;

    /// <summary>The pre-multipliers, per channel (<c>pre_mul</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 ) ]
    internal float[] PreMultipliers;

    /// <summary>The 3&#215;4 colour matrix, row-major (<c>cmatrix</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 12 ) ]
    internal float[] ColorMatrix;

    /// <summary>The 3&#215;4 secondary colour matrix, row-major (<c>ccm</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 12 ) ]
    internal float[] Ccm;

    /// <summary>The 3&#215;4 camera-to-RGB matrix, row-major (<c>rgb_cam</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 12 ) ]
    internal float[] RgbCamera;

    /// <summary>The 4&#215;3 camera-to-XYZ matrix, row-major (<c>cam_xyz</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 12 ) ]
    internal float[] CameraXyz;

    /// <summary>The Phase One black-level data (<c>phase_one_data</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 9 ) ]
    internal int[] PhaseOneData;

    /// <summary>Whether the flash fired (<c>flash_used</c>). Unused.</summary>
    internal float FlashUsed;

    /// <summary>The Canon exposure value (<c>canon_ev</c>). Unused.</summary>
    internal float CanonEv;

    /// <summary>A secondary model string (<c>model2</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] Model2;

    /// <summary>The DNG unique camera model (<c>UniqueCameraModel</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] UniqueCameraModel;

    /// <summary>The DNG localized camera model (<c>LocalizedCameraModel</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] LocalizedCameraModel;

    /// <summary>The DNG image unique ID (<c>ImageUniqueID</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] ImageUniqueID;

    /// <summary>The raw-data unique ID (<c>RawDataUniqueID</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 17 ) ]
    internal byte[] RawDataUniqueID;

    /// <summary>The original raw file name (<c>OriginalRawFileName</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] OriginalRawFileName;

    /// <summary>A pointer to the embedded ICC profile, or null (<c>profile</c>).</summary>
    internal IntPtr Profile;

    /// <summary>The embedded ICC profile length, in bytes (<c>profile_length</c>).</summary>
    internal uint ProfileLength;
}
