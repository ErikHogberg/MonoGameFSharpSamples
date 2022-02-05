module DebugScene

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

open MonoGame.Extended
open MonoGame.Extended.Input.InputListeners
open MonoGame.Extended.Sprites
open MonoGame.Extended.ViewportAdapters
open MonoGame.Extended.Screens
open MonoGame.Extended.Screens.Transitions

open GameScreenWithComponents
open Tools

type DebugScene(game: Game) =
    inherit GameScreenWithComponents(game)

    [<DefaultValue>]
    val mutable firaCode: SpriteFont
    
    [<DefaultValue>]
    val mutable camera: OrthographicCamera

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable mouseListener = MouseListener()
    let mutable touchListener = TouchListener()
    let mutable kbdListener = KeyboardListener()


    let mutable v1= Vector2(500f,400f)
    let mutable v2= Vector2(600f,400f)
    let mutable v3= Vector2(550f,450f)
    
    
    override this.Initialize() =
        
        
        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        this.camera <- OrthographicCamera(viewportAdapter)


        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        kbdListener.KeyPressed.Add(fun args  ->
            if args.Key = Keys.E then
                // v1 <- v1.MoveTowards v2 1f
                v2 <- v3 + (v2-v3).MoveTowards (v1-v3, 10f )
                ()
            if args.Key = Keys.Q then
                // v1 <- v1.MoveTowards v2 -1f
                v2 <- v3 + (v2-v3).MoveTowards (v1-v3, -10f, Vector2.UnitY)
            if args.Key = Keys.A then
                v2 <- v3 + (v2-v3).RotateTowards (v1-v3) 10f
                ()
            if args.Key = Keys.D then
                v2 <- v3 + (v2-v3).RotateTowards (v1-v3) -10f
                ()
            ())


        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // this.dot <- this.Content.Load "1px"
        this.firaCode <- this.Content.Load "Fira Code"

       
        base.LoadContent()
        ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds()

        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.Beige

        spriteBatch.Begin(transformMatrix = this.camera.GetViewMatrix())

        spriteBatch.DrawCircle(v1, 15f, 8, Color.Red, 5f)
        spriteBatch.DrawCircle(v3, 15f, 8, Color.Green, 5f)
        spriteBatch.DrawCircle(v2, 15f, 8, Color.Blue, 5f)


        spriteBatch.DrawString(this.firaCode, $"v2: {v2}", Vector2(100f, 100f), Color.Black);

        spriteBatch.End()

        base.Draw gameTime
        ()

