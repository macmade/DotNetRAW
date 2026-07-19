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
using System.Linq;

namespace DotNetRAW;

/// <summary>
/// The colour-filter-array (CFA) layout of a RAW sensor.
/// </summary>
/// <remarks>
/// Describes how colours are arranged over the sensor's pixels and provides a
/// per-pixel colour lookup equivalent to LibRAW's <c>COLOR(row, col)</c> for the
/// two common layouts: a repeating 2×2 Bayer mosaic and Fuji's 6×6 X-Trans mosaic.
/// Sensors without a CFA (Foveon, monochrome, or already-demosaiced input) are
/// reported as <see cref="Kind.None"/>.
/// </remarks>
public sealed record RAWCFAPattern
{
    /// <summary>
    /// The kind of colour-filter array.
    /// </summary>
    public enum Kind
    {
        /// <summary>No colour-filter array (Foveon, monochrome, or full-colour data).</summary>
        None,

        /// <summary>A repeating 2×2 Bayer mosaic.</summary>
        Bayer,

        /// <summary>Fuji's 6×6 X-Trans mosaic.</summary>
        XTrans,
    }

    /// <summary>
    /// The special colour index LibRAW returns for sensors without a CFA, meaning
    /// "all channels" (<c>0 + 1 + 2 + 3</c>).
    /// </summary>
    public static int FullColorIndex => 6;

    /// <summary>The kind of colour-filter array.</summary>
    public Kind PatternKind { get; }

    /// <summary>The packed CFA pattern value, as encoded by LibRAW (<c>filters</c>).</summary>
    public uint Filters { get; }

    /// <summary>
    /// The colour descriptor mapping colour indices to channel letters (e.g. <c>"RGBG"</c>).
    /// </summary>
    public string ColorDescription { get; }

    /// <summary>
    /// The 6×6 X-Trans colour-index grid, present only when <see cref="PatternKind"/>
    /// is <see cref="Kind.XTrans"/>.
    /// </summary>
    public int[][]? XTransPattern { get; }

    /// <summary>
    /// Creates a CFA pattern from explicit values.
    /// </summary>
    /// <remarks>
    /// The <see cref="PatternKind"/> is derived from <paramref name="filters"/>:
    /// <c>0</c> is <see cref="Kind.None"/>, <c>9</c> is <see cref="Kind.XTrans"/>,
    /// and any other value is treated as <see cref="Kind.Bayer"/>.
    /// </remarks>
    /// <param name="filters">The packed CFA pattern value.</param>
    /// <param name="colorDescription">The colour descriptor (channel letters).</param>
    /// <param name="xTransPattern">The 6×6 X-Trans grid, for X-Trans sensors.</param>
    public RAWCFAPattern( uint filters, string colorDescription, int[][]? xTransPattern = null )
    {
        this.Filters          = filters;
        this.ColorDescription = colorDescription;

        if( filters == 0 )
        {
            this.PatternKind   = Kind.None;
            this.XTransPattern = null;
        }
        else if( filters == 9 )
        {
            this.PatternKind   = Kind.XTrans;
            this.XTransPattern = xTransPattern;
        }
        else
        {
            this.PatternKind   = Kind.Bayer;
            this.XTransPattern = null;
        }
    }

    /// <summary>
    /// Creates a CFA pattern from a marshaled LibRAW image-parameters structure.
    /// </summary>
    /// <param name="idata">The marshaled <c>libraw_iparams_t</c> fields.</param>
    internal RAWCFAPattern( LibRawIParams idata ) : this( idata.Filters, idata.Cdesc.DecodeCString(), idata.Filters == 9 ? ReadXTransPattern( idata.XTrans ) : null )
    {}

    /// <summary>
    /// The colour index of the pixel at a given position.
    /// </summary>
    /// <remarks>
    /// The returned index maps into <see cref="ColorDescription"/> (index <c>0</c> is
    /// its first character). For <see cref="Kind.None"/> sensors this is
    /// <see cref="FullColorIndex"/>.
    /// </remarks>
    /// <param name="row">The pixel row.</param>
    /// <param name="column">The pixel column.</param>
    /// <returns>The colour index for the pixel.</returns>
    public int Color( int row, int column )
    {
        switch( this.PatternKind )
        {
            case Kind.XTrans:

                if( this.XTransPattern is null )
                {
                    return FullColorIndex;
                }

                int r = ( ( row % 6 ) + 6 ) % 6;
                int c = ( ( column % 6 ) + 6 ) % 6;

                return this.XTransPattern[ r ][ c ];

            case Kind.Bayer:

                int shift = ( ( ( row << 1 ) & 14 ) | ( column & 1 ) ) << 1;

                return ( int )( ( this.Filters >> shift ) & 3 );

            default:

                return FullColorIndex;
        }
    }

    /// <summary>
    /// The channel letter of the pixel at a given position.
    /// </summary>
    /// <param name="row">The pixel row.</param>
    /// <param name="column">The pixel column.</param>
    /// <returns>
    /// The channel letter from <see cref="ColorDescription"/>, or
    /// <see langword="null"/> if the colour index has no corresponding letter (e.g.
    /// <see cref="Kind.None"/> sensors).
    /// </returns>
    public char? Channel( int row, int column )
    {
        int index = this.Color( row, column );

        if( index < 0 || index >= this.ColorDescription.Length )
        {
            return null;
        }

        return this.ColorDescription[ index ];
    }

    /// <summary>
    /// Returns a compact summary of the colour-filter-array layout.
    /// </summary>
    /// <returns><c>"no CFA"</c>, <c>"Bayer (…)"</c> or <c>"X-Trans (…)"</c>.</returns>
    public override string ToString()
    {
        return this.PatternKind switch
        {
            Kind.Bayer  => $"Bayer ({ this.ColorDescription })",
            Kind.XTrans => $"X-Trans ({ this.ColorDescription })",
            _           => "no CFA",
        };
    }

    /// <summary>
    /// Compares this pattern with another for value equality, comparing the X-Trans
    /// grid structurally rather than by reference.
    /// </summary>
    /// <param name="other">The pattern to compare with.</param>
    /// <returns>
    /// <see langword="true"/> when both patterns have the same kind, filters,
    /// descriptor and grid contents.
    /// </returns>
    public bool Equals( RAWCFAPattern? other )
    {
        if( other is null )
        {
            return false;
        }

        if( ReferenceEquals( this, other ) )
        {
            return true;
        }

        return this.PatternKind == other.PatternKind
            && this.Filters == other.Filters
            && this.ColorDescription == other.ColorDescription
            && GridsEqual( this.XTransPattern, other.XTransPattern );
    }

    /// <summary>
    /// Returns a hash code consistent with <see cref="Equals(RAWCFAPattern)"/>,
    /// folding in the X-Trans grid contents.
    /// </summary>
    /// <returns>The structural hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new HashCode();

        hash.Add( this.PatternKind );
        hash.Add( this.Filters );
        hash.Add( this.ColorDescription );

        if( this.XTransPattern is not null )
        {
            foreach( int[] gridRow in this.XTransPattern )
            {
                foreach( int value in gridRow )
                {
                    hash.Add( value );
                }
            }
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Reshapes LibRAW's flat 6×6 <c>xtrans</c> array into a colour-index grid.
    /// </summary>
    /// <param name="xtrans">The flat, row-major <c>xtrans</c> field (36 values).</param>
    /// <returns>A 6×6 grid of colour indices.</returns>
    private static int[][] ReadXTransPattern( sbyte[] xtrans )
    {
        return CFixedArray.Matrix( xtrans, 6, 6 )
            .Select( gridRow => gridRow.Select( value => ( int )value ).ToArray() )
            .ToArray();
    }

    /// <summary>
    /// Compares two X-Trans grids for structural equality.
    /// </summary>
    /// <param name="first">The first grid, or <see langword="null"/>.</param>
    /// <param name="second">The second grid, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> when both are absent or have identical shape and
    /// contents.
    /// </returns>
    private static bool GridsEqual( int[][]? first, int[][]? second )
    {
        if( ReferenceEquals( first, second ) )
        {
            return true;
        }

        if( first is null || second is null || first.Length != second.Length )
        {
            return false;
        }

        return first.Zip( second, ( a, b ) => a.SequenceEqual( b ) ).All( equal => equal );
    }
}
