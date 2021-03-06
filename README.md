![License](https://img.shields.io/badge/license-MIT-blue.svg) [![Actions Status](https://github.com/arakis/gcaliper/workflows/Build/badge.svg)](https://github.com/arakis/gcaliper/actions)

gCaliper
========

![screenshot](media/screenshot.png)

gCaliper is a screen caliper to measure pixel distances, even with variable rotation. The theme is customizable. Windows, Linux and Mac.

Download
========

You can download a build here:
https://github.com/arakis/gcaliper/releases/latest

Extract the archive. The executable is called `gcaliper.exe`

Build
=====

Clone the repository:
```
git clone https://github.com/Arakis/gcaliper.git
cd gcaliper
```

You need the [.NET5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0). Run within the `src` directory:
```
dotnet build
```

If you have any suggestions, please let me know.

Contribution
============
In the moment i'm looking for help to create a DEB and RPM package.

Documentation
=============

* Use `Arrow keys` to move
* Use `Control+Arrow` keys to resize
* Use `Shift` for bigger steps
* You can rotate with Mouse or with `R` and `T` key.
* Use `Control` while rotation for variable angle
* Use `Shift` while rotation to snap to 45° steps
* Use `V` key and `H` key to switch between horizontal and vertical angle.
* `Right click` for context menu
* `N` key minimizes the application
* `Home` key sets the distance to 0 and `End` key sets the distance to nearly to screen dimension
* `C` key changes jaw color
* `Control+Q` or `Control+W` quits the application
