DotNetRAW
=========

[![Build Status](https://img.shields.io/github/actions/workflow/status/macmade/DotNetRAW/ci-win.yaml?label=Windows&logo=dotnet)](https://github.com/macmade/DotNetRAW/actions/workflows/ci-win.yaml)
[![Issues](http://img.shields.io/github/issues/macmade/DotNetRAW.svg?logo=github)](https://github.com/macmade/DotNetRAW/issues)
![Status](https://img.shields.io/badge/status-active-brightgreen.svg?logo=git)
![License](https://img.shields.io/badge/license-mit-brightgreen.svg?logo=open-source-initiative)  
[![Contact](https://img.shields.io/badge/follow-@macmade-blue.svg?logo=twitter&style=social)](https://twitter.com/macmade)
[![Sponsor](https://img.shields.io/badge/sponsor-macmade-pink.svg?logo=github-sponsors&style=social)](https://github.com/sponsors/macmade)

### About

Camera RAW metadata library for C# / .NET.

This library provides a simple, read-only interface to open camera RAW files and read
their image geometry, colour calibration, lens identity and shooting metadata, along
with the raw 16-bit Bayer sensor buffer. It is a managed binding over the native
[LibRAW](https://www.libraw.org/) library, reached through P/Invoke.

### Status

DotNetRAW is deliberately **read-only**. It opens a RAW file, unpacks the sensor data,
and surfaces the metadata as immutable values and the raw Bayer buffer as a caller-owned
copy. It does **not** demosaic or render the image, decode embedded thumbnails or
previews, apply white balance, or write any format — it reads what the camera recorded
and hands it back.

### Scope & Limitations

These are intentional properties of the library, not latent surprises:

- **Read-only**: there is no demosaicing, thumbnail/preview extraction, colour
  rendering or writing. The library exposes LibRAW's parsed metadata and the raw,
  unprocessed 16-bit Bayer samples.
- **Native dependency**: unlike a pure-managed library, DotNetRAW needs the native
  LibRAW binary at run time. The matching binary is bundled and loaded automatically
  (see *Requirements*).
- **Deterministic disposal**: a `RAWFile` owns a native LibRAW context, so it is
  `IDisposable`. Dispose it — ideally with a `using` statement — to release the native
  resources promptly; a finalizer is only a safety net. Every data accessor throws
  `ObjectDisposedException` after disposal.
- **The raw buffer is unprocessed**: `RawImage` is the 16-bit Bayer mosaic exactly as
  LibRAW unpacks it (or `null` when the sensor exposes no Bayer buffer — for example a
  non-Bayer layout — or before the sensor data has been unpacked). Turning it into a
  viewable image is the caller's responsibility.
- **Culture-invariant**: every value description renders identically regardless of the
  machine's current culture.
- **Not thread-safe**: a `RAWFile` wraps a mutable native context, so an instance must
  not be used from multiple threads without external synchronization.

### Requirements

DotNetRAW targets **.NET 10**. Its one dependency is the native
[LibRAW](https://www.libraw.org/) library, version **0.22.2**, which is bundled with
the library for each supported platform — no separate install or build is required:

- **Windows** — `win-x64`
- **macOS** — Apple Silicon and Intel (a single universal binary)

The correct native binary is copied next to your application automatically and resolved
at run time by import name. The library is continuously built and tested on Windows (see
the CI badge above) in both Debug and Release configurations, and developed and tested on
macOS.

### Building

The solution (`DotNetRAW.slnx`) contains the `DotNetRAW` class library and the
`DotNetRAWTests` xUnit test suite:

```bash
dotnet build DotNetRAW.slnx -c Release
dotnet test  DotNetRAW.slnx -c Release
```

### Example Usage

#### Reading metadata

```csharp
using System;
using DotNetRAW;

try
{
    // Open a RAW file. By default the sensor data is unpacked eagerly, so any open or
    // unpack failure surfaces right here.
    using RAWFile file = new RAWFile( "/path/to/photo.cr3" );

    // A rich, one-line summary: camera, sensor dimensions, CFA, exposure and lens.
    Console.WriteLine( file );

    // Each metadata group is its own immutable record with a readable description.
    Console.WriteLine( file.ImageInfo );   // camera make, model and colour count
    Console.WriteLine( file.ImageSizes );  // raw and output geometry
    Console.WriteLine( file.CfaPattern );  // colour-filter-array layout
    Console.WriteLine( file.ShotInfo );    // ISO, shutter, aperture, focal length
    Console.WriteLine( file.LensInfo );    // lens identity and focal range

    // Some metadata is optional and may be absent.
    if( file.GpsInfo is RAWGPSInfo gps )
    {
        Console.WriteLine( gps );
    }
}
catch( RAWException exception )
{
    Console.WriteLine( exception.Message );
}
```

#### Accessing the raw sensor buffer

```csharp
using System;
using DotNetRAW;

using RAWFile file = new RAWFile( "/path/to/photo.cr3" );

// The unpacked 16-bit Bayer samples, copied into a managed array (null when the sensor
// exposes no Bayer buffer).
if( file.RawImage is ushort[] samples )
{
    Console.WriteLine( samples.Length + " raw samples" );
}

// Or read the samples in place, without copying, through a borrowed span.
ushort brightest = file.WithRawImage( span =>
{
    ushort max = 0;

    foreach( ushort sample in span )
    {
        if( sample > max )
        {
            max = sample;
        }
    }

    return max;
} );

Console.WriteLine( brightest );
```

Opening is eager by default; pass `new RAWParsingOptions( unpacksImmediately: false )` to
defer unpacking and call `RAWFile.Unpack()` yourself later. A file can also be opened from
an in-memory `byte[]` instead of a path.

License
-------

DotNetRAW is released under the terms of the MIT License.

The bundled native [LibRAW](https://www.libraw.org/) library is distributed under its own
terms (LGPL-2.1 or CDDL-1.0), which apply to that binary only.

Repository Infos
----------------

    Owner:          Jean-David Gadina - XS-Labs
    Web:            www.xs-labs.com
    Blog:           www.noxeos.com
    Twitter:        @macmade
    GitHub:         github.com/macmade
    LinkedIn:       ch.linkedin.com/in/macmade/
    StackOverflow:  stackoverflow.com/users/182676/macmade
