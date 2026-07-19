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
using System.Globalization;
using System.Linq;

namespace DotNetRAW;

/// <summary>
/// Exposure and shot metadata for a RAW capture.
/// </summary>
/// <remarks>
/// Mirrors the commonly used fields of LibRAW's <c>libraw_imgother_t</c>, plus the
/// camera body serial from <c>libraw_shootinginfo_t</c>. Camera-specific shooting-mode
/// codes are intentionally left out of this curated, cross-vendor subset.
/// </remarks>
public sealed record RAWShotInfo
{
    /// <summary>
    /// The magnitude at or above which a rounded shutter reciprocal is rendered with
    /// the raw shutter value instead, guarding the integer conversion.
    /// </summary>
    private const float ReciprocalShutterLimit = 1e9f;

    /// <summary>
    /// The smallest <c>time_t</c> (Unix epoch seconds) that maps to a representable
    /// <see cref="DateTimeOffset"/>; smaller values are treated as no timestamp.
    /// </summary>
    private static readonly long MinTimestampSeconds = DateTimeOffset.MinValue.ToUnixTimeSeconds();

    /// <summary>
    /// The largest <c>time_t</c> (Unix epoch seconds) that maps to a representable
    /// <see cref="DateTimeOffset"/>; larger values are treated as no timestamp.
    /// </summary>
    private static readonly long MaxTimestampSeconds = DateTimeOffset.MaxValue.ToUnixTimeSeconds();

    /// <summary>The ISO sensitivity.</summary>
    public float IsoSpeed { get; }

    /// <summary>The shutter speed, in seconds.</summary>
    public float ShutterSpeed { get; }

    /// <summary>The aperture, as an f-number.</summary>
    public float Aperture { get; }

    /// <summary>The focal length, in millimetres.</summary>
    public float FocalLength { get; }

    /// <summary>The capture timestamp, or <see langword="null"/> if the file records none.</summary>
    public DateTimeOffset? Timestamp { get; }

    /// <summary>The position of this shot in the capture sequence.</summary>
    public long ShotOrder { get; }

    /// <summary>The image description recorded in the file.</summary>
    public string ImageDescription { get; }

    /// <summary>The artist/author recorded in the file.</summary>
    public string Artist { get; }

    /// <summary>The camera body serial number, if recorded.</summary>
    public string BodySerial { get; }

    /// <summary>
    /// Creates shot information from explicit values.
    /// </summary>
    /// <param name="isoSpeed">The ISO sensitivity.</param>
    /// <param name="shutterSpeed">The shutter speed, in seconds.</param>
    /// <param name="aperture">The aperture, as an f-number.</param>
    /// <param name="focalLength">The focal length, in millimetres.</param>
    /// <param name="timestamp">The capture timestamp.</param>
    /// <param name="shotOrder">The position in the capture sequence.</param>
    /// <param name="imageDescription">The image description.</param>
    /// <param name="artist">The artist/author.</param>
    /// <param name="bodySerial">The camera body serial number.</param>
    public RAWShotInfo( float isoSpeed, float shutterSpeed, float aperture, float focalLength, DateTimeOffset? timestamp, long shotOrder, string imageDescription, string artist, string bodySerial )
    {
        this.IsoSpeed         = isoSpeed;
        this.ShutterSpeed     = shutterSpeed;
        this.Aperture         = aperture;
        this.FocalLength      = focalLength;
        this.Timestamp        = timestamp;
        this.ShotOrder        = shotOrder;
        this.ImageDescription = imageDescription;
        this.Artist           = artist;
        this.BodySerial       = bodySerial;
    }

    /// <summary>
    /// Creates shot information from the marshaled LibRAW structures.
    /// </summary>
    /// <remarks>
    /// The exposure, timestamp and descriptive fields come from <paramref name="other"/>;
    /// the body serial from <paramref name="shooting"/>. A zero or out-of-range timestamp
    /// resolves to no capture time (see <see cref="TimestampFromUnixSeconds"/>).
    /// </remarks>
    /// <param name="other">The marshaled <c>libraw_imgother_t</c> fields.</param>
    /// <param name="shooting">The marshaled <c>libraw_shootinginfo_t</c> fields.</param>
    internal RAWShotInfo( LibRawImgOther other, LibRawShootingInfo shooting ) : this(
        other.IsoSpeed,
        other.Shutter,
        other.Aperture,
        other.FocalLength,
        TimestampFromUnixSeconds( other.Timestamp ),
        other.ShotOrder,
        other.Description.DecodeCString(),
        other.Artist.DecodeCString(),
        shooting.BodySerial.DecodeCString()
    )
    {}

    /// <summary>
    /// Returns a compact summary of the exposure settings.
    /// </summary>
    /// <returns>
    /// A comma-separated summary of the ISO, shutter, aperture and focal length that a
    /// file records (e.g. <c>"ISO 100, 1/250s, f/2.8, 50mm"</c>), or
    /// <c>"no exposure data"</c> when none are recorded.
    /// </returns>
    public override string ToString()
    {
        string[] parts =
        [
            this.IsoSpeed    > 0 ? $"ISO { this.IsoSpeed.CompactDescription() }" : "",
            this.ShutterDescription(),
            this.Aperture    > 0 ? $"f/{ this.Aperture.CompactDescription() }"   : "",
            this.FocalLength > 0 ? $"{ this.FocalLength.CompactDescription() }mm" : "",
        ];

        string summary = string.Join( ", ", parts.Where( part => part.Length > 0 ) );

        return summary.Length == 0 ? "no exposure data" : summary;
    }

    /// <summary>
    /// Renders the shutter speed: a rounded reciprocal fraction for sub-second speeds
    /// (e.g. <c>"1/250s"</c>), whole seconds otherwise, or an empty string when the
    /// file records no shutter speed.
    /// </summary>
    /// <returns>The shutter-speed fragment.</returns>
    private string ShutterDescription()
    {
        if( this.ShutterSpeed > 0 && this.ShutterSpeed < 1 )
        {
            float denominator = MathF.Round( 1.0f / this.ShutterSpeed, MidpointRounding.AwayFromZero );

            if( float.IsFinite( denominator ) && denominator < ReciprocalShutterLimit )
            {
                return string.Create( CultureInfo.InvariantCulture, $"1/{ ( int )denominator }s" );
            }

            return $"{ this.ShutterSpeed.CompactDescription() }s";
        }

        if( this.ShutterSpeed > 0 )
        {
            return $"{ this.ShutterSpeed.CompactDescription() }s";
        }

        return "";
    }

    /// <summary>
    /// Converts a native <c>time_t</c> to a capture timestamp.
    /// </summary>
    /// <remarks>
    /// A zero value means the file records no capture time, and a value outside the
    /// representable <see cref="DateTimeOffset"/> range is likewise treated as none
    /// rather than throwing, so a corrupt timestamp in a malformed file cannot fault
    /// the read.
    /// </remarks>
    /// <param name="seconds">The capture time as seconds since the Unix epoch.</param>
    /// <returns>
    /// The capture timestamp, or <see langword="null"/> when absent or unrepresentable.
    /// </returns>
    private static DateTimeOffset? TimestampFromUnixSeconds( long seconds )
    {
        if( seconds == 0 || seconds < MinTimestampSeconds || seconds > MaxTimestampSeconds )
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds( seconds );
    }
}
