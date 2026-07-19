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
using System.Text;

namespace DotNetRAW;

/// <summary>
/// Helpers for turning LibRAW's fixed-size C <c>char</c> buffers into strings.
/// </summary>
internal static class ByteArrayExtensions
{
    /// <summary>
    /// Decodes a fixed-size C <c>char</c> buffer as UTF-8, up to the first NUL.
    /// </summary>
    /// <remarks>
    /// LibRAW exposes many text fields as fixed C arrays (e.g. <c>char make[64]</c>),
    /// which the interop marshaller surfaces as fixed-length <c>byte</c> arrays.
    /// Only the bytes preceding the first NUL are decoded, so a buffer that is not
    /// NUL-terminated (fully used) is decoded in full and trailing garbage after a
    /// NUL is ignored.
    /// </remarks>
    /// <param name="bytes">The fixed-size buffer to decode.</param>
    /// <returns>The decoded string, empty when the buffer starts with a NUL.</returns>
    internal static string DecodeCString( this byte[] bytes )
    {
        int length = Array.IndexOf( bytes, ( byte )0 );

        if( length < 0 )
        {
            length = bytes.Length;
        }

        return Encoding.UTF8.GetString( bytes, 0, length );
    }
}
