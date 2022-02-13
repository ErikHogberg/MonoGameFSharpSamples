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

type DanmakuGame (game) =
    inherit GameScreenWithComponents (game)

    let mutable dot: Texture2D = null
    let mutable fira: SpriteFont = null

    let mutable camera: OrthographicCamera = null
    let mutable bullets1: PlayerBulletSystem = Unchecked.defaultof<PlayerBulletSystem>
    let mutable boids1: BoidsSystem = Unchecked.defaultof<BoidsSystem>

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let playerSpeed = 300f;
    let player = Player(playerSpeed,Vector2(300f, 600f))
    let playerBoundaries = RectangleF(Point2(200f,100f), Size2(400f, 600f))

    let box = RectangleF(600f, 200f, 50f,80f)
    let bubble = EllipseF(Vector2(600f, 400f), 50f,80f) 
    let bulletTarget = CircleF(Vector2(400f, 200f), 1f)

    let boidsTarget = CircleF(Vector2(1300f, 600f), 10f)

    let mutable asteroidAngle = 0f

    let tweener = new Tweener()

    let mutable mouseListener = MouseListener()
    let mutable touchListener = TouchListener()
    let mutable kbdListener = KeyboardListener()

    let mutable upPressed = false
    let mutable leftPressed = false
    let mutable downPressed = false
    let mutable rightPressed = false
    
    override this.Initialize() =
        
        // FIXME: stretched on launch until resize        
        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1080, 1920)

        camera <- OrthographicCamera(viewportAdapter)


        let easingFn = EasingFunctions.QuadraticIn
        

        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        kbdListener.KeyPressed.Add(fun args  ->

            match args.Key with 
            | Keys.Z ->
                bullets1.Firing <- true
            | Keys.W | Keys.I ->
                upPressed <- true
                // player.SetVelocity(Vector2.UnitY * -playerSpeed)
                player.CurrentVelocity <- Vector2.UnitY * -playerSpeed + Vector2.UnitX* player.CurrentVelocity.X
            | Keys.A | Keys.J ->
                leftPressed <- true
                player.CurrentVelocity <- Vector2.UnitX * -playerSpeed + Vector2.UnitY* player.CurrentVelocity.Y
                // player.SetVelocity(Vector2.UnitX * -playerSpeed)
            | Keys.S | Keys.K ->
                downPressed <- true
                player.CurrentVelocity <- Vector2.UnitY * playerSpeed + Vector2.UnitX* player.CurrentVelocity.X
                // player.SetVelocity(Vector2.UnitY * playerSpeed)
            | Keys.D | Keys.L ->
                rightPressed <- true
                player.CurrentVelocity <- Vector2.UnitX * playerSpeed + Vector2.UnitY* player.CurrentVelocity.Y
                // player.SetVelocity(Vector2.UnitX * playerSpeed)
            | _ -> ()

            ())

        kbdListener.KeyReleased.Add(fun args ->

            match args.Key with 
            | Keys.Z ->
                bullets1.Firing <- false
            | Keys.W | Keys.I ->
                upPressed <- false
                if( player.CurrentVelocity.Y < 0f) then
                    if downPressed then
                        player.CurrentVelocity <- Vector2.UnitY * playerSpeed + Vector2.UnitX* player.CurrentVelocity.X
                    else
                        player.CurrentVelocity <- Vector2.UnitX * player.CurrentVelocity.X
            | Keys.A | Keys.J ->
                leftPressed <- false
                if( player.CurrentVelocity.X < 0f) then
                    if rightPressed then 
                        player.CurrentVelocity <- Vector2.UnitX * playerSpeed + Vector2.UnitY* player.CurrentVelocity.Y
                    else
                        player.CurrentVelocity <- Vector2.UnitY * player.CurrentVelocity.Y
            | Keys.S | Keys.K ->
                downPressed <- false
                if( player.CurrentVelocity.Y > 0f) then
                    if upPressed then
                        player.CurrentVelocity <- Vector2.UnitY * -playerSpeed + Vector2.UnitX* player.CurrentVelocity.X
                    else
                        player.CurrentVelocity <- Vector2.UnitX * player.CurrentVelocity.X
            | Keys.D | Keys.L ->
                rightPressed <- false
                if( player.CurrentVelocity.X > 0f) then
                    if leftPressed then 
                        player.CurrentVelocity <- Vector2.UnitX * -playerSpeed + Vector2.UnitY* player.CurrentVelocity.Y
                    else
                        player.CurrentVelocity <- Vector2.UnitY * player.CurrentVelocity.Y

            | _ -> ()
            ())

        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        dot <- this.Content.Load "1px"
        fira <- this.Content.Load "Fira Code"

        boids1 <- new BoidsSystem(EllipseF(boidsTarget.Center, 300f, 450f))
        boids1.Target <- boidsTarget

        bullets1 <- new PlayerBulletSystem(player.Transform,playerBoundaries)

        // TODO
        bullets1.Target <- bulletTarget//CircleF(Vector2(300f, 200f), 1f)

        let world =
            WorldBuilder()
                
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, camera))

                .AddSystem(boids1)
                // .AddSystem(new BoidsRenderSystem(this.GraphicsDevice, camera))

                .AddSystem(bullets1)
                .AddSystem(new EnemyBulletSystem(Transform2(300f,150f,0f,0f,0f), playerBoundaries))
                .AddSystem(new DotRenderSystem(this.GraphicsDevice, camera))

                .Build()

        this.Components.Add(world)


        base.LoadContent()
        ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // TODO: tweener component or entity?
        tweener.Update dt

        asteroidAngle <- (asteroidAngle + dt * 0.15f) % MathF.Tau
        boids1.SpawnAngle <-  asteroidAngle

        player.Update gameTime
        player.ConstrainToFrame playerBoundaries

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

        player.Draw spriteBatch gameTime


        spriteBatch.DrawString(fira, "Danmaku", Vector2(600f, 100f), Color.WhiteSmoke);

        spriteBatch.End()

        base.Draw gameTime
        ()

