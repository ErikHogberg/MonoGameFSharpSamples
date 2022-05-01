module SpaceTactics

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Input.InputListeners
open MonoGame.Extended.Entities
open MonoGame.Extended.Sprites
open MonoGame.Extended.ViewportAdapters
open MonoGame.Extended.Tweening

open RenderSystem
open GameScreenWithComponents
open Asteroids
open TransformUpdater

type SpaceGame(game, graphics) =
    inherit GameScreenWithComponents(game, graphics)

    let mutable dot: Texture2D = null
    let mutable fira: SpriteFont = null
    let mutable camera: OrthographicCamera = null
    let mutable asteroids1: AsteroidShowerSystem option = None
    let mutable asteroidsRenderSystem: AsteroidRenderSystem option =None

    let mutable spriteBatch: SpriteBatch = null

    let box = RectangleF(600f, 200f, 50f,80f)
    let bubble = EllipseF(Vector2(600f, 400f), 50f,80f)

    let mutable asteroidAngle = 0f

    let mouseListener = MouseListener ()
    let touchListener = TouchListener ()
    let kbdListener = KeyboardListener ()

    
    override this.Initialize () =
        
        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera viewportAdapter

        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        kbdListener.KeyPressed.Add <| fun args  ->
            if args.Key = Keys.Space then
                match asteroidsRenderSystem with
                    | Some a -> a.AlwaysShow <- not a.AlwaysShow
                    | None -> ()

        base.Initialize ()

    override this.LoadContent () =
        spriteBatch <- new SpriteBatch (this.GraphicsDevice)

        dot <- this.Content.Load "1px"
        fira <- this.Content.Load "Fira Code"

        let tempAsteroids = new AsteroidShowerSystem(EllipseF(bubble.Center, 300f, 200f))
        tempAsteroids.Bubble <- bubble
        asteroids1 <- Some tempAsteroids

        let tempAsteroidsRender = new AsteroidRenderSystem(this.GraphicsDevice, camera)
        asteroidsRenderSystem <- Some tempAsteroidsRender


        let world =
            WorldBuilder()
                
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, camera))

                .AddSystem(tempAsteroids)
                .AddSystem(tempAsteroidsRender)
                .AddSystem(new AsteroidExpirySystem())

                .AddSystem(new TweenTransformerSystem())

                .Build()

        this.Components.Add world

        let testEntity = world.CreateEntity ()
        testEntity.Attach <| Transform2 (100f, 300f, 0f, 100f, 100f)
        let dotSprite = Sprite dot
        dotSprite.Color <- Color.Goldenrod
        testEntity.Attach dotSprite

        let bubbleEntity = world.CreateEntity ()
        bubbleEntity.Attach <| TweenTransformer (TweenTransformer.StretchTweener( 
            bubble, 
            100f, 
            0.5f, 
            1f, 
            EasingFunctions.QuadraticIn))

        base.LoadContent ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds ()

        asteroidAngle <- (asteroidAngle + dt * 0.15f) % (MathF.PI * 2f)
        match asteroids1 with
        | Some a ->a.SpawnAngle <- asteroidAngle
        | None -> ()
        
        base.Update gameTime

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.Gray

        spriteBatch.Begin(transformMatrix = camera.GetViewMatrix())

        spriteBatch.DrawEllipse(bubble.Center, Vector2(bubble.RadiusX, bubble.RadiusY), 32, Color.Azure)

        let spawnAngle = 
            match asteroids1 with
            | Some a -> a.SpawnAngle
            | None -> 0f
        
        let pointOnBoundary = 
            match asteroids1 with
            | Some a -> a.PointOnBoundary
            | None -> Vector2.Zero
        
        spriteBatch.DrawCircle(pointOnBoundary, 5f, 12, Color.Black)
        let rect = 
            match asteroids1 with
            | Some a -> a.SpawnRange ()
            | None -> RectangleF.Empty

        let topleft = (Vector2(rect.TopLeft.X, rect.TopLeft.Y)).Rotate(spawnAngle) + pointOnBoundary
        let topright= (Vector2(rect.TopRight.X, rect.TopRight.Y)).Rotate(spawnAngle)  + pointOnBoundary
        let bottomleft = (Vector2(rect.BottomLeft.X, rect.BottomLeft.Y) ).Rotate(spawnAngle) + pointOnBoundary
        let bottomright = (Vector2(rect.BottomRight.X, rect.BottomRight.Y)).Rotate(spawnAngle) + pointOnBoundary

        spriteBatch.DrawLine (topleft, topright, Color.Brown)
        spriteBatch.DrawLine (topleft, bottomleft, Color.Brown)
        spriteBatch.DrawLine (bottomright, topright, Color.Brown)
        spriteBatch.DrawLine (bottomright, bottomleft, Color.Brown)

        spriteBatch.DrawString (fira, "Space Game", Vector2(600f, 100f), Color.WhiteSmoke);

        spriteBatch.End ()

        base.Draw gameTime