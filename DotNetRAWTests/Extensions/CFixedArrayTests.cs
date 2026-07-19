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

using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="CFixedArray"/>, which reads LibRAW's fixed-size C
/// arrays into flat and reshaped managed arrays.
/// </summary>
public class CFixedArrayTests
{
    /// <summary>
    /// A flat fixed-size array is read into an array with its elements in order.
    /// </summary>
    [ Fact ]
    public void ValuesReadsArrayInOrder()
    {
        float[] source = [ 1, 2, 3, 4 ];
        float[] values = CFixedArray.Values( source );

        Assert.Equal< float >( [ 1, 2, 3, 4 ], values );
    }

    /// <summary>
    /// <see cref="CFixedArray.Values"/> returns an independent copy, so mutating
    /// the source afterwards does not affect the result.
    /// </summary>
    [ Fact ]
    public void ValuesReturnsIndependentCopy()
    {
        float[] source = [ 1, 2, 3, 4 ];
        float[] values = CFixedArray.Values( source );

        source[ 0 ] = 99;

        Assert.Equal< float >( [ 1, 2, 3, 4 ], values );
    }

    /// <summary>
    /// A flat row-major array is reshaped into rows and columns, matching the
    /// layout LibRAW uses for its matrices.
    /// </summary>
    [ Fact ]
    public void MatrixReshapesRowMajor()
    {
        int[]   source = [ 1, 2, 3, 4, 5, 6 ];
        int[][] matrix = CFixedArray.Matrix( source, 2, 3 );

        Assert.Equal( new int[][] { [ 1, 2, 3 ], [ 4, 5, 6 ] }, matrix );
    }

    /// <summary>
    /// A 3&#215;4 reshape matches the layout LibRAW uses for its colour matrices.
    /// </summary>
    [ Fact ]
    public void MatrixThreeByFour()
    {
        float[]   source = [ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 ];
        float[][] matrix = CFixedArray.Matrix( source, 3, 4 );

        Assert.Equal( new float[][] { [ 0, 1, 2, 3 ], [ 4, 5, 6, 7 ], [ 8, 9, 10, 11 ] }, matrix );
    }
}
