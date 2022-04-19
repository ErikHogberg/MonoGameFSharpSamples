namespace Danmaku

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems

open Tools

type Player()=
    let mutable hp = 100f
    
    member this.HP with get () = hp and set (value) = hp <-value
    
    