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
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Smoke tests proving the native LibRAW library is correctly packaged and
/// loadable on the running platform.
/// </summary>
/// <remarks>
/// These exercise the P/Invoke plumbing end to end: a passing assertion means
/// the native binary for the host runtime identifier was copied next to the
/// test assembly and resolved by <c>DllImport</c> at run time. The version is
/// asserted against the bundled LibRAW 0.22.2 build, which reports an identical
/// string across the Windows and macOS binaries.
/// </remarks>
public class NativeLibraryTests
{
    /// <summary>
    /// The bundled native library reports LibRAW's expected version string,
    /// proving it loads and its C entry points are callable on this platform.
    /// </summary>
    [ Fact ]
    public void LibRawVersionMatchesBundledBinary()
    {
        string? version = Marshal.PtrToStringUTF8( LibRaw.libraw_version() );

        Assert.Equal( "0.22.2-Release", version );
    }
}
