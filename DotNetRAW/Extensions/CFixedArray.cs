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

using System.Collections.Generic;
using System.Linq;

namespace DotNetRAW;

/// <summary>
/// Helpers for reading LibRAW's fixed-size C arrays into managed arrays.
/// </summary>
/// <remarks>
/// LibRAW exposes many values as fixed-size C arrays (e.g. <c>float cam_mul[4]</c>
/// or <c>float rgb_cam[3][4]</c>). The interop marshaller surfaces each of these
/// as a flat, one-dimensional managed array in row-major order. These helpers copy
/// such a field into an independent array the value types can own, and reshape a
/// flat two-dimensional field into rows and columns.
/// </remarks>
internal static class CFixedArray
{
    /// <summary>
    /// Copies a flat fixed-size C array field into an independent array.
    /// </summary>
    /// <remarks>
    /// The returned array is a fresh copy, so it stays valid and immutable once
    /// the source interop structure is discarded.
    /// </remarks>
    /// <typeparam name="T">The element type of the array.</typeparam>
    /// <param name="source">The flat fixed-size array field to copy.</param>
    /// <returns>The array's elements, in order.</returns>
    internal static T[] Values< T >( IReadOnlyList< T > source ) => [ .. source ];

    /// <summary>
    /// Reshapes a flat, row-major two-dimensional fixed-size C array field into a
    /// <paramref name="rows"/>&#215;<paramref name="columns"/> array of rows.
    /// </summary>
    /// <typeparam name="T">The element type of the array.</typeparam>
    /// <param name="source">The flat, row-major array field to reshape.</param>
    /// <param name="rows">The number of rows.</param>
    /// <param name="columns">The number of columns.</param>
    /// <returns>
    /// A <paramref name="rows"/>&#215;<paramref name="columns"/> array of the
    /// array's elements.
    /// </returns>
    internal static T[][] Matrix< T >( IReadOnlyList< T > source, int rows, int columns )
    {
        return Enumerable.Range( 0, rows )
            .Select( row => Enumerable.Range( 0, columns ).Select( column => source[ ( row * columns ) + column ] ).ToArray() )
            .ToArray();
    }
}
