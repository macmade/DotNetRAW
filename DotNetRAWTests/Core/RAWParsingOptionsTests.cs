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

using DotNetRAW;

namespace DotNetRAWTests;

/// <summary>
/// Unit tests for <see cref="RAWParsingOptions"/>.
/// </summary>
public class RAWParsingOptionsTests
{
    /// <summary>
    /// The description reflects whether unpacking is eager or lazy.
    /// </summary>
    [ Fact ]
    public void DescriptionReflectsUnpackMode()
    {
        Assert.Equal( "eager unpack", new RAWParsingOptions( true ).ToString() );
        Assert.Equal( "lazy unpack",  new RAWParsingOptions( false ).ToString() );
    }

    /// <summary>
    /// The default options and the parameterless constructor both unpack eagerly.
    /// </summary>
    [ Fact ]
    public void DefaultsToEagerUnpacking()
    {
        Assert.True( RAWParsingOptions.Default.UnpacksImmediately );
        Assert.True( new RAWParsingOptions().UnpacksImmediately );
        Assert.Equal( "eager unpack", RAWParsingOptions.Default.ToString() );
    }

    /// <summary>
    /// Two option sets compare equal when their unpack mode matches.
    /// </summary>
    [ Fact ]
    public void EqualityComparesUnpackMode()
    {
        Assert.Equal( new RAWParsingOptions( true ), RAWParsingOptions.Default );
        Assert.NotEqual( new RAWParsingOptions( false ), new RAWParsingOptions( true ) );
    }
}
