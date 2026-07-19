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
/// Round-trip tests for the LibRAW flat-C bindings, the context
/// <see cref="LibRawHandle"/>, and the <see cref="LibRawError"/> codes.
/// </summary>
/// <remarks>
/// These exercise the native library's lifecycle end to end - allocate, recycle,
/// close - proving the P/Invoke plumbing added in this milestone is callable, and
/// guard the hand-transcribed error-code values against the LibRAW headers.
/// </remarks>
public class LibRawInteropTests
{
    /// <summary>
    /// The numeric version of the bundled LibRAW is <c>5634</c> (<c>0x1602</c>,
    /// i.e. 0.22.2).
    /// </summary>
    [ Fact ]
    public void VersionNumberMatchesBundledBinary()
    {
        Assert.Equal( 5634, LibRaw.libraw_versionNumber() );
    }

    /// <summary>
    /// A context can be allocated, is valid, and releases cleanly on disposal
    /// (which recycles then closes it).
    /// </summary>
    [ Fact ]
    public void ContextInitializesAndReleases()
    {
        using LibRawHandle handle = LibRawHandle.Initialize();

        Assert.False( handle.IsInvalid );
    }

    /// <summary>
    /// <c>libraw_strerror</c> returns a non-empty message for the success code,
    /// proving the <c>const char*</c> return is decoded without freeing.
    /// </summary>
    [ Fact ]
    public void StrerrorReturnsAMessage()
    {
        string? message = Marshal.PtrToStringUTF8( LibRaw.libraw_strerror( ( int )LibRawError.Success ) );

        Assert.False( string.IsNullOrEmpty( message ) );
    }

    /// <summary>
    /// The error-code enum carries the exact numeric values from the LibRAW
    /// headers, asserted against independent literals.
    /// </summary>
    [ Fact ]
    public void ErrorCodesMatchTheHeaders()
    {
        Assert.Equal(       0, ( int )LibRawError.Success );
        Assert.Equal(      -1, ( int )LibRawError.UnspecifiedError );
        Assert.Equal(      -2, ( int )LibRawError.FileUnsupported );
        Assert.Equal(      -9, ( int )LibRawError.RequestForNonexistentThumbnail );
        Assert.Equal( -100007, ( int )LibRawError.InsufficientMemory );
        Assert.Equal( -100013, ( int )LibRawError.MempoolOverflow );
    }

    /// <summary>
    /// <see cref="LibRawErrorExtensions.IsFatal"/> classifies only codes below
    /// <c>-100000</c> as fatal.
    /// </summary>
    [ Fact ]
    public void IsFatalClassifiesOnlyTheDeepNegatives()
    {
        Assert.False( LibRawError.Success.IsFatal() );
        Assert.False( LibRawError.FileUnsupported.IsFatal() );
        Assert.False( LibRawError.RequestForNonexistentThumbnail.IsFatal() );
        Assert.True( LibRawError.InsufficientMemory.IsFatal() );
        Assert.True( LibRawError.IoError.IsFatal() );
        Assert.True( LibRawError.MempoolOverflow.IsFatal() );
    }
}
