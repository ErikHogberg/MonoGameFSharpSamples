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
open Danmaku
open SpaceTactics
open DebugScene


// TODO: use scene graphs

type Game1() as x =
    inherit Game()

    do x.Content.RootDirectory <- "Content"

    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch: SpriteBatch = null

    // note that listeners and many other things do not need to be mutable, despite being modified later. 
    // only the reference to the class instance needs to be immutable 
    let mouseListener = MouseListener()
    let touchListener = TouchListener()
    let kbdListener = KeyboardListener()

    // TODO: resize screen manager on window resize
    let screenManager = new ScreenManager()

    let mutable camera: OrthographicCamera = null
    let mutable dot: Texture2D = null

    member this.OnResize (e: EventArgs) = 
        graphics.PreferredBackBufferWidth <- graphics.GraphicsDevice.Viewport.Width;
        graphics.PreferredBackBufferHeight <- graphics.GraphicsDevice.Viewport.Height;
        graphics.ApplyChanges();
        ()
    
    override this.Initialize() =

        this.IsFixedTimeStep <- false
        this.IsMouseVisible <- true

        this.Window.AllowUserResizing <- true
        this.Window.ClientSizeChanged.Add this.OnResize

        graphics.PreferredBackBufferWidth <- 
            // 1920
            1280
        graphics.PreferredBackBufferHeight <- 
            // 1080
            720

        graphics.PreferMultiSampling <- false
        graphics.ApplyChanges()

        let listenerComponent =
            new InputListenerComponent(this, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent
        this.Components.Add screenManager

        kbdListener.KeyPressed.Add <| fun args  ->
            match args.Key with
            | Keys.Escape ->
                this.Exit()
            | Keys.D1 ->
                screenManager.LoadScreen(new DanmakuGame(this, graphics), new FadeTransition(this.GraphicsDevice, Color.Black, 0.5f))
            | Keys.D2 ->
                screenManager.LoadScreen(new SpaceGame(this, graphics), new FadeTransition(this.GraphicsDevice, Color.Red, 0.5f))
            | Keys.D3 ->
                screenManager.LoadScreen(new DebugScene(this, graphics), new FadeTransition(this.GraphicsDevice, Color.Green, 0.5f))
            | Keys.D4 ->
                screenManager.LoadScreen(new Test1.TestGame1(this, graphics), new FadeTransition(this.GraphicsDevice, Color.Blue, 1f))
            | Keys.D5 ->
                screenManager.LoadScreen(new MainMenu.MainMenu(this, graphics), new FadeTransition(this.GraphicsDevice, Color.Blue, 1f))
            // | Keys.Space ->
                // this.asteroidsRenderSystem.AlwaysShow <- not this.asteroidsRenderSystem.AlwaysShow
            | _ -> ()
           
            

        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera(viewportAdapter)

        base.Initialize()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // printfn "content root: %s" this.Content.RootDirectory
        dot <- this.Content.Load "1px"

        screenManager.LoadScreen(new MainMenu.MainMenu(this, graphics), new FadeTransition(this.GraphicsDevice, Color.Black, 1f))

        ()

    // override this.Update(gameTime) =
        // base.Update gameTime

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue
        base.Draw gameTime
