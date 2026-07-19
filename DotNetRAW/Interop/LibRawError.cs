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
/// The status and error codes returned by LibRAW's flat C API
/// (<c>enum LibRaw_errors</c>).
/// </summary>
/// <remarks>
/// The non-negative <see cref="Success"/> aside, the codes split into two ranges:
/// ordinary negatives (<c>-1</c> to <c>-9</c>) reporting recoverable API misuse or
/// missing data, and fatal negatives below <c>-100000</c> reporting unrecoverable
/// failures such as I/O errors or exhausted memory. Use <see cref="LibRawErrorExtensions.IsFatal"/>
/// to tell the two apart.
/// </remarks>
internal enum LibRawError
{
    /// <summary>The operation succeeded (<c>LIBRAW_SUCCESS</c>).</summary>
    Success = 0,

    /// <summary>An unspecified error occurred (<c>LIBRAW_UNSPECIFIED_ERROR</c>).</summary>
    UnspecifiedError = -1,

    /// <summary>The file format is not supported (<c>LIBRAW_FILE_UNSUPPORTED</c>).</summary>
    FileUnsupported = -2,

    /// <summary>A requested image does not exist (<c>LIBRAW_REQUEST_FOR_NONEXISTENT_IMAGE</c>).</summary>
    RequestForNonexistentImage = -3,

    /// <summary>An API call was made out of the required order (<c>LIBRAW_OUT_OF_ORDER_CALL</c>).</summary>
    OutOfOrderCall = -4,

    /// <summary>No thumbnail is present (<c>LIBRAW_NO_THUMBNAIL</c>).</summary>
    NoThumbnail = -5,

    /// <summary>The thumbnail format is not supported (<c>LIBRAW_UNSUPPORTED_THUMBNAIL</c>).</summary>
    UnsupportedThumbnail = -6,

    /// <summary>The input stream is closed (<c>LIBRAW_INPUT_CLOSED</c>).</summary>
    InputClosed = -7,

    /// <summary>The operation is not implemented (<c>LIBRAW_NOT_IMPLEMENTED</c>).</summary>
    NotImplemented = -8,

    /// <summary>A requested thumbnail does not exist (<c>LIBRAW_REQUEST_FOR_NONEXISTENT_THUMBNAIL</c>).</summary>
    RequestForNonexistentThumbnail = -9,

    /// <summary>
    /// Memory allocation failed (<c>LIBRAW_UNSUFFICIENT_MEMORY</c>; the LibRAW
    /// spelling is preserved only in the C macro).
    /// </summary>
    InsufficientMemory = -100007,

    /// <summary>The RAW data is corrupt (<c>LIBRAW_DATA_ERROR</c>).</summary>
    DataError = -100008,

    /// <summary>An I/O error occurred (<c>LIBRAW_IO_ERROR</c>).</summary>
    IoError = -100009,

    /// <summary>The operation was cancelled by a callback (<c>LIBRAW_CANCELLED_BY_CALLBACK</c>).</summary>
    CancelledByCallback = -100010,

    /// <summary>The requested crop is invalid (<c>LIBRAW_BAD_CROP</c>).</summary>
    BadCrop = -100011,

    /// <summary>The file exceeds LibRAW's size limits (<c>LIBRAW_TOO_BIG</c>).</summary>
    TooBig = -100012,

    /// <summary>LibRAW's memory pool overflowed (<c>LIBRAW_MEMPOOL_OVERFLOW</c>).</summary>
    MempoolOverflow = -100013,
}

/// <summary>
/// Helpers for classifying <see cref="LibRawError"/> codes.
/// </summary>
/// <remarks>
/// Kept beside <see cref="LibRawError"/> - an <c>enum</c> cannot carry methods -
/// so the classification LibRAW expresses through the <c>LIBRAW_FATAL_ERROR</c>
/// macro travels with the codes it applies to.
/// </remarks>
internal static class LibRawErrorExtensions
{
    /// <summary>
    /// The threshold below which a LibRAW error code is fatal
    /// (<c>LIBRAW_FATAL_ERROR</c> is <c>ec &lt; -100000</c>).
    /// </summary>
    private const int FatalThreshold = -100000;

    /// <summary>
    /// Whether a LibRAW error code denotes an unrecoverable failure.
    /// </summary>
    /// <param name="code">The LibRAW status/error code.</param>
    /// <returns>
    /// <see langword="true"/> for fatal codes (below <c>-100000</c>, such as I/O
    /// or memory failures); <see langword="false"/> for success and the ordinary
    /// recoverable codes.
    /// </returns>
    internal static bool IsFatal( this LibRawError code ) => (int)code < FatalThreshold;
}
