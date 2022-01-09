namespace SpaceGame

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


open Ship
open Asteroids
open Boids
open Tools
open RenderSystem
open UpdateSystem
open Danmaku
open SpaceTactics

type Game1() as x =
    inherit Game()

    do x.Content.RootDirectory <- "Content"

    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable mouseListener = MouseListener()
    let mutable touchListener = TouchListener()
    let mutable kbdListener = KeyboardListener()

    let screenManager = new ScreenManager()

    [<DefaultValue>]
    val mutable camera: OrthographicCamera

    [<DefaultValue>]
    val mutable dot: Texture2D
    
    override this.Initialize() =

        this.IsFixedTimeStep <- false
        this.IsMouseVisible <- true

        this.Window.AllowUserResizing <- true

        graphics.PreferredBackBufferWidth <- 1920//1280
        graphics.PreferredBackBufferHeight <- 1080//720
        graphics.PreferMultiSampling <- false
        graphics.ApplyChanges()

        let listenerComponent =
            new InputListenerComponent(this, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent
        this.Components.Add screenManager

        kbdListener.KeyPressed.Add(fun args  ->
            if args.Key = Keys.Escape then
                this.Exit()
                ()
            // else if args.Key = Keys.Space then
            //     this.asteroidsRenderSystem.AlwaysShow <- not this.asteroidsRenderSystem.AlwaysShow
            //     ()
            ())

        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        this.camera <- OrthographicCamera(viewportAdapter)


        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // printfn "content root: %s" this.Content.RootDirectory
        this.dot <- this.Content.Load "1px"

        // Singleton.Instance.Set("dot", this.dot)

        

        screenManager.LoadScreen(new DanmakuGame(this), new FadeTransition(this.GraphicsDevice, Color.Black, 1f))

        ()

    override this.Update(gameTime) =
        // let dt = gameTime.GetElapsedSeconds()
     
        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue

        // spriteBatch.Begin(transformMatrix = this.camera.GetViewMatrix())

        // spriteBatch.DrawEllipse(bubble.Center, Vector2(bubble.RadiusX, bubble.RadiusY), 32, Color.Azure)

        // let pointOnBoundary = this.asteroids1.PointOnBoundary
        
        // spriteBatch.DrawCircle(pointOnBoundary, 5f, 12, Color.Black)
        // let rect = this.asteroids1.SpawnRange()
        // let topleft =(Vector2(rect.TopLeft.X, rect.TopLeft.Y)).Rotate(this.asteroids1.SpawnAngle) + pointOnBoundary
        // let topright=(Vector2(rect.TopRight.X, rect.TopRight.Y)).Rotate(this.asteroids1.SpawnAngle)  + pointOnBoundary
        // let bottomleft =(Vector2(rect.BottomLeft.X, rect.BottomLeft.Y) ).Rotate(this.asteroids1.SpawnAngle) + pointOnBoundary
        // let bottomright =(Vector2(rect.BottomRight.X, rect.BottomRight.Y)).Rotate(this.asteroids1.SpawnAngle) + pointOnBoundary

        // spriteBatch.DrawLine(topleft, topright, Color.Brown)
        // spriteBatch.DrawLine(topleft, bottomleft, Color.Brown)
        // spriteBatch.DrawLine(bottomright, topright, Color.Brown)
        // spriteBatch.DrawLine(bottomright, bottomleft, Color.Brown)


        // spriteBatch.DrawCircle(boidsTarget, 12, Color.Chartreuse)

        // spriteBatch.End()

        base.Draw gameTime
        ()
