# MonoGame F# Samples
Some examples of using F# to write MonoGame projects.
A small winter holiday hobby project to learn F#.

## Current state

Has a setup of 3 projects: a desktop project, an untested C# android project (which I will try to replace with a F# project later), and a shared lib with the actual game code.

The recommended starting point for reading the game code is the shared library (./Lib/Lib.fs).

At the moment the project shows an asteroid shower using MonoGame.Extended entities based on the [rain example](https://www.monogameextended.net/docs/features/entities/entities/#example), as well as a boids example based on the same rain example.
Press space to toggle rendering of asteroids outside of its collision boundary, press esc to close the program.

The aim is to eventually create a few simplified game clones, such as clones of FTL and Touhou.

## How to build/run

Install [dotnet 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Run ``` dotnet run ``` in terminal.

## How to dev

Install [Visual Studio Code](https://code.visualstudio.com/Download).

Install the [Ionide-fsharp](https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp) extension.

