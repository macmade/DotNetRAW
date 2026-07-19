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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DotNetRAWTests;

/// <summary>
/// Helpers for locating the user-supplied RAW fixture files.
/// </summary>
/// <remarks>
/// Fixtures live in a <c>Test Files</c> directory at the repository root, outside
/// any project, so they are not copied to the build output. They are located
/// relative to this source file's compile-time path (captured via
/// <see cref="CallerFilePathAttribute"/>): a test assembly only ever runs from its
/// own checkout, so the path stays valid at run time. When no fixtures are present
/// the suite still runs - fixture-driven tests simply have nothing to iterate over.
/// </remarks>
public static class TestUtilities
{
    /// <summary>
    /// The file extensions recognized as RAW fixtures, matched case-insensitively
    /// and including the leading dot.
    /// </summary>
    private static readonly HashSet< string > RawFileExtensions = new( StringComparer.OrdinalIgnoreCase )
    {
        ".cr2", ".cr3", ".crw", ".nef", ".nrw", ".arw", ".sr2", ".srf",
        ".dng", ".raf", ".rw2", ".orf", ".pef", ".srw", ".rwl", ".iiq",
        ".3fr", ".fff", ".mos", ".mrw", ".kdc", ".dcr", ".erf", ".x3f",
    };

    /// <summary>
    /// The absolute paths of the RAW fixture files found at the repository root,
    /// searched recursively and sorted for deterministic ordering.
    /// </summary>
    /// <remarks>
    /// Despite the historical <c>…Urls</c> name, the values are absolute
    /// filesystem paths, not URIs.
    /// </remarks>
    public static IReadOnlyList< string > RawFileUrls => ResolveRawFileUrls();

    /// <summary>
    /// Whether any RAW fixtures are available.
    /// </summary>
    public static bool HasTestFiles => RawFileUrls.Count > 0;

    /// <summary>
    /// The RAW fixture files wrapped as xUnit theory data - one file path per case -
    /// for the fixture-driven <c>[ Theory ]</c> methods that consume them through
    /// <c>[ MemberData ]</c>.
    /// </summary>
    /// <remarks>
    /// Yields no cases when no fixtures are present, so fixture-driven theories simply
    /// run nothing rather than failing.
    /// </remarks>
    public static IEnumerable< object[] > RawFiles => RawFileUrls.Select( path => new object[] { path } );

    /// <summary>
    /// Resolves <see cref="RawFileUrls"/> relative to this source file's location.
    /// </summary>
    /// <param name="sourceFilePath">
    /// The compile-time path of this source file, supplied automatically by
    /// <see cref="CallerFilePathAttribute"/>; it is not passed explicitly.
    /// </param>
    /// <returns>
    /// The RAW fixture paths, sorted ordinally; empty when none are found.
    /// </returns>
    private static IReadOnlyList< string > ResolveRawFileUrls( [ CallerFilePath ] string sourceFilePath = "" )
    {
        string? testsDirectory = Path.GetDirectoryName( sourceFilePath );
        string? repositoryRoot = testsDirectory is null ? null : Path.GetDirectoryName( testsDirectory );

        if( repositoryRoot is null )
        {
            return [];
        }

        string root = Path.Combine( repositoryRoot, "Test Files" );

        if( Directory.Exists( root ) == false )
        {
            return [];
        }

        return Directory.EnumerateFiles( root, "*", SearchOption.AllDirectories )
            .Where( path => RawFileExtensions.Contains( Path.GetExtension( path ) ) )
            .OrderBy( path => path, StringComparer.Ordinal )
            .ToList();
    }
}
