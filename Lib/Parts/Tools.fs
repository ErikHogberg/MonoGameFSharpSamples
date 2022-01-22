module Tools

open System
open Microsoft.Xna.Framework
open MonoGame.Extended

module Singleton =
    open Microsoft.Xna.Framework.Graphics

    type Product internal () =
        let mutable textures: Map<string, Texture2D> = Map []

        member this.Set(key: string, value: Texture2D) =
            textures <- textures.Add(key, value)

        member this.Get(key: string) =
            textures[ key ]


    let Instance = Product()

type Point2 with
    member this.ToVector() = Vector2(this.X, this.Y)

type Vector2 with
    member this.MoveTowards (target: Vector2) (maxDistance: float32) =
        if maxDistance > 0f && (maxDistance **2f)* maxDistance > (target - this).NormalizedCopy().LengthSquared() then
            target
        else
            // FIXME: breaks with negative maxDistance
            let deltaDir =  (target - this)
            (this + (deltaDir.NormalizedCopy() * maxDistance))
    member this.RotateTowards (target: Vector2) (maxDistance: float32) =
        (this.MoveTowards target maxDistance).NormalizedCopy()


type TransformCollisionActor
    (
        transform: Transform2,
        radius: float32,
        onCollision: Collisions.CollisionEventArgs -> unit,
        data
    ) =
    let transform = transform
    let radius = radius
    let data = data

    member this.Data = data
    member this.Transform = transform

    interface Collisions.ICollisionActor with
        member this.Bounds = CircleF(transform.Position, radius)
        member this.OnCollision(args) = onCollision (args)

