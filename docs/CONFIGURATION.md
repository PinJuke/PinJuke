![Jukebox](images/jukebox-header.png)

- [Start](index.md)
- [Features](FEATURES.md)
- [Installation](INSTALLATION.md)
- [Configuration](CONFIGURATION.md)
- [Pinup Popper](PINUP-POPPER.md)
- [FAQ](FAQ.md)

# Configuration

The configuration is done via ini files. Ini files can be edited with text editors like *Notepad*.

The configuration is split into a **global** configuration ini file and a **playlist** configuration ini file. You can have multiple **playlist** configuration ini files, but PinJuke will use a single **playlist** configuration ini file at a time. The path to **playlist** configuration ini file gets passed to the application for example `PinJuke.exe Configs\Playlists\PinJuke.playlist.smooth-jazz.ini`. The **global** configuration ini file is always read from the path `Configs\PinJuke.global.ini`.

## Creating the configuration ini files

The zip archive ships with a sample ini file for **global** and **playlist** configuration respectively.

- Copy `Configs\Samples\PinJuke.global.sample.ini` to `Configs\PinJuke.global.ini`.
- Copy `Configs\Samples\PinJuke.playlist.sample.ini` to `Configs\Playlists\PinJuke.playlist.ini` (You can freely choose the file name e.g. `PinJuke.playlist.smooth-jazz.ini`).

## Global configuration

The global configuration should specify information about your cabinet setup e.g. the placement and the size of your displays or the key bindings.

## Playlist configuration

The playlist configuration should specify information about your music library e.g. the path to the root folder of your music library or background images.
