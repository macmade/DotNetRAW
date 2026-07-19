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

namespace DotNetRAW;

/// <summary>
/// Identifies which kind of error a <see cref="RAWException"/> represents.
/// </summary>
/// <remarks>
/// A discriminator a consumer can branch on to identify which failure a caught
/// <see cref="RAWException"/> represents.
/// </remarks>
public enum RAWErrorKind
{
    /// <summary>The provided path is not a valid local file URL.</summary>
    InvalidFileURL,

    /// <summary>The file at the provided path does not exist or is not readable.</summary>
    CannotReadFile,

    /// <summary>LibRAW failed to open the RAW input.</summary>
    OpenFailed,

    /// <summary>LibRAW failed to unpack the RAW data.</summary>
    UnpackFailed,

    /// <summary>The input is not a RAW format supported by LibRAW.</summary>
    UnsupportedFormat,

    /// <summary>
    /// A LibRAW operation failed with a status code not covered by a more specific
    /// case.
    /// </summary>
    LibRawError,
}
