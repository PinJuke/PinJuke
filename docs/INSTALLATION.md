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


# Installation

## Download

Head over to the [releases page](https://github.com/PinJuke/PinJuke/releases) to download the latest *application*.

You may also want to download the
- [PinJuke Milkdrop assets](https://github.com/PinJuke/PinJuke-Milkdrop/releases) for the visualizer
- [PinJuke Media assets](https://github.com/PinJuke/PinJuke-Media/releases) containing some [theme videos](THEME-VIDEOS.md)

## Extract the Archives

The zip archives contain a top level `PinJuke` folder so you can extract the files for example to `C:\vPinball` (the default location of the *PinUP Popper Baller Installer*).

If you extract the zip archive using the archiver software integrated into Windows, you will likely get a prompt when trying to run the application:

![Jukebox](images/windows-protection.png)

To solve this, open the context menu by right-clicking `PinJuke.exe`, then select `Properties`. Here, you can remove the execution block by checking the box labeled `Unblock` and clicking OK:

![Jukebox](images/windows-unblock.png)

## Install the .NET Runtime

If you run PinJuke for the first time, you will likely need to install the .NET runtime. A message box may appear, directing you to Microsoft's .NET download website:

![Jukebox](images/installation-dot-net.png)
