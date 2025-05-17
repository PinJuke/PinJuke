![Jukebox](images/jukebox-header.webp)

- [Start](index.md)
- [Features](FEATURES.md)
- [Installation](INSTALLATION.md)
- [Setup Wizard](ONBOARDING.md)
- [Configuration](CONFIGURATION.md)
- [Theme Video Clips](THEME-VIDEOS.md)
- [Controls](CONTROLS.md)
- [Run a Playlist File](RUN.md)
- [Pinup Popper](PINUP-POPPER.md)
- [FAQ](FAQ.md)


# Configuration

The configuration of PinJuke music player is divided into a **global** configuration and a **playlist** configuration. You can have multiple **playlist** configurations, but PinJuke music player will use a single **playlist** configuration at a time. (See how to [run the app using a playlist file](RUN.md).)

## Global Configuration

The global configuration should specify information about your cabinet setup e.g. the placement and the size of your displays or the key bindings.

## Playlist Configuration

The playlist configuration should specify information about your music library e.g. the path to the root folder of your music library or background images.

## Creating the Required Configuration INI Files

The configuration is stored in ini files under the `Configs` folder.

The PinJuke Configurator UI assists you in creating and editing the configuration files. To start the Configurator, simply run `PinJuke.exe` (without any arguments).

Add at least one **playlist** configuration. If you agree with the entries you have made, click the `Save all` button at the bottom to write the ini files.

![Window of the PinJuke Configurator showing the global configuration](images/configurator-1.webp)

![Window of the PinJuke Configurator showing the playlist configuration](images/configurator-2.webp)

## Manual Editing

If you ever want to edit the configuration manually, you can do so by using a text editor like *Notepad*.

The zip archive ships with a sample ini file for the **global** and **playlist** configuration respectively.

- Copy `Templates\PinJuke.global.sample.ini` to `Configs\PinJuke.global.ini`.
- Copy `Templates\PinJuke.playlist.sample.ini` to `Configs\Playlists\PinJuke.playlist.ini` (You can freely choose the file name of the playlist e.g. `smooth-jazz.ini`).
