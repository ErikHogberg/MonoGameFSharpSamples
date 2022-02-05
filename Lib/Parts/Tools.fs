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
    member this.MoveTowards (target: Vector2, maxDistance) =        
        if this = target || maxDistance > 0f && maxDistance * maxDistance > (target - this).LengthSquared() then
            target
        else
            let deltaDir = target - this
            (this + (Vector2.Normalize( deltaDir) * maxDistance))
    
    // TODO: make rotation distance more consistent
    // IDEA: translate target argument value to position perpendicular to direction to origin from this vector, keeping magnitude of delta between target and this vector. try not using sqrt
    member this.RotateTowards (target: Vector2) maxDistance =
        let magnitude = this.Length()
        if maxDistance > 0f && this = target then
            target 
        else
            (Vector2.Normalize(this.MoveTowards(target, maxDistance))) * magnitude

    member this.MoveTowards (target: Vector2, maxDistance, emergencyDir: Vector2) =
        if maxDistance < 0f && this = target then
            emergencyDir.NormalizedCopy() * maxDistance
        else
            this.MoveTowards( target, maxDistance)
        


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

