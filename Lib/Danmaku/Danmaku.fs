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
open MonoGame.Extended.Tweening

open RenderSystem
open GameScreenWithComponents
open Bullets
open Tools
open TransformUpdater

type DanmakuGame (game, graphics) =
    inherit GameScreenWithComponents (game, graphics)

    let mutable fira: SpriteFont = null

    let mutable camera: OrthographicCamera = null

    let mutable spriteBatch: SpriteBatch = null//Unchecked.defaultof<SpriteBatch>

    let playerTag = "player"
    let enemyTag = "enemy"

    let playerSpeed = 400f;
    let player = Player ()
    let playerTransform = Transform2 (Vector2(300f, 600f))
    let playerBoundaries = RectangleF (Point2(600f, 50f), Size2(700f, 1000f))
    let playerMover = ConstrainedTransform ()

    let playerBulletOnCollision (other: Collision.TransformCollisionActor) = other.Tag <> enemyTag
    let playerBulletSpawners = [
        BulletSpawner (
            10f, 
            Vector2.UnitY* -350f,
            playerTag,
            playerBulletOnCollision
        );
        BulletSpawner (
            10f, 
            Vector2.UnitY* -350f + Vector2.UnitX * 100f,
            playerTag,
            playerBulletOnCollision
        );
        BulletSpawner (
            10f, 
            Vector2.UnitY* -350f + Vector2.UnitX * -100f,
            playerTag,
            playerBulletOnCollision
        );
    ] 

    let mutable mouseListener = MouseListener ()
    let mutable touchListener = TouchListener ()
    let mutable kbdListener = KeyboardListener ()

    let mutable upPressed = false
    let mutable leftPressed = false
    let mutable downPressed = false
    let mutable rightPressed = false

    let mutable shiftPressed = false
    let sprintMul = 0.6f
    
    let PlayerSpeedEval () = 
        if shiftPressed then
            playerSpeed * sprintMul
        else
            playerSpeed

    do playerMover.Speed <- PlayerSpeedEval ()

    override this.Initialize () =
        
        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera viewportAdapter

        // let easingFn = EasingFunctions.QuadraticIn        

        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        // TODO: player input ecs

        kbdListener.KeyPressed.Add <| fun args  ->
            match args.Key with 
            | Keys.Z ->
                // TODO: sticky firing, always fire for a minimum time even if released
                // IDEA: implement in bullet spawner system, making firing settings per component
                for playerBulletSpawner in playerBulletSpawners do
                    playerBulletSpawner.Firing <- true
            | Keys.W | Keys.I ->
                upPressed <- true
                playerMover.Velocity <- Vector2.UnitY * -1f + Vector2.UnitX* playerMover.Velocity.X
            | Keys.A | Keys.J ->
                leftPressed <- true
                playerMover.Velocity <- Vector2.UnitX * -1f + Vector2.UnitY* playerMover.Velocity.Y
            | Keys.S | Keys.K ->
                downPressed <- true
                playerMover.Velocity <- Vector2.UnitY + Vector2.UnitX* playerMover.Velocity.X
            | Keys.D | Keys.L ->
                rightPressed <- true
                playerMover.Velocity <- Vector2.UnitX + Vector2.UnitY* playerMover.Velocity.Y
            | Keys.LeftShift -> 
                shiftPressed <- true
                playerMover.Speed <- PlayerSpeedEval()
            | _ -> ()


        kbdListener.KeyReleased.Add <| fun args ->
            match args.Key with 
            | Keys.Z ->
                for playerBulletSpawner in playerBulletSpawners do
                    playerBulletSpawner.Firing <- false
                // bullets1.Firing <- false
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
                        playerMover.Velocity <- Vector2.UnitX + Vector2.UnitY* playerMover.Velocity.Y 
                    else
                        playerMover.Velocity <- Vector2.UnitY * playerMover.Velocity.Y
            | Keys.S | Keys.K ->
                downPressed <- false
                if playerMover.Velocity.Y > 0f then
                    if upPressed then
                        playerMover.Velocity <- Vector2.UnitY * -1f + Vector2.UnitX* playerMover.Velocity.X
                    else
                        playerMover.Velocity <- Vector2.UnitX * playerMover.Velocity.X
            | Keys.D | Keys.L ->
                rightPressed <- false
                if playerMover.Velocity.X > 0f then
                    if leftPressed then 
                        playerMover.Velocity <- Vector2.UnitX * -1f + Vector2.UnitY* playerMover.Velocity.Y
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

                .AddSystem(new BulletSystem (playerBoundaries))
                .AddSystem(new BulletSpawnerSystem())
                .AddSystem(new EnemySpawnerSystem ())

                .AddSystem(new Collision.CollisionSystem(playerBoundaries))

                .AddSystem(new ConstrainedTransformSystem(playerBoundaries))
                .AddSystem(new TransformFollowerSystem ())
                .AddSystem(new TweenTransformerSystem())

                .Build ()

        let leftShooter = world.CreateEntity ()
        leftShooter.Attach <| Dot Color.GreenYellow
        leftShooter.Attach <| SizeComponent 10f
        leftShooter.Attach <| playerBulletSpawners.Tail.Head
        leftShooter.Attach <| TransformFollower (playerTransform, Vector2(50f, -20f))
        leftShooter.Attach <| Transform2 playerTransform.Position

        let rightShooter = world.CreateEntity()
        rightShooter.Attach <| Dot Color.GreenYellow
        rightShooter.Attach <| SizeComponent 10f
        rightShooter.Attach <| playerBulletSpawners.Tail.Tail.Head
        rightShooter.Attach <| TransformFollower (playerTransform, Vector2(-50f, -20f))
        rightShooter.Attach <| Transform2 playerTransform.Position

        let playerEntity = world.CreateEntity()
        playerEntity.Attach playerTransform
        playerEntity.Attach playerMover
        playerEntity.Attach playerBulletSpawners.Head
        playerEntity.Attach <| Dot Color.GreenYellow
        playerEntity.Attach <| SizeComponent 10f
        playerEntity.Attach <| Collision.TransformCollisionActor(
            playerTransform, 
            5f, 
            playerTag, 
            onCollision = fun other -> 
                if other.Tag = enemyTag then
                    player.HP <- player.HP - 10f
                    Console.WriteLine $"hit player (hp: {player.HP})"
                    
                let dead = player.HP <= 0f
                if dead then
                    world.DestroyEntity leftShooter
                    world.DestroyEntity rightShooter
                    ()

                not dead
            )


        let enemySpawner = world.CreateEntity ()
        enemySpawner.Attach <| EnemySpawner(5f, 4u)
        let enemySpawnerTransform = Transform2 (Vector2(800f, 100f))
        enemySpawner.Attach enemySpawnerTransform
        enemySpawner.Attach <| TweenTransformer (TweenTransformer.MoveTweener(enemySpawnerTransform, (Vector2(1200f,100f)), 2f, 0f, EasingFunctions.CircleInOut))
        enemySpawner.Attach <| Dot Color.Aquamarine
        enemySpawner.Attach <| SizeComponent 5f

        this.Components.Add world

        base.LoadContent ()


    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.PaleVioletRed

        spriteBatch.Begin (transformMatrix = camera.GetViewMatrix())
        spriteBatch.FillRectangle (playerBoundaries, Color.DarkSalmon)
        spriteBatch.DrawString (fira, "Danmaku", Vector2(300f, 300f), Color.WhiteSmoke)

        spriteBatch.End ()

        base.Draw gameTime

