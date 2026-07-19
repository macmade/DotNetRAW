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
using System.Numerics;

namespace DotNetRAW;

/// <summary>
/// Formatting helpers for binary floating-point values used in descriptions.
/// </summary>
internal static class FloatingPointExtensions
{
    /// <summary>
    /// The magnitude at or above which a whole value is rendered with its default
    /// representation rather than as an integer, guarding the integer conversion
    /// against values too large to represent exactly.
    /// </summary>
    private const double CompactIntegerLimit = 1e9;

    /// <summary>
    /// A compact textual form for descriptions: whole values drop the trailing
    /// <c>.0</c> (e.g. <c>50</c> rather than <c>50.0</c>), while fractional values
    /// keep their shortest round-tripping representation (e.g. <c>2.8</c>).
    /// </summary>
    /// <remarks>
    /// Non-finite or very large values fall back to the default representation.
    /// The output is always formatted with <see cref="CultureInfo.InvariantCulture"/>,
    /// so the decimal separator is a period regardless of the current culture.
    /// </remarks>
    /// <typeparam name="T">The floating-point type of the value.</typeparam>
    /// <param name="value">The value to render.</param>
    /// <returns>The compact, culture-invariant description of <paramref name="value"/>.</returns>
    internal static string CompactDescription< T >( this T value ) where T : IBinaryFloatingPointIeee754< T >
    {
        if( T.IsFinite( value ) && T.IsInteger( value ) && T.Abs( value ) < T.CreateChecked( CompactIntegerLimit ) )
        {
            return long.CreateTruncating( value ).ToString( CultureInfo.InvariantCulture );
        }

        return value.ToString( null, CultureInfo.InvariantCulture );
    }
}
