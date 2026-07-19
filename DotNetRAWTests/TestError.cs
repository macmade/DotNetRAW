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

namespace DotNetRAWTests;

/// <summary>
/// A generic failure used by the test suite itself for its own preconditions.
/// </summary>
/// <remarks>
/// Test-only on purpose and distinct from the library's public exception type: the
/// library's exception must carry only errors the library itself emits, so the
/// fixtures and helpers signal their own failed preconditions with this dedicated
/// type instead. Its message is prefixed with <c>Test Error:</c> to set it apart at
/// a glance.
/// </remarks>
internal sealed class TestError : Exception
{
    /// <summary>
    /// Initializes a new instance with a prefixed failure message.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    private TestError( string message ) : base( $"Test Error: { message }" )
    {}

    /// <summary>
    /// Creates an error describing a failed test precondition.
    /// </summary>
    /// <param name="message">A description of the failure.</param>
    /// <returns>The created error.</returns>
    internal static TestError Failure( string message ) => new TestError( message );
}
