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

// calculates an approximation of 1/sqrt(x)
// src: https://github.com/CartBlanche/MonoGame-Samples/blob/master/FarseerPhysicsEngine/Common/Math.cs#L149-L163
let InverseSqrt(x:float32) =
    let mutable convert = FloatConverter()
    convert.x <- x
    let xhalf = 0.5f * x
    convert.i <- 0x5f3759df - (convert.i >>> 1)
    let x2 = convert.x
    x2 * (1.5f - xhalf * x2 * x2)


type Point with
    member this.ToPoint2 = Point2(float32 this.X, float32 this.Y)
type Point2 with
    member this.ToVector = Vector2(this.X, this.Y)
    member this.ToSize = Size2(this.X, this.Y)
    member this.Inverse = Point2.Zero - this
type Size2 with
    member this.ToVector = Vector2(this.Width, this.Height)
    member this.ToPoint2 = Point2(this.Width, this.Height)
    static member Square size = Size2(size, size)

type Vector2 with
    member this.ToPoint2 = Point2(this.X, this.Y)

    // fast approximation of normalized vector
    member this.FastNormalizedCopy () =
        this * InverseSqrt(this.X * this.X + this.Y * this.Y )

    // fast approximation of vector magnitude
    member this.FastLength() =
        1f/InverseSqrt(this.X * this.X + this.Y * this.Y )
        

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
    
    member this.RotateTowards (target: Vector2) maxDistance =
        let magnitude = this.FastLength()
        if maxDistance > 0f && (this - target).FastLength() < maxDistance then
            target 
        else if maxDistance < 0f && (this - (-target)).FastLength() < -maxDistance then
            -target 
        else
            // (this.MoveTowards(target, maxDistance)).FastNormalizedCopy() * magnitude
            if Vector2.Dot(target.Rotate90Clockwise().FastNormalizedCopy(), this.FastNormalizedCopy()) > 0f then
                (this.MoveTowards(this.Rotate90CounterClockwise(), maxDistance)).NormalizedCopy() * magnitude
            else
                (this.MoveTowards(this.Rotate90Clockwise(), maxDistance)).NormalizedCopy() * magnitude
    
    // version that doesn't bother with restricting rotating past target
    member this.FasterRotateTowards (target: Vector2) maxDistance =
        let magnitude = this.FastLength()
        if Vector2.Dot(target.Rotate90Clockwise().FastNormalizedCopy(), this.FastNormalizedCopy()) > 0f then
            (this.MoveTowards(this.Rotate90CounterClockwise(), maxDistance)).NormalizedCopy() * magnitude
        else
            (this.MoveTowards(this.Rotate90Clockwise(), maxDistance)).NormalizedCopy() * magnitude

    member this.MoveTowards (target: Vector2, maxDistance, emergencyDir: Vector2) =
        if maxDistance < 0f && this = target then
            emergencyDir.FastNormalizedCopy() * maxDistance
        else
            this.MoveTowards (target, maxDistance)
        

