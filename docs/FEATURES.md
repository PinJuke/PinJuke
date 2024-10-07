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

# Features

This open source software adds a media player to your virtual pinball cabinet, featuring:
- An animated, GPU-accelerated (WPF) user interface that supports scaling and rotation according to your displays (Playfield, Backglass, DMD)
- Playback of media thanks to the FFmpeg library
  - Plays file types: music (mp3), videos (mp4), playlists (m3u)
- Milkdrop visualizations thanks to [projectM - The most advanced open-source music visualizer](https://github.com/projectM-visualizer/projectm)
- "Theme videos" play according to track changes
- User-friendly file browser
- Rich configuration options
  - The configuration splits into one global configuration ini file and one or more playlist configuration ini files
  - The PinJuke Configurator provides an UI, so no fiddling via text editor is necessary
- Utilizes DOF (DirectOutputFramework)
- Localized in English and German
