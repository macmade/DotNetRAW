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

// Interop struct fields are populated by the marshaller from native memory, not
// assigned in C# code.
#pragma warning disable CS0649

namespace DotNetRAW;

/// <summary>
/// LibRAW's <c>libraw_dnglens_t</c> (<c>lens.dng</c>): the lens geometry recorded
/// in a DNG file's <c>LensInfo</c> tag.
/// </summary>
/// <remarks>Fully modelled; the native structure holds exactly these four fields.</remarks>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawDngLens
{
    /// <summary>The minimum focal length, in millimetres (<c>MinFocal</c>).</summary>
    internal float MinFocal;

    /// <summary>The maximum focal length, in millimetres (<c>MaxFocal</c>).</summary>
    internal float MaxFocal;

    /// <summary>The maximum aperture at the minimum focal length (<c>MaxAp4MinFocal</c>).</summary>
    internal float MaxAp4MinFocal;

    /// <summary>The maximum aperture at the maximum focal length (<c>MaxAp4MaxFocal</c>).</summary>
    internal float MaxAp4MaxFocal;
}
