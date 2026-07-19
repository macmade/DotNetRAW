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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotNetRAW;

/// <summary>
/// P/Invoke bindings to LibRAW's flat C API.
/// </summary>
/// <remarks>
/// The native library is loaded by the name <see cref="LibraryName"/>; the
/// binary matching the host platform (<c>libraw.dll</c> on Windows,
/// <c>libraw.dylib</c> on macOS) is packaged per runtime identifier and copied
/// next to the assembly, so it resolves from the application base directory at
/// run time. The bindings are source-generated with <see cref="LibraryImportAttribute"/>
/// and use the C calling convention (<c>cdecl</c>).
/// <para>
/// The context handle (<c>libraw_data_t*</c>) and every LibRAW-owned string are
/// exchanged as <see cref="IntPtr"/> rather than marshaled to a managed type, so
/// the interop marshaller never frees memory the library owns. Callers wrap the
/// context in a <see cref="LibRawHandle"/> and decode strings with
/// <see cref="Marshal.PtrToStringUTF8(IntPtr)"/>.
/// </para>
/// </remarks>
internal static partial class LibRaw
{
    /// <summary>
    /// The name of the native LibRAW library, resolved per platform to
    /// <c>libraw.dll</c> or <c>libraw.dylib</c>.
    /// </summary>
    private const string LibraryName = "libraw";

    /// <summary>
    /// Allocates and initializes a LibRAW context (<c>libraw_data_t*</c>), wrapped
    /// in an owning <see cref="LibRawHandle"/>.
    /// </summary>
    /// <remarks>
    /// The source-generated marshaller creates the <see cref="LibRawHandle"/> and
    /// stores the returned pointer atomically, so there is no window in which the
    /// native context could leak. A failed native allocation yields a handle whose
    /// <see cref="LibRawHandle.IsInvalid"/> is <see langword="true"/>. Release is
    /// handled by the handle itself (<c>libraw_recycle</c> then <c>libraw_close</c>).
    /// </remarks>
    /// <param name="flags">
    /// The LibRAW constructor flags; <c>0</c> (<c>LIBRAW_OPTIONS_NONE</c>) for the
    /// default behaviour.
    /// </param>
    /// <returns>An owning handle to the new context.</returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial LibRawHandle libraw_init( uint flags );

    /// <summary>
    /// Opens a RAW file at the given path into the context, reading its metadata.
    /// </summary>
    /// <param name="context">The context returned by <see cref="libraw_init"/>.</param>
    /// <param name="fileName">The path to the RAW file, marshaled as UTF-8.</param>
    /// <returns>
    /// <c>LIBRAW_SUCCESS</c> (<c>0</c>) on success, or a negative
    /// <see cref="LibRawError"/> code on failure.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName, StringMarshalling = StringMarshalling.Utf8 ) ]
    internal static partial int libraw_open_file( IntPtr context, string fileName );

    /// <summary>
    /// Opens a RAW file at the given wide-character path into the context.
    /// </summary>
    /// <remarks>
    /// The Windows-only counterpart of <see cref="libraw_open_file"/>, taking a
    /// <c>const wchar_t*</c> (UTF-16) path. It is used on Windows so a non-ASCII path
    /// is not mangled by the narrow CRT that <see cref="libraw_open_file"/>'s UTF-8
    /// path would pass through; macOS and Linux keep the UTF-8 entry point.
    /// </remarks>
    /// <param name="context">The context returned by <see cref="libraw_init"/>.</param>
    /// <param name="fileName">The path to the RAW file, marshaled as UTF-16.</param>
    /// <returns>
    /// <c>LIBRAW_SUCCESS</c> (<c>0</c>) on success, or a negative
    /// <see cref="LibRawError"/> code on failure.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName, StringMarshalling = StringMarshalling.Utf16 ) ]
    internal static partial int libraw_open_wfile( IntPtr context, string fileName );

    /// <summary>
    /// Opens a RAW image held entirely in memory into the context.
    /// </summary>
    /// <remarks>
    /// LibRAW retains the caller's <paramref name="buffer"/> pointer for the
    /// lifetime of the context, so the buffer must stay pinned and alive until the
    /// context is recycled.
    /// </remarks>
    /// <param name="context">The context returned by <see cref="libraw_init"/>.</param>
    /// <param name="buffer">A pointer to the pinned RAW bytes (<c>const void*</c>).</param>
    /// <param name="size">The length of <paramref name="buffer"/> in bytes.</param>
    /// <returns>
    /// <c>LIBRAW_SUCCESS</c> (<c>0</c>) on success, or a negative
    /// <see cref="LibRawError"/> code on failure.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial int libraw_open_buffer( IntPtr context, IntPtr buffer, nuint size );

    /// <summary>
    /// Unpacks the RAW sensor data of the currently-open image.
    /// </summary>
    /// <param name="context">The context with an image already opened.</param>
    /// <returns>
    /// <c>LIBRAW_SUCCESS</c> (<c>0</c>) on success, or a negative
    /// <see cref="LibRawError"/> code on failure.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial int libraw_unpack( IntPtr context );

    /// <summary>
    /// Frees the per-image data held by the context, leaving it reusable.
    /// </summary>
    /// <param name="context">The context to recycle.</param>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial void libraw_recycle( IntPtr context );

    /// <summary>
    /// Destroys the context and releases all memory it owns.
    /// </summary>
    /// <param name="context">The context to close; must not be used afterwards.</param>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial void libraw_close( IntPtr context );

    /// <summary>
    /// Returns the human-readable message for a LibRAW error code.
    /// </summary>
    /// <param name="errorCode">A LibRAW status/error code.</param>
    /// <returns>
    /// A pointer to a statically-allocated, NUL-terminated C string owned by
    /// LibRAW; the caller must not free it. Decode with
    /// <see cref="Marshal.PtrToStringUTF8(IntPtr)"/>.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial IntPtr libraw_strerror( int errorCode );

    /// <summary>
    /// Returns LibRAW's human-readable version string, for example
    /// <c>"0.22.2-Release"</c>.
    /// </summary>
    /// <remarks>
    /// The C signature is <c>const char* libraw_version( void )</c>. The pointer
    /// refers to a statically-allocated, NUL-terminated string owned by LibRAW,
    /// so it is returned as an <see cref="IntPtr"/> - rather than marshaled
    /// directly as a <see cref="string"/> - to keep the interop marshaller from
    /// attempting to free memory the library owns. Decode it with
    /// <see cref="Marshal.PtrToStringUTF8(IntPtr)"/>.
    /// </remarks>
    /// <returns>
    /// A pointer to the NUL-terminated version string; the caller must not free
    /// it.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial IntPtr libraw_version();

    /// <summary>
    /// Returns LibRAW's numeric version, for example <c>0x1602</c> (<c>5634</c>)
    /// for <c>0.22.2</c>.
    /// </summary>
    /// <returns>The version encoded as <c>(major &lt;&lt; 16) | (minor &lt;&lt; 8) | patch</c>.</returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial int libraw_versionNumber();

    /// <summary>
    /// Returns an interior pointer to the context's image-parameters
    /// (<c>idata</c>) sub-structure.
    /// </summary>
    /// <param name="context">The context to read from.</param>
    /// <returns>
    /// A pointer to the context's <c>libraw_iparams_t</c>; not owned by the caller
    /// and valid only while the context lives.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial IntPtr libraw_get_iparams( IntPtr context );

    /// <summary>
    /// Returns an interior pointer to the context's lens-information
    /// (<c>lens</c>) sub-structure.
    /// </summary>
    /// <param name="context">The context to read from.</param>
    /// <returns>
    /// A pointer to the context's <c>libraw_lensinfo_t</c>; not owned by the caller
    /// and valid only while the context lives.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial IntPtr libraw_get_lensinfo( IntPtr context );

    /// <summary>
    /// Returns an interior pointer to the context's other-metadata
    /// (<c>other</c>) sub-structure.
    /// </summary>
    /// <param name="context">The context to read from.</param>
    /// <returns>
    /// A pointer to the context's <c>libraw_imgother_t</c>; not owned by the caller
    /// and valid only while the context lives.
    /// </returns>
    [ UnmanagedCallConv( CallConvs = new[] { typeof( CallConvCdecl ) } ) ]
    [ LibraryImport( LibraryName ) ]
    internal static partial IntPtr libraw_get_imgother( IntPtr context );
}
