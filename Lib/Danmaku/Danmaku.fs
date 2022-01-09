module Danmaku

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
open UpdateSystem
open GameScreenWithComponents
open Asteroids
open Boids

type DanmakuGame(game: Game) =
    inherit GameScreenWithComponents(game)


    [<DefaultValue>]
    val mutable dot: Texture2D
    
    [<DefaultValue>]
    val mutable camera: OrthographicCamera

    [<DefaultValue>]
    val mutable asteroids1: AsteroidShowerSystem
    [<DefaultValue>]
    val mutable asteroidsRenderSystem: AsteroidRenderSystem

    [<DefaultValue>]
    val mutable boids1: BoidsSystem

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let box = RectangleF(600f, 200f, 50f,80f)
    let bubble = EllipseF(Vector2(600f, 400f), 50f,80f)

    let boidsTarget = CircleF(Vector2(1300f, 600f), 0.7f)

    let mutable asteroidAngle = 0f

    let tweener = new Tweener()
    
    override this.Initialize() =
        
        
        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        this.camera <- OrthographicCamera(viewportAdapter)


        let easingFn = EasingFunctions.QuadraticIn

        let tween = 
            tweener.TweenTo(bubble, (fun bubble -> bubble.RadiusX), 100f, 1f, 1f)
                .RepeatForever(0.5f)
                .AutoReverse()
                .Easing(easingFn)

        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        this.dot <- this.Content.Load "1px"


        this.asteroids1 <- new AsteroidShowerSystem(EllipseF(bubble.Center, 300f, 200f))
        this.asteroids1.Bubble <- bubble

        this.asteroidsRenderSystem <- new AsteroidRenderSystem(this.GraphicsDevice, this.camera)

        this.boids1 <- new BoidsSystem(EllipseF(boidsTarget.Center, 300f, 450f))
        this.boids1.Target <- boidsTarget

        let world =
            WorldBuilder()
                
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, this.camera))
                .AddSystem(new TransformUpdateSystem())

                .AddSystem(this.asteroids1)
                .AddSystem(new AsteroidExpirySystem())
                .AddSystem(this.asteroidsRenderSystem)

                .AddSystem(this.boids1)
                .AddSystem(new BoidsRenderSystem(this.GraphicsDevice, this.camera))

                .Build()

        this.Components.Add(world)

        let testEntity = world.CreateEntity()
        testEntity.Attach(Transform2(100f, 300f, 0f, 100f, 100f))
        let mutable dotSprite = Sprite(this.dot)
        dotSprite.Color <- Color.Goldenrod
        testEntity.Attach(dotSprite)

        base.LoadContent()
        ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // TODO: tweener component or entity?
        tweener.Update(dt)

        asteroidAngle <- (asteroidAngle + dt * 0.15f) % MathF.Tau
        this.asteroids1.SpawnAngle <- asteroidAngle
        this.boids1.SpawnAngle <- MathF.Tau - asteroidAngle

        base.Update gameTime
        ()

    override this.Draw(gameTime) =

        spriteBatch.Begin(transformMatrix = this.camera.GetViewMatrix())

        spriteBatch.DrawEllipse(bubble.Center, Vector2(bubble.RadiusX, bubble.RadiusY), 32, Color.Azure)

        let pointOnBoundary = this.asteroids1.PointOnBoundary
        
        spriteBatch.DrawCircle(pointOnBoundary, 5f, 12, Color.Black)
        let rect = this.asteroids1.SpawnRange()
        let topleft =(Vector2(rect.TopLeft.X, rect.TopLeft.Y)).Rotate(this.asteroids1.SpawnAngle) + pointOnBoundary
        let topright=(Vector2(rect.TopRight.X, rect.TopRight.Y)).Rotate(this.asteroids1.SpawnAngle)  + pointOnBoundary
        let bottomleft =(Vector2(rect.BottomLeft.X, rect.BottomLeft.Y) ).Rotate(this.asteroids1.SpawnAngle) + pointOnBoundary
        let bottomright =(Vector2(rect.BottomRight.X, rect.BottomRight.Y)).Rotate(this.asteroids1.SpawnAngle) + pointOnBoundary

        spriteBatch.DrawLine(topleft, topright, Color.Brown)
        spriteBatch.DrawLine(topleft, bottomleft, Color.Brown)
        spriteBatch.DrawLine(bottomright, topright, Color.Brown)
        spriteBatch.DrawLine(bottomright, bottomleft, Color.Brown)


        spriteBatch.DrawCircle(boidsTarget, 12, Color.Chartreuse)

        spriteBatch.End()

        base.Draw gameTime
        ()

