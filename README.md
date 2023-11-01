# SteamWorkshopUploader
This is a tool that uploads mods for GZDoom games to the Steam Workshop. With a few changes, it could be used for other games too.

`SWUploaderNet50` targets `.NET 5.0` and uses [Facepunch.Steamworks](https://github.com/Facepunch/Facepunch.Steamworks).

`SWUploaderNet35` targets `.NET Framework 3.5` and uses [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET).

To build the solution, you need to clone the two libraries mentioned above so they are parallel to this repo. Example:
```
C:\Code
	|__SteamWorkshopUploader
	|__Facepunch.Steamworks
	|__Steamworks.NET
```
Then open `Steamworks.NET\Standalone\Steamworks.NET.csproj` in a text editor and change `<TargetFrameworkVersion>` to `v3.5`.
Optionally you can change `<DebugType>` to `none` for all build configurations.
Finally, edit properties for `Facepunch.Steamworks\Facepunch.Steamworks\Facepunch.Steamworks.Win64.csproj` in Visual Studio, and add `net5.0-windows` to the list of `Target frameworks`.
You can also scroll down and set `Debug symbols` to none, and turn off API documentation generation.
After building the Net5.0 project for release, publish to folder with the included profile.
