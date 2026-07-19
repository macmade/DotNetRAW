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
using System.IO;
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Tests for opening, unpacking, reading and disposing a <see cref="RAWFile"/>.
/// </summary>
/// <remarks>
/// The error-path tests need no fixtures; the success paths are parameterised over
/// the user-supplied RAW files under <c>Test Files/</c> and simply run no cases when
/// none are present.
/// </remarks>
public class RAWFileTests
{
    /// <summary>
    /// Opening a path that does not exist reports an invalid file location.
    /// </summary>
    [ Fact ]
    public void NonexistentPathThrowsInvalidFileURL()
    {
        string id   = Guid.NewGuid().ToString( "N", CultureInfo.InvariantCulture );
        string path = Path.Combine( Path.GetTempPath(), $"dotnetraw-missing-{ id }.cr2" );

        RAWException exception = Assert.Throws< RAWException >( () => new RAWFile( path ) );

        Assert.Equal( RAWErrorKind.InvalidFileURL, exception.Kind );
    }

    /// <summary>
    /// Opening a directory rather than a file reports an invalid file location.
    /// </summary>
    [ Fact ]
    public void DirectoryPathThrowsInvalidFileURL()
    {
        RAWException exception = Assert.Throws< RAWException >( () => new RAWFile( Path.GetTempPath() ) );

        Assert.Equal( RAWErrorKind.InvalidFileURL, exception.Kind );
    }

    /// <summary>
    /// Opening an existing but unreadable file reports a read failure. Gated to
    /// non-Windows (POSIX permissions), and skipped when the process can read the
    /// file regardless (e.g. running as root).
    /// </summary>
    [ Fact ]
    public void UnreadableFileThrowsCannotReadFile()
    {
        if( OperatingSystem.IsWindows() )
        {
            return;
        }

        string id   = Guid.NewGuid().ToString( "N", CultureInfo.InvariantCulture );
        string path = Path.Combine( Path.GetTempPath(), $"dotnetraw-unreadable-{ id }.cr2" );

        File.WriteAllBytes( path, new byte[] { 1, 2, 3, 4 } );

        try
        {
            File.SetUnixFileMode( path, UnixFileMode.None );

            if( IsReadable( path ) )
            {
                return;
            }

            RAWException exception = Assert.Throws< RAWException >( () => new RAWFile( path ) );

            Assert.Equal( RAWErrorKind.CannotReadFile, exception.Kind );
        }
        finally
        {
            File.SetUnixFileMode( path, UnixFileMode.UserRead | UnixFileMode.UserWrite );
            File.Delete( path );
        }
    }

    /// <summary>
    /// Opening bytes that are not a RAW image fails with a <see cref="RAWException"/>.
    /// </summary>
    [ Fact ]
    public void GarbageBytesThrow()
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes( "this is definitely not a RAW file" );

        Assert.Throws< RAWException >( () => new RAWFile( bytes ) );
    }

    /// <summary>
    /// Opening empty bytes fails with a <see cref="RAWException"/>.
    /// </summary>
    [ Fact ]
    public void EmptyBytesThrow()
    {
        Assert.Throws< RAWException >( () => new RAWFile( Array.Empty< byte >() ) );
    }

    /// <summary>
    /// Every provided RAW fixture opens and unpacks eagerly and reports a non-empty
    /// description.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void OpensEagerlyWithNonEmptyDescription( string path )
    {
        using RAWFile file = new RAWFile( path );

        Assert.False( string.IsNullOrEmpty( file.ToString() ) );
    }

    /// <summary>
    /// Every provided RAW fixture can be opened lazily and unpacked on demand, and
    /// <see cref="RAWFile.Unpack"/> is idempotent.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void OpensLazilyAndUnpacksIdempotently( string path )
    {
        using RAWFile file = new RAWFile( path, new RAWParsingOptions( false ) );

        file.Unpack();
        file.Unpack();

        Assert.False( string.IsNullOrEmpty( file.ToString() ) );
    }

    /// <summary>
    /// A fixture opened from in-memory bytes behaves like one opened from a path.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void OpensFromBytesWithNonEmptyDescription( string path )
    {
        byte[] bytes = File.ReadAllBytes( path );

        using RAWFile file = new RAWFile( bytes );

        Assert.False( string.IsNullOrEmpty( file.ToString() ) );
    }

    /// <summary>
    /// After an eager open, the unpacked Bayer buffer is available and the zero-copy
    /// view spans exactly as many samples as the copy.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void RawImageCopyAndViewAgree( string path )
    {
        using RAWFile file = new RAWFile( path );

        ushort[]? copy = file.RawImage;

        if( copy is null )
        {
            return;
        }

        int viewLength = file.WithRawImage( samples => samples.Length );

        Assert.Equal( copy.Length, viewLength );
    }

    /// <summary>
    /// Reading metadata after disposal throws <see cref="ObjectDisposedException"/>.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void AccessorsThrowAfterDispose( string path )
    {
        RAWFile file = new RAWFile( path );

        file.Dispose();

        Assert.Throws< ObjectDisposedException >( () => file.ImageInfo );
    }

    /// <summary>
    /// After disposal, <see cref="RAWFile.ToString"/> returns a sentinel rather than
    /// throwing, so it stays safe to call implicitly (debuggers, logging, interpolation).
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void ToStringReturnsSentinelAfterDispose( string path )
    {
        RAWFile file = new RAWFile( path );

        file.Dispose();

        Assert.Equal( "RAWFile (disposed)", file.ToString() );
    }

    /// <summary>
    /// Whether the current process can open a file for reading.
    /// </summary>
    /// <param name="path">The file to probe.</param>
    /// <returns><see langword="true"/> if the file opens for reading.</returns>
    private static bool IsReadable( string path )
    {
        try
        {
            using FileStream stream = File.OpenRead( path );

            return true;
        }
        catch( Exception )
        {
            return false;
        }
    }
}
