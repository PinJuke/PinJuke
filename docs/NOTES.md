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


# Theme Video Clips

## Rotate and encode videos

* https://stackoverflow.com/questions/3937387/rotating-videos-with-ffmpeg
* https://unix.stackexchange.com/questions/28803/how-can-i-reduce-a-videos-size-with-ffmpeg
* https://superuser.com/questions/268985/remove-audio-from-video-file-with-ffmpeg

```Shell
./ffmpeg -i "Turntable Start.mp4" -vf "transpose=1" -an -vcodec libx264 -crf 28 "Turntable Start New.mp4"
```

## Create idle video

```Shell
ffmpeg -i "Turntable Start.mp4" -frames:v 1 -q:v 2 first_frame.jpg
ffmpeg -loop 1 -i first_frame.jpg -c:v libx264 -t 2 -pix_fmt yuv420p -vf "fps=25" output.mp4
```
