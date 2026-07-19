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
using System.Runtime.InteropServices;

namespace DotNetRAW;

/// <summary>
/// A <see cref="SafeHandle"/> owning a native LibRAW context (<c>libraw_data_t*</c>).
/// </summary>
/// <remarks>
/// Releasing the handle calls <c>libraw_recycle</c> then <c>libraw_close</c>, the
/// pair LibRAW requires to free both the per-image data and the context itself.
/// Wrapping the pointer in a <see cref="SafeHandle"/> guarantees that release
/// happens exactly once - deterministically on <see cref="SafeHandle.Dispose()"/>,
/// or otherwise on finalization - and prevents the pointer being reclaimed while a
/// P/Invoke call using it is still in flight.
/// </remarks>
internal sealed class LibRawHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new, invalid handle owning nothing.
    /// </summary>
    /// <remarks>
    /// Public and parameterless so the source-generated interop marshaller can
    /// construct it when marshalling a <c>libraw_init</c> return and store the
    /// native pointer atomically; production code obtains handles through
    /// <see cref="Initialize"/>. The handle owns the pointer, so it is released on
    /// disposal.
    /// </remarks>
    public LibRawHandle() : base( IntPtr.Zero, ownsHandle: true )
    {}

    /// <summary>
    /// Whether the handle holds no native context.
    /// </summary>
    public override bool IsInvalid => this.handle == IntPtr.Zero;

    /// <summary>
    /// Allocates a new native LibRAW context and adopts it.
    /// </summary>
    /// <returns>
    /// A handle owning the new context; its <see cref="IsInvalid"/> is
    /// <see langword="true"/> when the native allocation failed.
    /// </returns>
    internal static LibRawHandle Initialize() => LibRaw.libraw_init( 0 );

    /// <summary>
    /// Releases the native context by recycling then closing it.
    /// </summary>
    /// <returns>Always <see langword="true"/>; the release cannot fail.</returns>
    protected override bool ReleaseHandle()
    {
        LibRaw.libraw_recycle( this.handle );
        LibRaw.libraw_close( this.handle );

        return true;
    }
}
