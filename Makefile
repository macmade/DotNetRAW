#-------------------------------------------------------------------------------
# Copyright (c) 2026, Jean-David Gadina - www.xs-labs.com
# Distributed under the terms of the MIT License.
#
# Builds the DotNetRAW class library, runs the xUnit test suite and produces the
# NuGet package. The .NET toolchain is cross-platform, so every target runs
# unchanged on macOS, Linux and Windows.
#
# Unlike a pure-managed package, the DotNetRAW package carries the native LibRAW
# binaries it binds through P/Invoke - libraw.dll for win-x64 and a universal
# (arm64 + x86_64) libraw.dylib for macOS - under runtimes/<rid>/native/, so a
# consumer gets the correct native resolved automatically with no build from
# source. The binaries are committed under runtimes/ and packed by the csproj.
#
# The .NET SDK is used through the `DOTNET` variable, which prefers a `dotnet`
# found on `PATH` and otherwise falls back to the default macOS install location.
# Override it on the command line if your SDK lives elsewhere (`make DOTNET=...`).
#-------------------------------------------------------------------------------

DIR_ROOT            := $(realpath .)/
DIR_BUILD           := $(DIR_ROOT)Build/

SOLUTION            := DotNetRAW.slnx
PROJECT             := DotNetRAW/DotNetRAW.csproj

CONFIGURATION       := Release

DOTNET              := $(shell command -v dotnet || echo /usr/local/share/dotnet/dotnet)

.PHONY: all build test pack clean

all: build test

	@:

build:

	@$(DOTNET) build $(SOLUTION) -c $(CONFIGURATION)

test:

	@$(DOTNET) test $(SOLUTION) -c $(CONFIGURATION)

pack:

	@mkdir -p $(DIR_BUILD)
	@$(DOTNET) pack $(PROJECT) -c $(CONFIGURATION) -o $(DIR_BUILD)

clean:

	@$(DOTNET) clean $(SOLUTION) -c $(CONFIGURATION)
	@rm -f $(DIR_BUILD)*.nupkg $(DIR_BUILD)*.snupkg
