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

using System.Runtime.InteropServices;

namespace DotNetRAW;

/// <summary>
/// Identifying information about the underlying LibRAW library the wrapper binds.
/// </summary>
/// <remarks>
/// A thin, read-only accessor over LibRAW's version entry points, letting consumers
/// report or assert on the exact LibRAW build in use.
/// </remarks>
public static class LibRAWVersion
{
    /// <summary>
    /// The LibRAW version string, for example <c>"0.22.2-Release"</c>.
    /// </summary>
    /// <remarks>
    /// Read from <c>libraw_version</c>; empty when the library reports no string.
    /// </remarks>
    public static string String => Marshal.PtrToStringUTF8( LibRaw.libraw_version() ) ?? "";

    /// <summary>
    /// The LibRAW version encoded as a single integer, computed as
    /// <c>(major &lt;&lt; 16) | (minor &lt;&lt; 8) | patch</c> - for example
    /// <c>5634</c> for <c>0.22.2</c>.
    /// </summary>
    /// <remarks>Read from <c>libraw_versionNumber</c>.</remarks>
    public static int Number => LibRaw.libraw_versionNumber();

    /// <summary>
    /// Creates and immediately releases a LibRAW context to prove the native library
    /// links and a working context can be spun up and torn down.
    /// </summary>
    /// <remarks>
    /// A smoke helper over <c>libraw_init</c>/<c>libraw_close</c> (through the owning
    /// <see cref="LibRawHandle"/>, which recycles then closes the context on
    /// disposal); it is not part of the public API.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if a LibRAW context was created and released.
    /// </returns>
    internal static bool CanCreateContext()
    {
        using LibRawHandle handle = LibRawHandle.Initialize();

        return handle.IsInvalid == false;
    }
}
