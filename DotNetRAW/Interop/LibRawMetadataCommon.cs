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
/// The leading fields of LibRAW's <c>libraw_metadata_common_t</c>
/// (<c>makernotes.common</c>), covering the flash, temperature, colour-space and
/// firmware metadata the port reads.
/// </summary>
/// <remarks>
/// Modelled sequentially through <see cref="Firmware"/>; the trailing
/// <c>ExposureCalibrationShift</c>, AF data and count are unused and left off.
/// The interspersed temperature and EXIF fields the port does not surface are
/// modelled to preserve the layout.
/// </remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawMetadataCommon
{
    /// <summary>The flash exposure compensation, in EV (<c>FlashEC</c>).</summary>
    internal float FlashEC;

    /// <summary>The flash guide number (<c>FlashGN</c>).</summary>
    internal float FlashGN;

    /// <summary>The camera temperature (<c>CameraTemperature</c>).</summary>
    internal float CameraTemperature;

    /// <summary>The sensor temperature (<c>SensorTemperature</c>).</summary>
    internal float SensorTemperature;

    /// <summary>The secondary sensor temperature (<c>SensorTemperature2</c>). Unused.</summary>
    internal float SensorTemperature2;

    /// <summary>The lens temperature (<c>LensTemperature</c>).</summary>
    internal float LensTemperature;

    /// <summary>The ambient temperature (<c>AmbientTemperature</c>).</summary>
    internal float AmbientTemperature;

    /// <summary>The battery temperature (<c>BatteryTemperature</c>).</summary>
    internal float BatteryTemperature;

    /// <summary>The EXIF ambient temperature (<c>exifAmbientTemperature</c>). Unused.</summary>
    internal float ExifAmbientTemperature;

    /// <summary>The EXIF humidity (<c>exifHumidity</c>). Unused.</summary>
    internal float ExifHumidity;

    /// <summary>The EXIF pressure (<c>exifPressure</c>). Unused.</summary>
    internal float ExifPressure;

    /// <summary>The EXIF water depth (<c>exifWaterDepth</c>). Unused.</summary>
    internal float ExifWaterDepth;

    /// <summary>The EXIF acceleration (<c>exifAcceleration</c>). Unused.</summary>
    internal float ExifAcceleration;

    /// <summary>The EXIF camera elevation angle (<c>exifCameraElevationAngle</c>). Unused.</summary>
    internal float ExifCameraElevationAngle;

    /// <summary>The real (measured) ISO (<c>real_ISO</c>).</summary>
    internal float RealISO;

    /// <summary>The EXIF exposure index (<c>exifExposureIndex</c>).</summary>
    internal float ExifExposureIndex;

    /// <summary>The colour-space code (<c>ColorSpace</c>).</summary>
    internal ushort ColorSpace;

    /// <summary>The firmware string, NUL-terminated (<c>firmware</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 ) ]
    internal byte[] Firmware;
}
