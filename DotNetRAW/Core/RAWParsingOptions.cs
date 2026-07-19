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
/// Options controlling how a RAW file is parsed.
/// </summary>
/// <remarks>
/// The read-only scope keeps this intentionally small; it currently only governs
/// when the RAW data is unpacked. Because the language default of a value type
/// zero-initializes its fields, use <see cref="Default"/> (or a constructor) for the
/// eager-unpacking default rather than <c>default</c>, which would be lazy.
/// </remarks>
public readonly record struct RAWParsingOptions
{
    /// <summary>
    /// Whether the RAW data is unpacked as soon as the file is opened.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/> (the default), the file unpacks eagerly while it
    /// is opened, so any open error and any unpack error surface up front. When
    /// <see langword="false"/>, unpacking is deferred until it is requested.
    /// </remarks>
    public bool UnpacksImmediately { get; init; }

    /// <summary>The default options: eager unpacking.</summary>
    public static RAWParsingOptions Default { get; } = new RAWParsingOptions();

    /// <summary>
    /// Creates a set of parsing options that unpacks eagerly.
    /// </summary>
    public RAWParsingOptions() : this( true )
    {}

    /// <summary>
    /// Creates a set of parsing options.
    /// </summary>
    /// <param name="unpacksImmediately">
    /// Whether to unpack the RAW data as soon as the file is opened.
    /// </param>
    public RAWParsingOptions( bool unpacksImmediately )
    {
        this.UnpacksImmediately = unpacksImmediately;
    }

    /// <summary>
    /// Returns a compact summary of the options.
    /// </summary>
    /// <returns><c>"eager unpack"</c> when unpacking eagerly, otherwise <c>"lazy unpack"</c>.</returns>
    public override string ToString() => this.UnpacksImmediately ? "eager unpack" : "lazy unpack";
}
