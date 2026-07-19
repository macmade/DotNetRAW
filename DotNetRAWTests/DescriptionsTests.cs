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
/// Fixture-driven tests for the rich <see cref="RAWFile"/> description and for the
/// fixture-discovery infrastructure itself. The per-type value descriptions are pinned
/// by each type's own unit tests; these exercise the parts that only make sense against
/// real RAW samples.
/// </summary>
public class DescriptionsTests
{
    /// <summary>
    /// The rich file description is non-empty and includes the sensor dimensions - so it
    /// always carries the <c>×</c> separator - for real samples.
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [ Theory ]
    [ MemberData( nameof( TestUtilities.RawFiles ), MemberType = typeof( TestUtilities ) ) ]
    public void FileDescriptionIsRich( string path )
    {
        using RAWFile file        = new RAWFile( path );
        string        description = file.ToString();

        Assert.False( string.IsNullOrEmpty( description ) );
        Assert.Contains( "×", description );
    }

    /// <summary>
    /// The <see cref="TestUtilities.HasTestFiles"/> flag is consistent with the discovered
    /// fixtures, whether or not any are present.
    /// </summary>
    [ Fact ]
    public void FixtureDiscoveryIsConsistent()
    {
        Assert.Equal( TestUtilities.RawFileUrls.Count > 0, TestUtilities.HasTestFiles );
    }
}
