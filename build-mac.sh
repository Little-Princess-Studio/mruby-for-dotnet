#!/bin/sh

if [ "$1" = "clean" ]; then
    echo "Cleaning mruby and mruby-wrapper build cache"
    cd mruby
    rake clean

    cd ../mruby-shared
    xmake clean

    cd ..
    exit
fi

echo "Building mruby and mruby-wrapper for Windows"

echo "Building mruby"
sh ./build-mruby-mac.sh

echo "Building mruby-shared"
cd mruby-shared
xmake f -m release
xmake

echo "Building mruby-wrapper"
cd ../mruby-wrapper
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet pack --configuration Release

cd ../

echo "Build successful"
