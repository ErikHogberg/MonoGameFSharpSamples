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

type DebugScene(game: Game, graphics) =
    inherit GameScreenWithComponents(game, graphics)

    let mutable firaCode: SpriteFont = null
    let mutable camera: OrthographicCamera = null

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let mouseListener = MouseListener()
    let touchListener = TouchListener()
    let kbdListener = KeyboardListener()


    let mutable v1 = Vector2(500f,400f)
    let mutable v2 = Vector2(600f,400f)
    let mutable v3 = Vector2(550f,450f)

    let mutable iSqrt1 = 0f;
    let mutable iSqrt2 = 0f;
    let mutable sqrtVal = 1f;

    let mutable fastLength = 1f;
    let mutable length = 1f;
    
    
    override this.Initialize() =
        
        
        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera(viewportAdapter)


        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        kbdListener.KeyPressed.Add(fun args  ->
            if args.Key = Keys.E then
                v2 <- v3 + (v2-v3).MoveTowards (v1-v3, 10f )
                ()
            if args.Key = Keys.Q then
                v2 <- v3 + (v2-v3).MoveTowards (v1-v3, -10f, Vector2.UnitY)
            if args.Key = Keys.A then
                v2 <- v3 + (v2-v3).RotateTowards (v1-v3) 10f
                ()
            if args.Key = Keys.D then
                v2 <- v3 + (v2-v3).RotateTowards (v1-v3) -10f
                ()
            if args.Key = Keys.X then
                iSqrt1 <- 1f/MathF.Sqrt(sqrtVal)
                iSqrt2 <- Tools.InverseSqrt(sqrtVal)
                sqrtVal <- sqrtVal + 1f
                fastLength <- (v2-v3).FastLength()
                length <- (v2-v3).Length()
                ()
            if args.Key = Keys.Left then
                v2 <- v2 - Vector2.UnitX*10f
            if args.Key = Keys.Right then
                v2 <- v2 + Vector2.UnitX*10f
                ()
            ())


        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // this.dot <- this.Content.Load "1px"
        firaCode <- this.Content.Load "Fira Code"

       
        base.LoadContent()
        ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds()

        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.Beige

        spriteBatch.Begin(transformMatrix = camera.GetViewMatrix())

        spriteBatch.DrawCircle(v1, 15f, 8, Color.Red, 5f)
        spriteBatch.DrawCircle(v3, 15f, 8, Color.Green, 5f)
        spriteBatch.DrawCircle(v2, 15f, 8, Color.Blue, 5f)

        let v2Delta = v2-v3
        spriteBatch.DrawString(firaCode, $"v2: {v2Delta}", Vector2(100f, 100f), Color.Black);
        spriteBatch.DrawString(firaCode, $"{v2Delta.FastNormalizedCopy()}", Vector2(100f, 120f), Color.Black);
        spriteBatch.DrawString(firaCode, $"{v2Delta.NormalizedCopy()}", Vector2(100f, 140f), Color.Black);

        spriteBatch.DrawString(firaCode, $"isqrt({sqrtVal}): {iSqrt1}, {iSqrt2}", Vector2(100f, 200f), Color.Black);

        spriteBatch.DrawString(firaCode, $"ln: {fastLength} (fast)", Vector2(100f, 300f), Color.Black);
        spriteBatch.DrawString(firaCode, $"ln: {length}", Vector2(100f, 320f), Color.Black);

        spriteBatch.DrawString(firaCode, "Debug Scene", Vector2(600f, 100f), Color.Black);

        spriteBatch.End()

        base.Draw gameTime
        ()

