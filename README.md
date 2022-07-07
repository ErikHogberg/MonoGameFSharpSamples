# MonoGame F# Samples
Some examples of using F# to write MonoGame projects.
A small winter holiday hobby project to learn F#.

## Current state

It currently has a setup of 3 projects: a desktop project, an untested C# android project (which I will try to replace with a F# project later), and a shared lib with the actual game code.

The recommended starting point for reading the game code is the shared library (./Lib/Library.fs).

At the moment the project only shows an asteroid shower using MonoGame.Extended entities based on the [rain example](https://www.monogameextended.net/docs/features/entities/entities/#example).
Use number keys to switch between "game screens".
Press space to toggle rendering of asteroids outside of its collision boundary, press esc to close the program.

The aim is to eventually create a few simplified game clones, such the spaceship battle gameplay from FTL, and a Touhou-style bullet hell game.

## How to build/run

Install [dotnet 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

From a terminal:

Install the Monogame Content Builder using ``` dotnet tool install -g dotnet-mgcb ``` and then run ``` mgcb Content.mgcb ``` from the ./Lib/Content/ directory.

Run ``` dotnet run ``` with the Desktop folder as the working directory.

Tested on Windows and Linux.

## How to dev

Install [Visual Studio Code](https://code.visualstudio.com/Download).

Install the [Ionide-fsharp](https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp) extension.

## Credits

| Resource | License | Source |
|----------|---------|--------|
|./Lib/Content/tiled/iso-64x64-building.png    |   CC BY 3.0 license   |   https://opengameart.org/content/isometric-64x64-medieval-building-tileset |
|./Lib/Content/tiled/iso-64x64-outside.png     |   CC BY 3.0 license   |   https://opengameart.org/content/isometric-64x64-outside-tileset |
