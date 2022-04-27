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
open TransformUpdater

type DanmakuGame (game, graphics) =
    inherit GameScreenWithComponents (game, graphics)

    let mutable dot: Texture2D = null
    let mutable fira: SpriteFont = null

    let mutable camera: OrthographicCamera = null

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let playerTag = "player"
    let enemyTag = "enemy"

    let playerSpeed = 400f;
    let player = Player ()
    let playerTransform = Transform2(Vector2(300f, 600f))
    let playerBoundaries = RectangleF(Point2(600f, 50f), Size2(700f, 1000f))
    let playerMover = ConstrainedTransform ()
    let playerBulletSpawners = [
        BulletSpawner (
            10f, 
            Vector2.UnitY* -350f,
            playerTag,
            (fun other -> other.Tag <> enemyTag)
        );
        BulletSpawner (
            10f, 
            Vector2.UnitY* -350f + Vector2.UnitX * 100f,
            playerTag,
            (fun other -> other.Tag <> enemyTag)
        );
        BulletSpawner (
            10f, 
            Vector2.UnitY* -350f + Vector2.UnitX * -100f,
            playerTag,
            (fun other -> other.Tag <> enemyTag)
        );
    ] 

    let bulletTarget = CircleF(Point2(700f, 200f), 10f)

    // let tweener = new Tweener()

    let mutable mouseListener = MouseListener()
    let mutable touchListener = TouchListener()
    let mutable kbdListener = KeyboardListener()

    let mutable upPressed = false
    let mutable leftPressed = false
    let mutable downPressed = false
    let mutable rightPressed = false

    let mutable shiftPressed = false
    let sprintMul = 0.6f
    
    let PlayerSpeedEval() = 
        if shiftPressed then
            playerSpeed * sprintMul
        else
            playerSpeed

    do playerMover.Speed <- PlayerSpeedEval ()

    override this.Initialize() =
        
        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera(viewportAdapter)

        // let easingFn = EasingFunctions.QuadraticIn        

        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        // TODO: player input ecs

        kbdListener.KeyPressed.Add(fun args  ->

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

            ())

        kbdListener.KeyReleased.Add(fun args ->

            match args.Key with 
            | Keys.Z ->
                for playerBulletSpawner in playerBulletSpawners do
                    playerBulletSpawner.Firing <- false
                // bullets1.Firing <- false
            | Keys.W | Keys.I ->
                upPressed <- false
                if( playerMover.Velocity.Y < 0f) then
                    if downPressed then
                        playerMover.Velocity <- Vector2.UnitY+ Vector2.UnitX* playerMover.Velocity.X
                    else
                        playerMover.Velocity <- Vector2.UnitX * playerMover.Velocity.X
            | Keys.A | Keys.J ->
                leftPressed <- false
                if( playerMover.Velocity.X < 0f) then
                    if rightPressed then 
                        playerMover.Velocity <- Vector2.UnitX + Vector2.UnitY* playerMover.Velocity.Y 
                    else
                        playerMover.Velocity <- Vector2.UnitY * playerMover.Velocity.Y
            | Keys.S | Keys.K ->
                downPressed <- false
                if( playerMover.Velocity.Y > 0f) then
                    if upPressed then
                        playerMover.Velocity <- Vector2.UnitY * -1f + Vector2.UnitX* playerMover.Velocity.X
                    else
                        playerMover.Velocity <- Vector2.UnitX * playerMover.Velocity.X
            | Keys.D | Keys.L ->
                rightPressed <- false
                if( playerMover.Velocity.X > 0f) then
                    if leftPressed then 
                        playerMover.Velocity <- Vector2.UnitX * -1f + Vector2.UnitY* playerMover.Velocity.Y
                    else
                        playerMover.Velocity <- Vector2.UnitY * playerMover.Velocity.Y
            | Keys.LeftShift -> 
                shiftPressed <- false
                playerMover.Speed <- PlayerSpeedEval()
            | _ -> ()
            ())

        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        fira <- this.Content.Load "Fira Code"

        let world =
            WorldBuilder()
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, camera))
                .AddSystem(new DotRenderSystem(this.GraphicsDevice, camera))

                .AddSystem(new BulletSystem(playerBoundaries))
                .AddSystem(new BulletSpawnerSystem())
                .AddSystem(new EnemySpawnerSystem ())

                .AddSystem(new Collision.CollisionSystem(playerBoundaries))

                .AddSystem(new ConstrainedTransformSystem(playerBoundaries))
                .AddSystem(new TransformFollowerSystem ())

                .Build ()

        let leftShooter = world.CreateEntity()
        leftShooter.Attach (Dot(Size2(10f,10f), Color.GreenYellow))
        leftShooter.Attach playerBulletSpawners.Tail.Head
        leftShooter.Attach (new TransformFollower (playerTransform, Vector2(50f, -20f)))
        leftShooter.Attach (Transform2(playerTransform.Position))

        let rightShooter = world.CreateEntity()
        rightShooter.Attach (Dot(Size2(10f,10f), Color.GreenYellow))
        rightShooter.Attach playerBulletSpawners.Tail.Tail.Head
        rightShooter.Attach (new TransformFollower (playerTransform, Vector2(-50f, -20f)))
        rightShooter.Attach (Transform2(playerTransform.Position))

        let playerEntity = world.CreateEntity()
        playerEntity.Attach playerTransform
        playerEntity.Attach playerMover
        // for playerBulletSpawner in playerBulletSpawners do
        playerEntity.Attach playerBulletSpawners.Head
        playerEntity.Attach (Dot(Size2(10f,10f), Color.GreenYellow))
        playerEntity.Attach (Collision.TransformCollisionActor(playerTransform, 5f, playerTag, 
            onCollision = (fun other -> 
                if other.Tag = enemyTag then
                    player.HP <- player.HP - 10f
                    Console.WriteLine $"hit player (hp: {player.HP})"
                    
                let dead = player.HP <= 0f
                if dead then
                    world.DestroyEntity leftShooter
                    world.DestroyEntity rightShooter
                    ()

                not dead
            )))



        // Console.WriteLine "load enemy"

        let enemySpawner = world.CreateEntity ()
        enemySpawner.Attach (new EnemySpawner(5f, 4u))
        enemySpawner.Attach (new Transform2(Vector2(800f, 100f)))

        // let enemyEntity = world.CreateEntity()
        // let enemyTransform = Transform2(bulletTarget.Position.ToVector())
        // enemyEntity.Attach enemyTransform
        // enemyEntity.Attach (Collision.TransformCollisionActor(enemyTransform, bulletTarget.Radius, enemyTag, 
        //     onCollision = (fun other -> other.Tag <> playerTag)))
        // enemyEntity.Attach (Dot(Size2(20f,15f), Color.AliceBlue))
        // let enemyBulletSpawner = BulletSpawner (
        //     3f, 
        //     Vector2.UnitY* 150f,
        //     enemyTag,
        //     (fun other -> 
        //         let hit = other.Tag = playerTag
        //         if hit then
        //             Console.WriteLine "enemy bullet hit"
        //         not hit
        //     )
        // )
        // enemyBulletSpawner.Firing <- true
        // enemyEntity.Attach enemyBulletSpawner

        this.Components.Add (world)

        base.LoadContent ()


    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.PaleVioletRed

        spriteBatch.Begin (transformMatrix = camera.GetViewMatrix())
        spriteBatch.FillRectangle (playerBoundaries, Color.DarkSalmon)
        spriteBatch.DrawString (fira, "Danmaku", Vector2(800f, 100f), Color.WhiteSmoke);

        spriteBatch.End ()

        base.Draw gameTime

