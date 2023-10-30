# Building PinJuke from source

## Cloning the project

Get the project and its dependencies:

```shell
git clone --recurse-submodules https://github.com/PinJuke/PinJuke.git
cd PinJuke
```

## Getting FFmpeg

Download `ffmpeg-n4.4-latest-win64-gpl-shared-4.4.zip` from https://github.com/BtbN/FFmpeg-Builds/releases and place the dll files and LICENSE.txt from the archive in `src\PinJuke\ffmpeg`.

Note: https://github.com/unosquare/ffmediaelement/issues/213#issuecomment-1120111735.

## Building projectM

Open the `Developer PowerShell for VS 2022`:

```PowerShell
cd deps\projectm
cmake . -B build -DCMAKE_BUILD_TYPE=Release "-DCMAKE_INSTALL_PREFIX=$PWD/dist" "-DCMAKE_TOOLCHAIN_FILE=$Env:VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake"
cmake --build build --target install --config Release
```
