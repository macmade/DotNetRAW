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
/// and use the C calling convention.
/// </remarks>
internal static partial class LibRaw
{
    /// <summary>
    /// The name of the native LibRAW library, resolved per platform to
    /// <c>libraw.dll</c> or <c>libraw.dylib</c>.
    /// </summary>
    private const string LibraryName = "libraw";

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
}
