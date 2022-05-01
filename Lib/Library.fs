namespace Game1

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Input.InputListeners
open MonoGame.Extended.ViewportAdapters
open MonoGame.Extended.Screens
open MonoGame.Extended.Screens.Transitions

open Danmaku
open SpaceTactics
open DebugScene
open GameScreenWithComponents

type Game1() as x =
    inherit Game()

    do x.Content.RootDirectory <- "Content"

    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch: SpriteBatch = null

    let mouseListener = MouseListener ()
    let touchListener = TouchListener ()
    let kbdListener = KeyboardListener ()

    let screenManager = new ScreenManager()

    let mutable camera: OrthographicCamera = null
    let mutable dot: Texture2D = null

    let mutable screenCalls: (unit -> GameScreenWithComponents) list = [
            (fun _ -> (new DanmakuGame(x, graphics)));
            (fun _ -> new SpaceGame(x, graphics));
            (fun _ -> new DebugScene(x, graphics));
            (fun _ -> new Test1.TestGame1(x, graphics))
        ]

    do screenCalls <- (fun _ -> new MainMenu.MainMenu(x, graphics, screenCalls)) :: screenCalls


    member this.OnResize e = 
        graphics.PreferredBackBufferWidth <- graphics.GraphicsDevice.Viewport.Width
        graphics.PreferredBackBufferHeight <- graphics.GraphicsDevice.Viewport.Height
        graphics.ApplyChanges ()
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
        graphics.ApplyChanges ()

        let listenerComponent =
            new InputListenerComponent (this, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent
        this.Components.Add screenManager

        kbdListener.KeyPressed.Add <| fun args  ->
            match args.Key with
            | Keys.Escape ->
                this.Exit ()
            | Keys.D1 ->
                screenManager.LoadScreen(screenCalls[1] (), new FadeTransition(this.GraphicsDevice, Color.Black, 0.5f))
            | Keys.D2 ->
                screenManager.LoadScreen(screenCalls[2] (), new FadeTransition(this.GraphicsDevice, Color.Red, 0.5f))
            | Keys.D3 ->
                screenManager.LoadScreen(screenCalls[3] (), new FadeTransition(this.GraphicsDevice, Color.Green, 0.5f))
            | Keys.D4 ->
                screenManager.LoadScreen(screenCalls[4] (), new FadeTransition(this.GraphicsDevice, Color.Blue, 1f))
            | Keys.D5 ->
                screenManager.LoadScreen(screenCalls[0] (), new FadeTransition(this.GraphicsDevice, Color.Orange, 1f))
            | _ -> ()
           
            

        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 16, 9)

        camera <- OrthographicCamera viewportAdapter

        base.Initialize()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // printfn "content root: %s" this.Content.RootDirectory
        dot <- this.Content.Load "1px"


        screenManager.LoadScreen(screenCalls.Head (), new FadeTransition(this.GraphicsDevice, Color.Black, 1f))

        ()

    // override this.Update(gameTime) =
        // base.Update gameTime

    override this.Draw gameTime =
        this.GraphicsDevice.Clear Color.CornflowerBlue
        base.Draw gameTime
