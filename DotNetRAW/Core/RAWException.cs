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

namespace DotNetRAW;

/// <summary>
/// The exception thrown by DotNetRAW when opening or reading a RAW file.
/// </summary>
/// <remarks>
/// A single exception type carrying a <see cref="RAWErrorKind"/> discriminator and
/// constructed through one static factory per error case. Failures originating in
/// LibRAW also carry the raw LibRAW status <see cref="Code"/> and its human-readable
/// message (from <c>libraw_strerror</c>), so callers can inspect the exact failure.
/// Every message is prefixed with <c>RAW Error: </c>.
/// <para>
/// Throwing from the public API is a deliberate, documented departure from the usual
/// "avoid throwing from public methods" guidance: the library reports failures
/// through this exception, matching the read-only source library's error model.
/// </para>
/// </remarks>
public sealed class RAWException : Exception
{
    /// <summary>The prefix applied to every RAW error message.</summary>
    private const string MessagePrefix = "RAW Error: ";

    /// <summary>The kind of error this exception represents.</summary>
    public RAWErrorKind Kind { get; }

    /// <summary>
    /// The LibRAW status code for failures originating in LibRAW, or
    /// <see langword="null"/> for errors raised before LibRAW is involved.
    /// </summary>
    public int? Code { get; }

    /// <summary>
    /// Initializes a new instance for the given kind, optional status code and
    /// specific description.
    /// </summary>
    /// <param name="kind">The kind of error.</param>
    /// <param name="code">The LibRAW status code, or <see langword="null"/> when not applicable.</param>
    /// <param name="description">
    /// The specific description, appended to the shared RAW error prefix.
    /// </param>
    private RAWException( RAWErrorKind kind, int? code, string description ) : base( MessagePrefix + description )
    {
        this.Kind = kind;
        this.Code = code;
    }

    /// <summary>Creates a <see cref="RAWErrorKind.InvalidFileURL"/> error.</summary>
    /// <param name="path">The offending file path.</param>
    /// <returns>The created exception.</returns>
    public static RAWException InvalidFileURL( string path ) => new RAWException( RAWErrorKind.InvalidFileURL, null, $"Invalid file URL: { path }" );

    /// <summary>Creates a <see cref="RAWErrorKind.CannotReadFile"/> error.</summary>
    /// <param name="path">The file that could not be read.</param>
    /// <returns>The created exception.</returns>
    public static RAWException CannotReadFile( string path ) => new RAWException( RAWErrorKind.CannotReadFile, null, $"Cannot read file: { path }" );

    /// <summary>Creates a <see cref="RAWErrorKind.OpenFailed"/> error.</summary>
    /// <param name="code">The LibRAW status code.</param>
    /// <param name="message">The LibRAW error message for the code.</param>
    /// <returns>The created exception.</returns>
    public static RAWException OpenFailed( int code, string message ) => new RAWException( RAWErrorKind.OpenFailed, code, $"Failed to open the RAW file ({ code.ToString( CultureInfo.InvariantCulture ) }): { message }" );

    /// <summary>Creates a <see cref="RAWErrorKind.UnpackFailed"/> error.</summary>
    /// <param name="code">The LibRAW status code.</param>
    /// <param name="message">The LibRAW error message for the code.</param>
    /// <returns>The created exception.</returns>
    public static RAWException UnpackFailed( int code, string message ) => new RAWException( RAWErrorKind.UnpackFailed, code, $"Failed to unpack the RAW data ({ code.ToString( CultureInfo.InvariantCulture ) }): { message }" );

    /// <summary>Creates a <see cref="RAWErrorKind.UnsupportedFormat"/> error.</summary>
    /// <returns>The created exception.</returns>
    public static RAWException UnsupportedFormat() => new RAWException( RAWErrorKind.UnsupportedFormat, null, "Unsupported RAW format" );

    /// <summary>Creates a <see cref="RAWErrorKind.LibRawError"/> error.</summary>
    /// <param name="code">The LibRAW status code.</param>
    /// <param name="message">The LibRAW error message for the code.</param>
    /// <returns>The created exception.</returns>
    public static RAWException LibRawError( int code, string message ) => new RAWException( RAWErrorKind.LibRawError, code, $"LibRAW error ({ code.ToString( CultureInfo.InvariantCulture ) }): { message }" );
}
