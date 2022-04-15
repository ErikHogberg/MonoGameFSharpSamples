namespace Danmaku

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems

open Tools

// IDEA: implement component interface
type Player(maxVelocity: float32, startPos: Vector2)=
    let mutable hp = 100f
    
    let maxVelocity = maxVelocity
    let mutable velocity = Vector2.Zero
    let mutable speed = 1f

    let transform = Transform2(startPos) 

    let capVelocity() =
        if(velocity.LengthSquared() > maxVelocity**2f) then
            velocity <- velocity.FastNormalizedCopy() * maxVelocity
        ()

    member this.HP with get () = hp and set (value) = hp <-value
    member this.CurrentVelocity with get () = velocity and set (value) = velocity <- value
    member this.Speed with get () = speed and set (value) = speed <- value
    member this.Transform with get () = transform
    
    member this.SetVelocity (newVelocity: Vector2) = 
        velocity <- newVelocity
        capVelocity()
        
    member this.SetVelocity (newVelocityMagnitude: float32) = 
        velocity <- velocity.FastNormalizedCopy() * newVelocityMagnitude
        capVelocity()

    member this.ConstrainToFrame (frame: RectangleF) =
        if not (frame.Contains(transform.Position.ToPoint2())) then
            transform.Position <- frame.ClosestPointTo(transform.Position.ToPoint2()).ToVector()
        ()

    member this.Update (gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        transform.Position <- transform.Position + this.CurrentVelocity* speed * dt
        ()

    member this.Draw (spriteBatch: SpriteBatch) (gameTime: GameTime) =
        spriteBatch.DrawCircle(transform.Position, 15f, 8, Color.GreenYellow, 5f)
        ()
    