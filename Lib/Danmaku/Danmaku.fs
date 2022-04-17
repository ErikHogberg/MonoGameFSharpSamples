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
open Boids
open Bullets
open Tools

type DanmakuGame (game, graphics) =
    inherit GameScreenWithComponents (game, graphics)

    let mutable dot: Texture2D = null
    let mutable fira: SpriteFont = null

    let mutable camera: OrthographicCamera = null

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let playerSpeed = 300f;
    let player = Player(playerSpeed,Vector2(300f, 600f))
    let playerBoundaries = RectangleF(Point2(600f,50f), Size2(700f, 1000f))
    let playerBulletSpawner = BulletSpawner (10f, Vector2.UnitY* -150f) 

    let bulletTarget = CircleF(Point2(700f, 200f), 10f)

    let tweener = new Tweener()

    let mutable mouseListener = MouseListener()
    let mutable touchListener = TouchListener()
    let mutable kbdListener = KeyboardListener()

    let mutable upPressed = false
    let mutable leftPressed = false
    let mutable downPressed = false
    let mutable rightPressed = false

    let mutable shiftPressed = false
    let sprintSpeed = 1.5f
    
    let PlayerSpeedEval() = 
        if shiftPressed then
            playerSpeed*sprintSpeed
        else
            playerSpeed

    do player.Speed <- PlayerSpeedEval()

    override this.Initialize() =
        
        // FIXME: stretched on launch until resize        
        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1080, 1920)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera(viewportAdapter)

        // TODO: resize on screen change
        // IDEA: expose camera field, resize from Game1
        // graphics.PreferredBackBufferWidth <- graphics.GraphicsDevice.Viewport.Width;
        // graphics.PreferredBackBufferHeight <- graphics.GraphicsDevice.Viewport.Height;
        // graphics.ApplyChanges()

        // let easingFn = EasingFunctions.QuadraticIn
        

        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        kbdListener.KeyPressed.Add(fun args  ->

            match args.Key with 
            | Keys.Z ->
                playerBulletSpawner.Firing <- true
                // bullets1.Firing <- true
            | Keys.W | Keys.I ->
                upPressed <- true
                // player.SetVelocity(Vector2.UnitY * -playerSpeed)
                player.CurrentVelocity <- Vector2.UnitY * -1f + Vector2.UnitX* player.CurrentVelocity.X
            | Keys.A | Keys.J ->
                leftPressed <- true
                player.CurrentVelocity <- Vector2.UnitX * -1f + Vector2.UnitY* player.CurrentVelocity.Y
                // player.SetVelocity(Vector2.UnitX * -playerSpeed)
            | Keys.S | Keys.K ->
                downPressed <- true
                player.CurrentVelocity <- Vector2.UnitY + Vector2.UnitX* player.CurrentVelocity.X
                // player.SetVelocity(Vector2.UnitY * playerSpeed)
            | Keys.D | Keys.L ->
                rightPressed <- true
                player.CurrentVelocity <- Vector2.UnitX + Vector2.UnitY* player.CurrentVelocity.Y
                // player.SetVelocity(Vector2.UnitX * playerSpeed)
            | Keys.LeftShift -> 
                shiftPressed <- true
                player.Speed <- PlayerSpeedEval()
            | _ -> ()

            ())

        kbdListener.KeyReleased.Add(fun args ->

            match args.Key with 
            | Keys.Z ->
                playerBulletSpawner.Firing <- false
                // bullets1.Firing <- false
            | Keys.W | Keys.I ->
                upPressed <- false
                if( player.CurrentVelocity.Y < 0f) then
                    if downPressed then
                        player.CurrentVelocity <- Vector2.UnitY+ Vector2.UnitX* player.CurrentVelocity.X
                    else
                        player.CurrentVelocity <- Vector2.UnitX * player.CurrentVelocity.X
            | Keys.A | Keys.J ->
                leftPressed <- false
                if( player.CurrentVelocity.X < 0f) then
                    if rightPressed then 
                        player.CurrentVelocity <- Vector2.UnitX + Vector2.UnitY* player.CurrentVelocity.Y 
                    else
                        player.CurrentVelocity <- Vector2.UnitY * player.CurrentVelocity.Y
            | Keys.S | Keys.K ->
                downPressed <- false
                if( player.CurrentVelocity.Y > 0f) then
                    if upPressed then
                        player.CurrentVelocity <- Vector2.UnitY * -1f + Vector2.UnitX* player.CurrentVelocity.X
                    else
                        player.CurrentVelocity <- Vector2.UnitX * player.CurrentVelocity.X
            | Keys.D | Keys.L ->
                rightPressed <- false
                if( player.CurrentVelocity.X > 0f) then
                    if leftPressed then 
                        player.CurrentVelocity <- Vector2.UnitX * -1f + Vector2.UnitY* player.CurrentVelocity.Y
                    else
                        player.CurrentVelocity <- Vector2.UnitY * player.CurrentVelocity.Y
            | Keys.LeftShift -> 
                shiftPressed <- false
                player.Speed <- PlayerSpeedEval()
            | _ -> ()
            ())

        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        dot <- this.Content.Load "1px"
        fira <- this.Content.Load "Fira Code"

        let world =
            WorldBuilder()
                
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, camera))

                .AddSystem(new BulletSystem(playerBoundaries))
                // .AddSystem(new EnemyBulletSystem(Transform2(800f,150f,0f,0f,0f), playerBoundaries))
                .AddSystem(new BulletSpawnerSystem())
                .AddSystem(new DotRenderSystem(this.GraphicsDevice, camera))
                .AddSystem(new Collision.CollisionSystem(playerBoundaries))

                .Build ()

        let playerEntity = world.CreateEntity()
        playerEntity.Attach player.Transform
        playerEntity.Attach playerBulletSpawner

        this.Components.Add (world)

        base.LoadContent ()
        ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // TODO: tweener component or entity?
        tweener.Update dt

        player.Update gameTime
        player.ConstrainToFrame playerBoundaries

        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.PaleVioletRed

        spriteBatch.Begin (transformMatrix = camera.GetViewMatrix())

        spriteBatch.FillRectangle (playerBoundaries, Color.DarkSalmon)
        spriteBatch.DrawCircle(bulletTarget, 8, Color.AliceBlue, 5f)

        player.Draw spriteBatch gameTime

        spriteBatch.DrawString (fira, "Danmaku", Vector2(600f, 100f), Color.WhiteSmoke);

        spriteBatch.End ()

        base.Draw gameTime
        ()

