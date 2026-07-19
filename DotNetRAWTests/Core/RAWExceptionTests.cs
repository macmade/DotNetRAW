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
using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="RAWException"/>.
/// </summary>
public class RAWExceptionTests
{
    /// <summary>
    /// Every factory sets the matching <see cref="RAWErrorKind"/>, prefixes the
    /// message with <c>RAW Error:</c>, and embeds its payload.
    /// </summary>
    [ Fact ]
    public void FactoriesProduceTheExpectedKindPrefixAndPayload()
    {
        ( RAWException Error, RAWErrorKind Kind, string Payload )[] cases =
        [
            ( RAWException.InvalidFileURL( "/tmp/photo.cr2" ), RAWErrorKind.InvalidFileURL,    "/tmp/photo.cr2"         ),
            ( RAWException.CannotReadFile( "/tmp/photo.cr2" ), RAWErrorKind.CannotReadFile,    "/tmp/photo.cr2"         ),
            ( RAWException.OpenFailed( -2, "unsupported" ),    RAWErrorKind.OpenFailed,        "unsupported"            ),
            ( RAWException.UnpackFailed( -9, "io error" ),     RAWErrorKind.UnpackFailed,      "io error"               ),
            ( RAWException.UnsupportedFormat(),                RAWErrorKind.UnsupportedFormat, "Unsupported RAW format" ),
            ( RAWException.LibRawError( -1, "unspecified" ),   RAWErrorKind.LibRawError,       "unspecified"            ),
        ];

        foreach( ( RAWException error, RAWErrorKind kind, string payload ) in cases )
        {
            Assert.Equal( kind, error.Kind );
            Assert.StartsWith( "RAW Error:", error.Message, StringComparison.Ordinal );
            Assert.Contains( payload, error.Message, StringComparison.Ordinal );
        }
    }

    /// <summary>
    /// Each factory renders its full message exactly, pinning the template text so
    /// a stray reword, spacing or ordering change is caught.
    /// </summary>
    [ Fact ]
    public void MessagesRenderExactly()
    {
        Assert.Equal( "RAW Error: Invalid file URL: /tmp/photo.cr2",            RAWException.InvalidFileURL( "/tmp/photo.cr2" ).Message );
        Assert.Equal( "RAW Error: Cannot read file: /tmp/photo.cr2",            RAWException.CannotReadFile( "/tmp/photo.cr2" ).Message );
        Assert.Equal( "RAW Error: Failed to open the RAW file (-2): unsupported", RAWException.OpenFailed( -2, "unsupported" ).Message );
        Assert.Equal( "RAW Error: Failed to unpack the RAW data (-9): io error",  RAWException.UnpackFailed( -9, "io error" ).Message );
        Assert.Equal( "RAW Error: Unsupported RAW format",                       RAWException.UnsupportedFormat().Message );
        Assert.Equal( "RAW Error: LibRAW error (-1): unspecified",               RAWException.LibRawError( -1, "unspecified" ).Message );
    }

    /// <summary>
    /// A LibRAW error exposes its status code and embeds both the code and the
    /// LibRAW message in the description.
    /// </summary>
    [ Fact ]
    public void DescriptionIncludesCodeAndMessage()
    {
        RAWException error = RAWException.OpenFailed( -2, "Unsupported file format" );

        Assert.Equal( -2, error.Code );
        Assert.Contains( "-2", error.Message, StringComparison.Ordinal );
        Assert.Contains( "Unsupported file format", error.Message, StringComparison.Ordinal );
    }

    /// <summary>
    /// Errors raised before LibRAW is involved carry no status code.
    /// </summary>
    [ Fact ]
    public void CodeIsNullForErrorsRaisedBeforeLibRaw()
    {
        Assert.Null( RAWException.InvalidFileURL( "/tmp/photo.cr2" ).Code );
        Assert.Null( RAWException.CannotReadFile( "/tmp/photo.cr2" ).Code );
        Assert.Null( RAWException.UnsupportedFormat().Code );
    }

    /// <summary>
    /// The status code in a message is formatted with the invariant culture, so it
    /// never picks up culture-specific digit grouping under a non-invariant culture.
    /// </summary>
    [ Fact ]
    public void CodeInMessageIsCultureInvariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo( "fr-FR" );

            RAWException error = RAWException.LibRawError( 123456, "boom" );

            Assert.Contains( "123456", error.Message, StringComparison.Ordinal );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
