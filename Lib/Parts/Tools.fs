module Tools

open System
open System.Diagnostics
open System.Runtime.InteropServices
open Microsoft.Xna.Framework
open MonoGame.Extended

// src: https://github.com/CartBlanche/MonoGame-Samples/blob/master/FarseerPhysicsEngine/Common/Math.cs#L284-L291
#nowarn "9"
[<StructLayout(LayoutKind.Explicit)>]
type FloatConverter =
    struct
        [<FieldOffset(0)>]
        val mutable x:float32;
        [<FieldOffset(0)>]
        val mutable i: int;
    end

// src: https://github.com/CartBlanche/MonoGame-Samples/blob/master/FarseerPhysicsEngine/Common/Math.cs#L149-L163
let InverseSqrt(x:float32) =
    let mutable convert = FloatConverter()
    convert.x <- x
    let xhalf = 0.5f * x
    convert.i <- 0x5f3759df - (convert.i >>> 1)
    let x2 = convert.x
    x2 * (1.5f - xhalf * x2 * x2)


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

    member this.FastNormalizedCopy () =
        let inverseSqrt = InverseSqrt(this.X * this.X + this.Y * this.Y )
        this * inverseSqrt

    member this.Rotate90Clockwise () =
        Vector2(this.Y, -this.X)
    member this.Rotate90CounterClockwise () =
        Vector2(-this.Y, this.X)

    member this.MoveTowards (target: Vector2, maxDistance) =        
        if this = target || maxDistance > 0f && maxDistance * maxDistance > (target - this).LengthSquared() then
            target
        else
            let deltaDir = target - this
            (this +  deltaDir.FastNormalizedCopy() * maxDistance)
    
    // TODO: make rotation distance more consistent
    // IDEA: translate target argument value to position perpendicular to direction to origin from this vector, keeping magnitude of delta between target and this vector. try not using sqrt
    member this.RotateTowards (target: Vector2) maxDistance =
        let magnitude = this.Length()
        if maxDistance > 0f && this = target then
            target 
        else
            // (this.MoveTowards(target, maxDistance)).FastNormalizedCopy() * magnitude
            if Vector2.Dot(target.Rotate90Clockwise().FastNormalizedCopy(), this.FastNormalizedCopy()) > 0f then
                (this.MoveTowards(this.Rotate90CounterClockwise(), maxDistance)).NormalizedCopy() * magnitude
            else
                (this.MoveTowards(this.Rotate90Clockwise(), maxDistance)).NormalizedCopy() * magnitude

    member this.MoveTowards (target: Vector2, maxDistance, emergencyDir: Vector2) =
        if maxDistance < 0f && this = target then
            emergencyDir.FastNormalizedCopy() * maxDistance
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

