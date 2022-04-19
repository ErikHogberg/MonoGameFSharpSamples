module Ship

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open Tools
open Microsoft.FSharp.Collections

type Tile() =
    let mutable neighbors: List<Tile> = []
    


type SpaceShip() =
    let mutable tiles: List<Tile> = []
    
    


