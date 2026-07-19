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
/// Camera-common metadata drawn from a file's maker notes.
/// </summary>
/// <remarks>
/// Mirrors the cross-vendor fields of LibRAW's <c>libraw_metadata_common_t</c>: flash
/// output, environmental temperatures, colour space, firmware, and sensitivity.
/// Per-vendor maker-note structures and camera-specific autofocus data blobs are
/// intentionally out of scope.
/// </remarks>
public sealed record RAWCameraMetadata
{
    /// <summary>The flash exposure compensation applied, in stops.</summary>
    public float FlashExposureCompensation { get; }

    /// <summary>The flash guide number.</summary>
    public float FlashGuideNumber { get; }

    /// <summary>The camera body temperature, in degrees Celsius.</summary>
    public float CameraTemperature { get; }

    /// <summary>The sensor temperature, in degrees Celsius.</summary>
    public float SensorTemperature { get; }

    /// <summary>The lens temperature, in degrees Celsius.</summary>
    public float LensTemperature { get; }

    /// <summary>The ambient temperature, in degrees Celsius.</summary>
    public float AmbientTemperature { get; }

    /// <summary>The battery temperature, in degrees Celsius.</summary>
    public float BatteryTemperature { get; }

    /// <summary>The colour-space code recorded by the camera.</summary>
    public int ColorSpace { get; }

    /// <summary>The camera firmware version string.</summary>
    public string Firmware { get; }

    /// <summary>The real (measured) ISO sensitivity.</summary>
    public float RealISO { get; }

    /// <summary>The EXIF exposure index.</summary>
    public float ExposureIndex { get; }

    /// <summary>
    /// Creates camera metadata from explicit values.
    /// </summary>
    /// <param name="flashExposureCompensation">The flash exposure compensation, in stops.</param>
    /// <param name="flashGuideNumber">The flash guide number.</param>
    /// <param name="cameraTemperature">The camera body temperature.</param>
    /// <param name="sensorTemperature">The sensor temperature.</param>
    /// <param name="lensTemperature">The lens temperature.</param>
    /// <param name="ambientTemperature">The ambient temperature.</param>
    /// <param name="batteryTemperature">The battery temperature.</param>
    /// <param name="colorSpace">The colour-space code.</param>
    /// <param name="firmware">The firmware version string.</param>
    /// <param name="realISO">The real ISO sensitivity.</param>
    /// <param name="exposureIndex">The EXIF exposure index.</param>
    public RAWCameraMetadata( float flashExposureCompensation, float flashGuideNumber, float cameraTemperature, float sensorTemperature, float lensTemperature, float ambientTemperature, float batteryTemperature, int colorSpace, string firmware, float realISO, float exposureIndex )
    {
        this.FlashExposureCompensation = flashExposureCompensation;
        this.FlashGuideNumber          = flashGuideNumber;
        this.CameraTemperature         = cameraTemperature;
        this.SensorTemperature         = sensorTemperature;
        this.LensTemperature           = lensTemperature;
        this.AmbientTemperature        = ambientTemperature;
        this.BatteryTemperature        = batteryTemperature;
        this.ColorSpace                = colorSpace;
        this.Firmware                  = firmware;
        this.RealISO                   = realISO;
        this.ExposureIndex             = exposureIndex;
    }

    /// <summary>
    /// Creates camera metadata from a marshaled LibRAW common-metadata structure.
    /// </summary>
    /// <param name="common">The marshaled <c>libraw_metadata_common_t</c> fields.</param>
    internal RAWCameraMetadata( LibRawMetadataCommon common ) : this(
        common.FlashEC,
        common.FlashGN,
        common.CameraTemperature,
        common.SensorTemperature,
        common.LensTemperature,
        common.AmbientTemperature,
        common.BatteryTemperature,
        common.ColorSpace,
        common.Firmware.DecodeCString(),
        common.RealISO,
        common.ExifExposureIndex
    )
    {}

    /// <summary>
    /// Returns a compact summary of the firmware and colour space.
    /// </summary>
    /// <returns>
    /// A description of the form <c>"firmware 1.3.3, color space 1"</c>, using
    /// <c>"unknown firmware"</c> when no firmware string is recorded.
    /// </returns>
    public override string ToString()
    {
        string firmware = this.Firmware.Length == 0 ? "unknown firmware" : $"firmware { this.Firmware }";

        return string.Create( CultureInfo.InvariantCulture, $"{ firmware }, color space { this.ColorSpace }" );
    }
}
