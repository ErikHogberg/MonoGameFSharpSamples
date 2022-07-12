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

    let mouseListener = MouseListener ()
    let touchListener = TouchListener ()
    let kbdListener = KeyboardListener ()

    let screenManager = new ScreenManager()

    let mutable screenCalls: (unit -> GameScreenWithComponents) list = [
            (fun _ -> (new DanmakuGame(x, graphics)));
            (fun _ -> new SpaceGame(x, graphics));
            (fun _ -> new DebugScene(x, graphics));
            (fun _ -> new Test1.TestGame1(x, graphics))
            (fun _ -> new Bookshop.BookShopGame(x, graphics))
            (fun _ -> new CardGame.CardGame(x, graphics))
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
            | Keys.D6 ->
                screenManager.LoadScreen(screenCalls[5] (), new FadeTransition(this.GraphicsDevice, Color.Beige, 1f))
            | Keys.D7 ->
                screenManager.LoadScreen(screenCalls[6] (), new FadeTransition(this.GraphicsDevice, Color.Beige, 1f))
            | _ -> ()
           

        base.Initialize()

    override this.LoadContent() =

        screenManager.LoadScreen(screenCalls.Head (), new FadeTransition(this.GraphicsDevice, Color.Black, 1f))

        ()

