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
/// LibRAW's <c>libraw_shootinginfo_t</c> (<c>shootinginfo</c>). Of its fields the
/// port reads only <see cref="BodySerial"/>; the rest are modelled to preserve
/// the layout.
/// </summary>
[ StructLayout( LayoutKind.Sequential ) ]
internal struct LibRawShootingInfo
{
    /// <summary>The drive mode (<c>DriveMode</c>). Unused.</summary>
    internal short DriveMode;

    /// <summary>The focus mode (<c>FocusMode</c>). Unused.</summary>
    internal short FocusMode;

    /// <summary>The metering mode (<c>MeteringMode</c>). Unused.</summary>
    internal short MeteringMode;

    /// <summary>The active AF point (<c>AFPoint</c>). Unused.</summary>
    internal short AFPoint;

    /// <summary>The exposure mode (<c>ExposureMode</c>). Unused.</summary>
    internal short ExposureMode;

    /// <summary>The exposure program (<c>ExposureProgram</c>). Unused.</summary>
    internal short ExposureProgram;

    /// <summary>The image-stabilization mode (<c>ImageStabilization</c>). Unused.</summary>
    internal short ImageStabilization;

    /// <summary>The camera body serial number, NUL-terminated (<c>BodySerial</c>).</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] BodySerial;

    /// <summary>The internal body serial number, NUL-terminated (<c>InternalBodySerial</c>). Unused.</summary>
    [ MarshalAs( UnmanagedType.ByValArray, SizeConst = 64 ) ]
    internal byte[] InternalBodySerial;
}
