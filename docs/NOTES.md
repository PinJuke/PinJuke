![Jukebox](images/jukebox-header.png)

- [Start](index.md)
- [Features](FEATURES.md)
- [Installation](INSTALLATION.md)
- [Configuration](CONFIGURATION.md)
- [Pinup Popper](PINUP-POPPER.md)
- [FAQ](FAQ.md)

# Theme videos

## Rotate and encode videos

* https://stackoverflow.com/questions/3937387/rotating-videos-with-ffmpeg
* https://unix.stackexchange.com/questions/28803/how-can-i-reduce-a-videos-size-with-ffmpeg
* https://superuser.com/questions/268985/remove-audio-from-video-file-with-ffmpeg

```Shell
./ffmpeg -i "Turntable Start.mp4" -vf "transpose=1" -an -vcodec libx265 -crf 28 "Turntable Start New.mp4"
```
