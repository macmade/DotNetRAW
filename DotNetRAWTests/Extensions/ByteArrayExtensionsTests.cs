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

using System.Text;
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="ByteArrayExtensions.DecodeCString"/>, which decodes
/// LibRAW's fixed-size C <c>char</c> buffers.
/// </summary>
public class ByteArrayExtensionsTests
{
    /// <summary>
    /// Decoding stops at the first NUL, ignoring any bytes that follow it in the
    /// fixed-size buffer.
    /// </summary>
    [ Fact ]
    public void DecodeCStringStopsAtFirstNul()
    {
        byte[] bytes = [ ( byte )'R', ( byte )'G', ( byte )'B', 0, ( byte )'X', ( byte )'Y' ];

        Assert.Equal( "RGB", bytes.DecodeCString() );
    }

    /// <summary>
    /// A buffer with no NUL terminator is decoded in full.
    /// </summary>
    [ Fact ]
    public void DecodeCStringReadsWholeBufferWhenNotTerminated()
    {
        byte[] bytes = Encoding.UTF8.GetBytes( "Canon" );

        Assert.Equal( "Canon", bytes.DecodeCString() );
    }

    /// <summary>
    /// A leading NUL yields an empty string.
    /// </summary>
    [ Fact ]
    public void DecodeCStringWithLeadingNulYieldsEmpty()
    {
        byte[] bytes = [ 0, 0, 0, 0 ];

        Assert.Equal( "", bytes.DecodeCString() );
    }

    /// <summary>
    /// An empty buffer yields an empty string.
    /// </summary>
    [ Fact ]
    public void DecodeCStringOnEmptyBufferYieldsEmpty()
    {
        byte[] bytes = [];

        Assert.Equal( "", bytes.DecodeCString() );
    }

    /// <summary>
    /// Multi-byte UTF-8 content is decoded correctly up to the NUL.
    /// </summary>
    [ Fact ]
    public void DecodeCStringDecodesUtf8()
    {
        byte[] content = Encoding.UTF8.GetBytes( "Nikon café" );
        byte[] bytes   = [ .. content, 0, ( byte )'!' ];

        Assert.Equal( "Nikon café", bytes.DecodeCString() );
    }
}
