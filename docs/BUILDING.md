![Jukebox](images/jukebox-header.png)

- [Start](index.md)
- [Features](FEATURES.md)
- [Installation](INSTALLATION.md)
- [Configuration](CONFIGURATION.md)
- [Theme videos](THEME-VIDEOS.md)
- [Run a playlist file](RUN.md)
- [Controls](CONTROLS.md)
- [Pinup Popper](PINUP-POPPER.md)
- [FAQ](FAQ.md)

# Building PinJuke music player from source

## Cloning the project

Get the project and its dependencies:

```shell
git clone --recurse-submodules https://github.com/PinJuke/PinJuke.git
cd PinJuke
```

## Getting FFmpeg

Download the zip archive `ffmpeg-7.0.2-full_build-shared.zip` from https://github.com/GyanD/codexffmpeg/releases/tag/7.0.2 and place the dll files and LICENSE.txt from the archive in `src\PinJuke\ffmpeg`.

## Building projectM

Open the `Developer PowerShell for VS 2022`:

```PowerShell
cd deps\projectm
cmake . -B build -DCMAKE_BUILD_TYPE=Release "-DCMAKE_INSTALL_PREFIX=$PWD/dist" "-DCMAKE_TOOLCHAIN_FILE=$Env:VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake"
cmake --build build --target install --config Release -j $env:NUMBER_OF_PROCESSORS
```
