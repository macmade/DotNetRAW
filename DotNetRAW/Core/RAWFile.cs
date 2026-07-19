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
using System.Linq;
using System.Runtime.InteropServices;

namespace DotNetRAW;

/// <summary>
/// A read-only view over a camera RAW file, backed by LibRAW.
/// </summary>
/// <remarks>
/// Opens a RAW file - from a path or from in-memory bytes - and, depending on the
/// <see cref="RAWParsingOptions"/>, unpacks its sensor data. Structured metadata and
/// the raw sensor buffer are read off the underlying LibRAW context.
/// <para>
/// The file owns a native LibRAW context for its whole lifetime (through a
/// <see cref="LibRawHandle"/>) and releases it on <see cref="Dispose()"/>, with a
/// finalizer as a safety net. When opened from bytes it also owns a pinned copy of
/// the input, because LibRAW retains the buffer pointer and reads from it lazily.
/// The underlying context is not safe to use across threads, so an instance must not
/// be shared between them.
/// </para>
/// </remarks>
public sealed class RAWFile : IDisposable
{
    /// <summary>
    /// A reader over the zero-copy 16-bit Bayer sample view, used with
    /// <see cref="WithRawImage{TResult}(RawImageReader{TResult})"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ReadOnlySpan{T}"/> is a <c>ref struct</c>, so this dedicated
    /// delegate is used in place of a <see cref="Func{T, TResult}"/>, which cannot take
    /// one as a type argument. The span is valid only for the duration of the call.
    /// </remarks>
    /// <typeparam name="TResult">The result the reader produces.</typeparam>
    /// <param name="samples">The unpacked 16-bit Bayer samples.</param>
    /// <returns>The reader's result.</returns>
    public delegate TResult RawImageReader< TResult >( ReadOnlySpan< ushort > samples );

    /// <summary>The owning handle to the native LibRAW context.</summary>
    private readonly LibRawHandle handle;

    /// <summary>
    /// The pin holding the owned input bytes in place for the file's lifetime when it
    /// was opened from memory; unallocated for a path-based open.
    /// </summary>
    private GCHandle bufferPin;

    /// <summary>Whether the RAW data has been unpacked.</summary>
    private bool unpacked;

    /// <summary>Whether the file has been disposed.</summary>
    private bool disposed;

    /// <summary>The options the file was parsed with.</summary>
    public RAWParsingOptions Options { get; }

    /// <summary>
    /// Opens a RAW file from a path, unpacking eagerly.
    /// </summary>
    /// <param name="path">The location of the RAW file to open.</param>
    /// <exception cref="RAWException">
    /// The path is missing or a directory
    /// (<see cref="RAWErrorKind.InvalidFileURL"/>), the file cannot be read
    /// (<see cref="RAWErrorKind.CannotReadFile"/>), or LibRAW fails to open or unpack it.
    /// </exception>
    public RAWFile( string path ) : this( path, RAWParsingOptions.Default )
    {}

    /// <summary>
    /// Opens a RAW file from a path.
    /// </summary>
    /// <remarks>
    /// The location is passed to LibRAW's wide-character entry point on Windows and its
    /// UTF-8 entry point elsewhere, so a non-ASCII path opens correctly on every
    /// platform. A failure is classified only after the open fails, so a missing path
    /// or a directory is an invalid location and an existing-but-unreadable file is a
    /// read failure, with anything else surfaced as the LibRAW open error.
    /// </remarks>
    /// <param name="path">The location of the RAW file to open.</param>
    /// <param name="options">The parsing options to apply.</param>
    /// <exception cref="RAWException">
    /// The path is missing or a directory
    /// (<see cref="RAWErrorKind.InvalidFileURL"/>), the file cannot be read
    /// (<see cref="RAWErrorKind.CannotReadFile"/>), or LibRAW fails to open or unpack it.
    /// </exception>
    public RAWFile( string path, RAWParsingOptions options )
    {
        ArgumentNullException.ThrowIfNull( path );

        LibRawHandle context = CreateContext();
        int          status;

        try
        {
            status = OpenPath( context, path );
        }
        catch
        {
            context.Dispose();

            throw;
        }

        if( status != ( int )LibRawError.Success )
        {
            context.Dispose();

            throw ClassifyPathOpenError( path, status );
        }

        this.handle  = context;
        this.Options = options;

        this.FinishOpen();
    }

    /// <summary>
    /// Opens a RAW file from in-memory bytes, unpacking eagerly.
    /// </summary>
    /// <param name="bytes">The complete contents of a RAW file.</param>
    /// <exception cref="RAWException">LibRAW fails to open or unpack the bytes.</exception>
    public RAWFile( byte[] bytes ) : this( bytes, RAWParsingOptions.Default )
    {}

    /// <summary>
    /// Opens a RAW file from in-memory bytes.
    /// </summary>
    /// <remarks>
    /// LibRAW keeps the supplied pointer and reads from it lazily, so the bytes are
    /// copied into an array this file pins and owns for its whole lifetime.
    /// </remarks>
    /// <param name="bytes">The complete contents of a RAW file.</param>
    /// <param name="options">The parsing options to apply.</param>
    /// <exception cref="RAWException">LibRAW fails to open or unpack the bytes.</exception>
    public RAWFile( byte[] bytes, RAWParsingOptions options )
    {
        ArgumentNullException.ThrowIfNull( bytes );

        LibRawHandle context = CreateContext();
        GCHandle     pin     = default;
        int          status;

        try
        {
            byte[] owned = ( byte[] )bytes.Clone();

            pin    = GCHandle.Alloc( owned, GCHandleType.Pinned );
            status = LibRaw.libraw_open_buffer( context.DangerousGetHandle(), pin.AddrOfPinnedObject(), ( nuint )owned.Length );
        }
        catch
        {
            if( pin.IsAllocated )
            {
                pin.Free();
            }

            context.Dispose();

            throw;
        }

        if( status != ( int )LibRawError.Success )
        {
            pin.Free();
            context.Dispose();

            throw OpenError( status );
        }

        this.handle    = context;
        this.bufferPin = pin;
        this.Options   = options;

        this.FinishOpen();
    }

    /// <summary>
    /// Unpacks the RAW sensor data if it has not been unpacked yet.
    /// </summary>
    /// <remarks>
    /// Idempotent: calling it again has no effect. It runs automatically during
    /// construction when <see cref="RAWParsingOptions.UnpacksImmediately"/> is set.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    /// <exception cref="RAWException">LibRAW fails to unpack the data.</exception>
    public void Unpack()
    {
        IntPtr context = this.Context();

        if( this.unpacked )
        {
            return;
        }

        int status = LibRaw.libraw_unpack( context );

        if( status != ( int )LibRawError.Success )
        {
            throw UnpackError( status );
        }

        this.unpacked = true;
    }

    /// <summary>The geometry of the RAW image: sensor and output dimensions, margins, pitch, pixel aspect, and orientation.</summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWImageSizes ImageSizes => new RAWImageSizes( this.ReadStruct< LibRawImageSizes >( LibRawData.SizesOffset ) );

    /// <summary>Camera identification and sensor-description metadata.</summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWImageInfo ImageInfo => new RAWImageInfo( this.ReadStruct< LibRawIParams >( LibRawData.IParamsOffset ) );

    /// <summary>The colour-filter-array layout of the sensor.</summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWCFAPattern CfaPattern => new RAWCFAPattern( this.ReadStruct< LibRawIParams >( LibRawData.IParamsOffset ) );

    /// <summary>Exposure and shot metadata (ISO, shutter, aperture, focal length, timestamp, description, artist, body serial).</summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWShotInfo ShotInfo => new RAWShotInfo( this.ReadStruct< LibRawImgOther >( LibRawData.ImgOtherOffset ), this.ReadStruct< LibRawShootingInfo >( LibRawData.ShootingInfoOffset ) );

    /// <summary>The GPS location recorded with the file, or <see langword="null"/> if none is present.</summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWGPSInfo? GpsInfo => RAWGPSInfo.FromNative( this.ReadStruct< LibRawImgOther >( LibRawData.ImgOtherOffset ).ParsedGps );

    /// <summary>Colour calibration data: black/saturation levels, white-balance multipliers, and colour-conversion matrices.</summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWColorData ColorData => new RAWColorData( this.ReadStruct< LibRawColorData >( LibRawData.ColorOffset + LibRawData.ColorBodyRelativeOffset ), this.ReadChannelBlackLevels() );

    /// <summary>Lens identification and characteristics.</summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWLensInfo LensInfo => new RAWLensInfo( this.ReadStruct< LibRawLensInfo >( LibRawData.LensInfoOffset ), this.ReadStruct< LibRawMakerNotesLens >( LibRawData.MakerNotesLensOffset ), this.ReadStruct< LibRawDngLens >( LibRawData.DngLensOffset ) );

    /// <summary>Camera-common maker-note metadata (flash, temperatures, colour space, firmware, sensitivity).</summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWCameraMetadata CameraMetadata => new RAWCameraMetadata( this.ReadStruct< LibRawMetadataCommon >( LibRawData.MetadataCommonOffset ) );

    /// <summary>
    /// A description of the unpacked raw sensor buffer: its geometry and which in-memory
    /// layout LibRAW produced (<see cref="RAWSensorData.Layout.None"/> before unpacking).
    /// </summary>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public RAWSensorData SensorData => new RAWSensorData( this.ReadStruct< LibRawRawData >( LibRawData.RawDataOffset ), this.ReadStruct< LibRawImageSizes >( LibRawData.SizesOffset ) );

    /// <summary>
    /// A copy of the unpacked 16-bit single-channel Bayer buffer, or <see langword="null"/>
    /// when it is unavailable (the file is not unpacked, or the decoder produced a
    /// different layout - see <see cref="SensorData"/>).
    /// </summary>
    /// <remarks>The returned array is owned by the caller; <see cref="WithRawImage{TResult}(RawImageReader{TResult})"/> avoids the copy.</remarks>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public ushort[]? RawImage => this.WithRawImage( samples => samples.ToArray() );

    /// <summary>
    /// Provides temporary, zero-copy access to the unpacked 16-bit Bayer buffer.
    /// </summary>
    /// <remarks>
    /// The span passed to <paramref name="reader"/> points directly into memory owned by
    /// LibRAW and is valid only for the duration of the call; it must not escape.
    /// The buffer spans <c>raw_height × (raw_pitch / 2)</c> samples, matching the geometry
    /// in <see cref="SensorData"/>.
    /// </remarks>
    /// <typeparam name="TResult">The result the reader produces.</typeparam>
    /// <param name="reader">A reader receiving the raw sample span.</param>
    /// <returns>
    /// The reader's result, or the default of <typeparamref name="TResult"/>
    /// (<see langword="null"/> for reference types) when no 16-bit Bayer buffer is available.
    /// </returns>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    public TResult? WithRawImage< TResult >( RawImageReader< TResult > reader )
    {
        ArgumentNullException.ThrowIfNull( reader );

        IntPtr           context = this.Context();
        LibRawImageSizes sizes   = ReadStructAt< LibRawImageSizes >( context, LibRawData.SizesOffset );
        LibRawRawData    rawData = ReadStructAt< LibRawRawData >( context, LibRawData.RawDataOffset );
        int              height  = sizes.RawHeight;
        int              pitch   = ( int )sizes.RawPitch;

        if( rawData.RawImage == IntPtr.Zero || height <= 0 || pitch <= 0 )
        {
            return default;
        }

        int count = height * ( pitch / 2 );

        try
        {
            unsafe
            {
                return reader( new ReadOnlySpan< ushort >( ( void* )rawData.RawImage, count ) );
            }
        }
        finally
        {
            // The span points into memory owned by the native context. Keep this file -
            // and therefore its context handle - alive until the reader has finished, as
            // nothing above references it once the context pointer has been captured.
            GC.KeepAlive( this );
        }
    }

    /// <summary>
    /// Returns a rich, one-line summary of the file: camera, sensor dimensions,
    /// colour-filter array, exposure, and lens, omitting anything the file does not record.
    /// </summary>
    /// <remarks>
    /// Unlike the data accessors, this never throws once the file is disposed - it returns
    /// a short sentinel instead - so it stays safe to call implicitly, for example from a
    /// debugger, logging, or string interpolation.
    /// </remarks>
    /// <returns>
    /// The summary with its parts joined by commas, or <c>"RAWFile (disposed)"</c> once the
    /// file has been disposed.
    /// </returns>
    public override string ToString()
    {
        if( this.disposed )
        {
            return "RAWFile (disposed)";
        }

        RAWImageInfo  info   = this.ImageInfo;
        RAWImageSizes sizes  = this.ImageSizes;
        RAWCFAPattern cfa    = this.CfaPattern;
        RAWShotInfo   shot   = this.ShotInfo;
        string        camera = $"{ info.Make } { info.Model }".Trim();
        bool          shots  = shot.IsoSpeed > 0 || shot.ShutterSpeed > 0 || shot.Aperture > 0 || shot.FocalLength > 0;

        string[] parts =
        [
            camera.Length == 0 ? "RAW file" : camera,
            string.Create( CultureInfo.InvariantCulture, $"{ sizes.RawWidth }×{ sizes.RawHeight }" ),
            cfa.PatternKind == RAWCFAPattern.Kind.None ? "" : cfa.ToString(),
            shots ? shot.ToString() : "",
            this.LensInfo.LensModel,
        ];

        return string.Join( ", ", parts.Where( part => part.Length > 0 ) );
    }

    /// <summary>
    /// Releases the native context and any owned input buffer.
    /// </summary>
    public void Dispose()
    {
        this.Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Releases the native context and unpins the owned input buffer.
    /// </summary>
    /// <remarks>
    /// The context handle is a <see cref="LibRawHandle"/>, so it is disposed
    /// deterministically here and otherwise finalizes itself; the pin is a runtime
    /// resource freed on both paths. Idempotent, so the finalizer and an explicit
    /// <see cref="Dispose()"/> never double-release.
    /// </remarks>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose()"/>; <see langword="false"/>
    /// from the finalizer.
    /// </param>
    private void Dispose( bool disposing )
    {
        if( this.disposed )
        {
            return;
        }

        this.disposed = true;

        if( disposing )
        {
            this.handle.Dispose();
        }

        if( this.bufferPin.IsAllocated )
        {
            this.bufferPin.Free();
        }
    }

    /// <summary>
    /// Releases the native context and any owned input buffer if <see cref="Dispose()"/>
    /// was not called.
    /// </summary>
    ~RAWFile()
    {
        this.Dispose( false );
    }

    /// <summary>
    /// Unpacks eagerly when the options request it, releasing all resources if the
    /// unpack fails.
    /// </summary>
    private void FinishOpen()
    {
        if( this.Options.UnpacksImmediately == false )
        {
            return;
        }

        try
        {
            this.Unpack();
        }
        catch
        {
            this.Dispose();

            throw;
        }
    }

    /// <summary>
    /// Returns the live native context pointer, guarding against use after disposal.
    /// </summary>
    /// <returns>The context (<c>libraw_data_t*</c>) pointer.</returns>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    private IntPtr Context()
    {
        ObjectDisposedException.ThrowIf( this.disposed, this );

        return this.handle.DangerousGetHandle();
    }

    /// <summary>
    /// Marshals one of LibRAW's sub-structures from the context at its byte offset.
    /// </summary>
    /// <typeparam name="T">The marshaling struct type.</typeparam>
    /// <param name="offset">The offset of the sub-structure within the context.</param>
    /// <returns>The marshaled sub-structure.</returns>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    private T ReadStruct< T >( int offset ) where T : struct
    {
        try
        {
            return ReadStructAt< T >( this.Context(), offset );
        }
        finally
        {
            // Keep this file alive across the native read: the marshalling dereferences
            // the context pointer at a GC safepoint, so the handle must not be finalized
            // mid-read.
            GC.KeepAlive( this );
        }
    }

    /// <summary>
    /// Marshals a struct from a context pointer at a byte offset.
    /// </summary>
    /// <typeparam name="T">The marshaling struct type.</typeparam>
    /// <param name="context">The context base pointer.</param>
    /// <param name="offset">The offset of the sub-structure within the context.</param>
    /// <returns>The marshaled struct.</returns>
    private static T ReadStructAt< T >( IntPtr context, int offset ) where T : struct
    {
        return Marshal.PtrToStructure< T >( IntPtr.Add( context, offset ) )!;
    }

    /// <summary>
    /// Reads the first four per-channel black levels (<c>cblack</c>) from the context.
    /// </summary>
    /// <remarks>
    /// <c>cblack</c> is an inline array in the colour sub-structure, so its first four
    /// values are always present (zero when unused); it is read here separately because
    /// the marshaled colour struct starts past it, at
    /// <see cref="LibRawData.ColorBodyRelativeOffset"/>.
    /// </remarks>
    /// <returns>The four per-channel black levels.</returns>
    /// <exception cref="ObjectDisposedException">The file has been disposed.</exception>
    private uint[] ReadChannelBlackLevels()
    {
        try
        {
            IntPtr cblack = IntPtr.Add( this.Context(), LibRawData.ColorOffset + LibRawData.ColorCblackRelativeOffset );
            uint[] levels = new uint[ 4 ];

            for( int index = 0; index < levels.Length; index++ )
            {
                levels[ index ] = ( uint )Marshal.ReadInt32( cblack, index * sizeof( int ) );
            }

            return levels;
        }
        finally
        {
            // Keep this file alive across the reads that dereference the native context.
            GC.KeepAlive( this );
        }
    }

    /// <summary>
    /// Allocates a native LibRAW context, failing with a LibRAW error when the native
    /// allocation does not succeed.
    /// </summary>
    /// <returns>An owning handle to the new context.</returns>
    /// <exception cref="RAWException">The native allocation failed.</exception>
    private static LibRawHandle CreateContext()
    {
        LibRawHandle context = LibRawHandle.Initialize();

        if( context.IsInvalid )
        {
            context.Dispose();

            throw RAWException.LibRawError( ( int )LibRawError.InsufficientMemory, MessageFor( ( int )LibRawError.InsufficientMemory ) );
        }

        return context;
    }

    /// <summary>
    /// Opens a path into the context through the platform-appropriate LibRAW entry point.
    /// </summary>
    /// <param name="context">The context to open into.</param>
    /// <param name="path">The RAW file path.</param>
    /// <returns>The LibRAW status code.</returns>
    private static int OpenPath( LibRawHandle context, string path )
    {
        IntPtr pointer = context.DangerousGetHandle();

        if( OperatingSystem.IsWindows() )
        {
            return LibRaw.libraw_open_wfile( pointer, path );
        }

        return LibRaw.libraw_open_file( pointer, path );
    }

    /// <summary>
    /// Classifies a failed path open as an invalid location, an unreadable file, or a
    /// LibRAW open failure.
    /// </summary>
    /// <remarks>
    /// The classification runs only after the open fails, so there is no
    /// time-of-check/time-of-use gap.
    /// </remarks>
    /// <param name="path">The path that failed to open.</param>
    /// <param name="status">The LibRAW status code from the open attempt.</param>
    /// <returns>The exception describing the failure.</returns>
    private static RAWException ClassifyPathOpenError( string path, int status )
    {
        if( Directory.Exists( path ) || File.Exists( path ) == false )
        {
            return RAWException.InvalidFileURL( path );
        }

        if( IsUnreadable( path ) )
        {
            return RAWException.CannotReadFile( path );
        }

        return OpenError( status );
    }

    /// <summary>
    /// Whether an existing file cannot be opened for reading.
    /// </summary>
    /// <param name="path">The file to probe.</param>
    /// <returns><see langword="true"/> when the file cannot be read.</returns>
    private static bool IsUnreadable( string path )
    {
        try
        {
            using FileStream stream = File.OpenRead( path );

            return false;
        }
        catch( Exception exception ) when( exception is IOException or UnauthorizedAccessException )
        {
            return true;
        }
    }

    /// <summary>
    /// Maps a LibRAW open status code to a <see cref="RAWException"/>.
    /// </summary>
    /// <param name="status">A non-success LibRAW status code from an open call.</param>
    /// <returns>The corresponding exception.</returns>
    private static RAWException OpenError( int status )
    {
        if( status == ( int )LibRawError.FileUnsupported )
        {
            return RAWException.UnsupportedFormat();
        }

        return RAWException.OpenFailed( status, MessageFor( status ) );
    }

    /// <summary>
    /// Maps a LibRAW unpack status code to a <see cref="RAWException"/>.
    /// </summary>
    /// <param name="status">A non-success LibRAW status code from an unpack call.</param>
    /// <returns>The corresponding exception.</returns>
    private static RAWException UnpackError( int status )
    {
        if( status == ( int )LibRawError.FileUnsupported )
        {
            return RAWException.UnsupportedFormat();
        }

        return RAWException.UnpackFailed( status, MessageFor( status ) );
    }

    /// <summary>
    /// Returns the LibRAW error message for a status code.
    /// </summary>
    /// <param name="status">A LibRAW status code.</param>
    /// <returns>The message from <c>libraw_strerror</c>, or an empty string.</returns>
    private static string MessageFor( int status )
    {
        return Marshal.PtrToStringUTF8( LibRaw.libraw_strerror( status ) ) ?? "";
    }
}
