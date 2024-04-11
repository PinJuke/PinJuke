# Building PinJuke from source

## Cloning the project

Get the project and its dependencies:

```shell
git clone --recurse-submodules https://github.com/PinJuke/PinJuke.git
cd PinJuke
```

## Getting FFmpeg

Download the zip archive from https://github.com/PinJuke/FFmpeg-Builds/releases and place the dll files and LICENSE.txt from the archive in `src\PinJuke\ffmpeg`.

## Building projectM

Open the `Developer PowerShell for VS 2022`:

```PowerShell
cd deps\projectm
cmake . -B build -DCMAKE_BUILD_TYPE=Release "-DCMAKE_INSTALL_PREFIX=$PWD/dist" "-DCMAKE_TOOLCHAIN_FILE=$Env:VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake"
cmake --build build --target install --config Release
```
