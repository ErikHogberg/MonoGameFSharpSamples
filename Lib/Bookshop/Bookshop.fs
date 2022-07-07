namespace Bookshop

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
open MonoGame.Extended.Tweening

open GameScreenWithComponents
open Tools
open TransformUpdater
open RenderSystem

type BookShopGame (game, graphics) =
    inherit GameScreenWithComponents (game, graphics)

    let mutable fira: SpriteFont = null
    let mutable camera: OrthographicCamera = null
    let mutable spriteBatch: SpriteBatch = null//Unchecked.defaultof<SpriteBatch>

    let playerSpeed = 400f;
    // let playerTransform = Transform2 (Vector2(300f, 600f))
    // let playerBoundaries = RectangleF (Point2(600f, 50f), Size2(700f, 1000f))

    let mouseListener = MouseListener ()
    let touchListener = TouchListener ()
    let kbdListener = KeyboardListener ()

    let mutable upPressed = false
    let mutable leftPressed = false
    let mutable downPressed = false
    let mutable rightPressed = false

    let mutable shiftPressed = false
    let sprintMul = 1.6f
    
    let PlayerSpeedEval () = 
        if shiftPressed then
            playerSpeed * sprintMul
        else
            playerSpeed


    let playerTransform = Transform2 (Vector2(300f, 600f))
    let playerBoundaries = RectangleF (Point2(600f, 50f), Size2(700f, 1000f))
    let playerMover = ConstrainedTransform ()

    do playerMover.Speed <- PlayerSpeedEval ()

    override this.Initialize () =
        
        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera viewportAdapter


        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        kbdListener.KeyPressed.Add <| fun args  ->
            match args.Key with 
            | Keys.E ->
                // TODO: use/interact
                ()
            | Keys.W | Keys.I | Keys.Up ->
                upPressed <- true
                playerMover.Velocity <- Vector2.UnitY * -1f + Vector2.UnitX * playerMover.Velocity.X
            | Keys.A | Keys.J | Keys.Left ->
                leftPressed <- true
                playerMover.Velocity <- Vector2.UnitX * -1f + Vector2.UnitY * playerMover.Velocity.Y
            | Keys.S | Keys.K | Keys.Down->
                downPressed <- true
                playerMover.Velocity <- Vector2.UnitY + Vector2.UnitX * playerMover.Velocity.X
            | Keys.D | Keys.L | Keys.Right ->
                rightPressed <- true
                playerMover.Velocity <- Vector2.UnitX + Vector2.UnitY * playerMover.Velocity.Y
            | Keys.LeftShift -> 
                shiftPressed <- true
                playerMover.Speed <- PlayerSpeedEval ()
            | _ -> ()


        kbdListener.KeyReleased.Add <| fun args ->
            match args.Key with 
            | Keys.W | Keys.I ->
                upPressed <- false
                if playerMover.Velocity.Y < 0f then
                    if downPressed then
                        playerMover.Velocity <- Vector2.UnitY+ Vector2.UnitX* playerMover.Velocity.X
                    else
                        playerMover.Velocity <- Vector2.UnitX * playerMover.Velocity.X
            | Keys.A | Keys.J ->
                leftPressed <- false
                if playerMover.Velocity.X < 0f then
                    if rightPressed then 
                        playerMover.Velocity <- Vector2.UnitX + Vector2.UnitY * playerMover.Velocity.Y 
                    else
                        playerMover.Velocity <- Vector2.UnitY * playerMover.Velocity.Y
            | Keys.S | Keys.K ->
                downPressed <- false
                if playerMover.Velocity.Y > 0f then
                    if upPressed then
                        playerMover.Velocity <- Vector2.UnitY * -1f + Vector2.UnitX * playerMover.Velocity.X
                    else
                        playerMover.Velocity <- Vector2.UnitX * playerMover.Velocity.X
            | Keys.D | Keys.L ->
                rightPressed <- false
                if playerMover.Velocity.X > 0f then
                    if leftPressed then 
                        playerMover.Velocity <- Vector2.UnitX * -1f + Vector2.UnitY * playerMover.Velocity.Y
                    else
                        playerMover.Velocity <- Vector2.UnitY * playerMover.Velocity.Y
            | Keys.LeftShift -> 
                shiftPressed <- false
                playerMover.Speed <- PlayerSpeedEval ()
            | _ -> ()
            
        base.Initialize ()


    override this.LoadContent () =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        fira <- this.Content.Load "Fira Code"

        let world =
            WorldBuilder()
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, camera))
                .AddSystem(new DotRenderSystem(this.GraphicsDevice, camera))

                .AddSystem(new ConstrainedTransformSystem(playerBoundaries))
                .AddSystem(new TransformFollowerSystem ())
                .AddSystem(new TweenTransformerSystem())

                .AddSystem(new ecs.DelayedActionSystem())

                .Build ()


        let playerEntity = world.CreateEntity()
        playerEntity.Attach playerTransform
        playerEntity.Attach playerMover
        playerEntity.Attach <| Dot Color.GreenYellow
        playerEntity.Attach <| SizeComponent 10f

        this.Components.Add world

        base.LoadContent ()


    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.PaleVioletRed

        spriteBatch.Begin (transformMatrix = camera.GetViewMatrix())
        spriteBatch.FillRectangle (playerBoundaries, Color.DarkSalmon)
        spriteBatch.DrawString (fira, "Bookshop", Vector2(300f, 300f), Color.WhiteSmoke)

        spriteBatch.End ()

        base.Draw gameTime

