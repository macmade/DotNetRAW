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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetRAW;

/// <summary>
/// Colour calibration data used to interpret a RAW file's samples.
/// </summary>
/// <remarks>
/// Mirrors the commonly used fields of LibRAW's <c>libraw_colordata_t</c>: the black
/// and saturation levels that bound the sample range, the white-balance multipliers,
/// and the colour-conversion matrices. The full per-block black map, tone curve, and
/// per-vendor colour tables are beyond this curated scope.
/// </remarks>
public sealed record RAWColorData
{
    /// <summary>The global black level subtracted from raw samples.</summary>
    public int BlackLevel { get; }

    /// <summary>The per-channel black-level adjustments (up to four channels).</summary>
    public int[] ChannelBlackLevels { get; }

    /// <summary>The saturation (white) level of the raw samples.</summary>
    public int Maximum { get; }

    /// <summary>The maximum sample value actually observed in the data.</summary>
    public int DataMaximum { get; }

    /// <summary>The camera white-balance multipliers (<c>cam_mul</c>), one per channel.</summary>
    public float[] CameraMultipliers { get; }

    /// <summary>The pre-multipliers (<c>pre_mul</c>) LibRAW would apply, one per channel.</summary>
    public float[] PreMultipliers { get; }

    /// <summary>The camera-to-sRGB conversion matrix (<c>rgb_cam</c>), 3&#215;4.</summary>
    public float[][] RgbCamera { get; }

    /// <summary>The camera-to-XYZ conversion matrix (<c>cam_xyz</c>), 4&#215;3.</summary>
    public float[][] CameraXyz { get; }

    /// <summary>The embedded colour matrix (<c>cmatrix</c>), 3&#215;4.</summary>
    public float[][] ColorMatrix { get; }

    /// <summary>Whether the file embeds an ICC colour profile.</summary>
    public bool HasEmbeddedColorProfile { get; }

    /// <summary>The length in bytes of the embedded colour profile, or <c>0</c> if none.</summary>
    public int ColorProfileLength { get; }

    /// <summary>
    /// Creates colour data from explicit values.
    /// </summary>
    /// <param name="blackLevel">The global black level.</param>
    /// <param name="channelBlackLevels">The per-channel black-level adjustments.</param>
    /// <param name="maximum">The saturation level.</param>
    /// <param name="dataMaximum">The maximum observed sample value.</param>
    /// <param name="cameraMultipliers">The camera white-balance multipliers.</param>
    /// <param name="preMultipliers">The pre-multipliers.</param>
    /// <param name="rgbCamera">The camera-to-sRGB matrix.</param>
    /// <param name="cameraXyz">The camera-to-XYZ matrix.</param>
    /// <param name="colorMatrix">The embedded colour matrix.</param>
    /// <param name="hasEmbeddedColorProfile">Whether an ICC profile is embedded.</param>
    /// <param name="colorProfileLength">The embedded profile length in bytes.</param>
    public RAWColorData( int blackLevel, int[] channelBlackLevels, int maximum, int dataMaximum, float[] cameraMultipliers, float[] preMultipliers, float[][] rgbCamera, float[][] cameraXyz, float[][] colorMatrix, bool hasEmbeddedColorProfile, int colorProfileLength )
    {
        this.BlackLevel              = blackLevel;
        this.ChannelBlackLevels      = channelBlackLevels;
        this.Maximum                 = maximum;
        this.DataMaximum             = dataMaximum;
        this.CameraMultipliers       = cameraMultipliers;
        this.PreMultipliers          = preMultipliers;
        this.RgbCamera               = rgbCamera;
        this.CameraXyz               = cameraXyz;
        this.ColorMatrix             = colorMatrix;
        this.HasEmbeddedColorProfile = hasEmbeddedColorProfile;
        this.ColorProfileLength      = colorProfileLength;
    }

    /// <summary>
    /// Creates colour data from a marshaled LibRAW colour structure and its
    /// per-channel black levels.
    /// </summary>
    /// <remarks>
    /// The four <c>cblack</c> values are read separately from the context (the
    /// interop <c>color</c> struct starts after them) and passed in.
    /// </remarks>
    /// <param name="color">The marshaled <c>libraw_colordata_t</c> fields.</param>
    /// <param name="channelBlackLevels">The first four <c>cblack</c> values.</param>
    internal RAWColorData( LibRawColorData color, uint[] channelBlackLevels ) : this(
        ( int )color.Black,
        channelBlackLevels.Select( level => ( int )level ).ToArray(),
        ( int )color.Maximum,
        ( int )color.DataMaximum,
        CFixedArray.Values( color.CameraMultipliers ),
        CFixedArray.Values( color.PreMultipliers ),
        CFixedArray.Matrix( color.RgbCamera, 3, 4 ),
        CFixedArray.Matrix( color.CameraXyz, 4, 3 ),
        CFixedArray.Matrix( color.ColorMatrix, 3, 4 ),
        color.Profile != IntPtr.Zero && color.ProfileLength > 0,
        ( int )color.ProfileLength
    )
    {}

    /// <summary>
    /// Returns a compact summary of the black and saturation levels.
    /// </summary>
    /// <returns>
    /// A description of the form <c>"black N, max N"</c>, with <c>", ICC profile"</c>
    /// appended when a profile is embedded.
    /// </returns>
    public override string ToString()
    {
        string profile = this.HasEmbeddedColorProfile ? ", ICC profile" : "";

        return string.Create( CultureInfo.InvariantCulture, $"black { this.BlackLevel }, max { this.Maximum }{ profile }" );
    }

    /// <summary>
    /// Compares this colour data with another for value equality, comparing the
    /// arrays and matrices structurally rather than by reference.
    /// </summary>
    /// <param name="other">The colour data to compare with.</param>
    /// <returns>
    /// <see langword="true"/> when both have the same scalar levels, profile state
    /// and array/matrix contents.
    /// </returns>
    public bool Equals( RAWColorData? other )
    {
        if( other is null )
        {
            return false;
        }

        if( ReferenceEquals( this, other ) )
        {
            return true;
        }

        return this.BlackLevel == other.BlackLevel
            && this.Maximum == other.Maximum
            && this.DataMaximum == other.DataMaximum
            && this.HasEmbeddedColorProfile == other.HasEmbeddedColorProfile
            && this.ColorProfileLength == other.ColorProfileLength
            && this.ChannelBlackLevels.SequenceEqual( other.ChannelBlackLevels )
            && this.CameraMultipliers.SequenceEqual( other.CameraMultipliers )
            && this.PreMultipliers.SequenceEqual( other.PreMultipliers )
            && MatricesEqual( this.RgbCamera, other.RgbCamera )
            && MatricesEqual( this.CameraXyz, other.CameraXyz )
            && MatricesEqual( this.ColorMatrix, other.ColorMatrix );
    }

    /// <summary>
    /// Returns a hash code consistent with <see cref="Equals(RAWColorData)"/>,
    /// folding in the array and matrix contents.
    /// </summary>
    /// <returns>The structural hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new HashCode();

        hash.Add( this.BlackLevel );
        hash.Add( this.Maximum );
        hash.Add( this.DataMaximum );
        hash.Add( this.HasEmbeddedColorProfile );
        hash.Add( this.ColorProfileLength );

        AddRange( ref hash, this.ChannelBlackLevels );
        AddRange( ref hash, this.CameraMultipliers );
        AddRange( ref hash, this.PreMultipliers );
        AddRange( ref hash, this.RgbCamera.SelectMany( row => row ) );
        AddRange( ref hash, this.CameraXyz.SelectMany( row => row ) );
        AddRange( ref hash, this.ColorMatrix.SelectMany( row => row ) );

        return hash.ToHashCode();
    }

    /// <summary>
    /// Compares two row-major matrices for structural equality.
    /// </summary>
    /// <param name="first">The first matrix.</param>
    /// <param name="second">The second matrix.</param>
    /// <returns>
    /// <see langword="true"/> when both have identical shape and contents.
    /// </returns>
    private static bool MatricesEqual( float[][] first, float[][] second )
    {
        if( ReferenceEquals( first, second ) )
        {
            return true;
        }

        if( first.Length != second.Length )
        {
            return false;
        }

        return first.Zip( second, ( a, b ) => a.SequenceEqual( b ) ).All( equal => equal );
    }

    /// <summary>
    /// Folds every element of a sequence into a hash code.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="hash">The hash code being built.</param>
    /// <param name="values">The values to fold in.</param>
    private static void AddRange< T >( ref HashCode hash, IEnumerable< T > values )
    {
        foreach( T value in values )
        {
            hash.Add( value );
        }
    }
}
