namespace Danmaku

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

open MonoGame.Extended
open MonoGame.Extended.Input.InputListeners
open MonoGame.Extended.Entities
open MonoGame.Extended.Sprites
open MonoGame.Extended.ViewportAdapters
open MonoGame.Extended.Screens
open MonoGame.Extended.Screens.Transitions
open MonoGame.Extended.Tweening

open RenderSystem
open GameScreenWithComponents
open Asteroids
open Boids
open Bullets
open Tools

type TestGame1 (game, graphics) =
    inherit GameScreenWithComponents (game, graphics)

    let mutable dot: Texture2D = null
    let mutable fira: SpriteFont = null

    let mutable camera: OrthographicCamera = null
    let mutable boids1: BoidsSystem = Unchecked.defaultof<BoidsSystem>

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let boidsTarget = CircleF(Point2(1300f, 600f), 10f)

    let mutable spawnAngle = 0f

    
    override this.Initialize() =
        
        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera(viewportAdapter)

        
        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        dot <- this.Content.Load "1px"
        fira <- this.Content.Load "Fira Code"

        boids1 <- new BoidsSystem(EllipseF(boidsTarget.Center.ToVector, 300f, 450f))
        boids1.Target <- boidsTarget


        let world =
            WorldBuilder()
                
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, camera))

                .AddSystem(boids1)
                // .AddSystem(new BoidsRenderSystem(this.GraphicsDevice, camera))

                // .AddSystem(new EnemyBulletSystem(Transform2(300f,150f,0f,0f,0f), playerBoundaries))
                .AddSystem(new DotRenderSystem(this.GraphicsDevice, camera))

                .Build()

        this.Components.Add(world)


        base.LoadContent()
        ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // TODO: tweener component or entity?

        spawnAngle <- (spawnAngle + dt * 0.15f) % (MathF.PI*2f)
        boids1.SpawnAngle <-  spawnAngle

        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.PaleVioletRed

        spriteBatch.Begin(transformMatrix = camera.GetViewMatrix())

        spriteBatch.FillRectangle(0f,0f,1080f,1920f, Color.DarkCyan)

        spriteBatch.DrawCircle(boidsTarget, 12, Color.Chartreuse, 5f)

        let pointOnBoundary = boids1.PointOnBoundary
        
        spriteBatch.DrawCircle(pointOnBoundary, 5f, 12, Color.Black)
        let rect = boids1.SpawnRange()
        let topleft = (Vector2(rect.TopLeft.X, rect.TopLeft.Y)).Rotate(boids1.SpawnAngle) + pointOnBoundary
        let topright= (Vector2(rect.TopRight.X, rect.TopRight.Y)).Rotate(boids1.SpawnAngle)  + pointOnBoundary
        let bottomleft = (Vector2(rect.BottomLeft.X, rect.BottomLeft.Y) ).Rotate(boids1.SpawnAngle) + pointOnBoundary
        let bottomright = (Vector2(rect.BottomRight.X, rect.BottomRight.Y)).Rotate(boids1.SpawnAngle) + pointOnBoundary

        let thickness = 0.3f
        spriteBatch.DrawLine(topleft, topright, Color.Brown, thickness)
        spriteBatch.DrawLine(topleft, bottomleft, Color.Brown, thickness)
        spriteBatch.DrawLine(bottomright, topright, Color.Brown, thickness)
        spriteBatch.DrawLine(bottomright, bottomleft, Color.Brown, thickness)

        // player.Draw spriteBatch gameTime


        spriteBatch.DrawString(fira, "boids", Vector2(600f, 100f), Color.WhiteSmoke);

        spriteBatch.End()

        base.Draw gameTime
        ()

