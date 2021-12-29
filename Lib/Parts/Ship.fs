module Ship

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open Tools
open Microsoft.FSharp.Collections

type Tile() =
    [<DefaultValue>]
    val mutable neighbors: List<Tile>
    


type SpaceShip() =
    [<DefaultValue>]
    val mutable tiles: List<Tile>
    
    


