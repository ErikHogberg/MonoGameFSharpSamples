module SpaceTactics

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

type SpaceGame(game, graphics) =
    inherit GameScreenWithComponents(game, graphics)

    let mutable dot: Texture2D = null
    let mutable fira: SpriteFont = null
    let mutable camera: OrthographicCamera = null
    let mutable asteroids1: AsteroidShowerSystem = Unchecked.defaultof<AsteroidShowerSystem>
    let mutable asteroidsRenderSystem: AsteroidRenderSystem = Unchecked.defaultof<AsteroidRenderSystem>

    let mutable spriteBatch: SpriteBatch = null

    let box = RectangleF(600f, 200f, 50f,80f)
    let bubble = EllipseF(Vector2(600f, 400f), 50f,80f)

    let mutable asteroidAngle = 0f

    let tweener = new Tweener()

    let mouseListener = MouseListener()
    let touchListener = TouchListener()
    let kbdListener = KeyboardListener()

    
    override this.Initialize() =
        
        let viewportAdapter =
            // new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera(viewportAdapter)

        let easingFn =  Func<float32, float32>(EasingFunctions.QuadraticIn)

        let tween = 
            tweener.TweenTo(bubble, (fun bubble -> bubble.RadiusX), 100f, 1f, 1f)
                .RepeatForever(0.5f)
                .AutoReverse()
                .Easing(easingFn)

        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        kbdListener.KeyPressed.Add(fun args  ->
            if args.Key = Keys.Space then
                asteroidsRenderSystem.AlwaysShow <- not asteroidsRenderSystem.AlwaysShow
                ()
            ())

        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        dot <- this.Content.Load "1px"
        fira <- this.Content.Load "Fira Code"

        asteroids1 <- new AsteroidShowerSystem(EllipseF(bubble.Center, 300f, 200f))
        asteroids1.Bubble <- bubble

        asteroidsRenderSystem <- new AsteroidRenderSystem(this.GraphicsDevice, camera)

        let world =
            WorldBuilder()
                
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, camera))
                // .AddSystem(new TransformUpdateSystem())

                .AddSystem(asteroids1)
                .AddSystem(new AsteroidExpirySystem())
                .AddSystem(asteroidsRenderSystem)

                .Build()

        this.Components.Add(world)

        let testEntity = world.CreateEntity()
        testEntity.Attach(Transform2(100f, 300f, 0f, 100f, 100f))
        let dotSprite = Sprite(dot)
        dotSprite.Color <- Color.Goldenrod
        testEntity.Attach(dotSprite)

        base.LoadContent()
        ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // TODO: tweener component or entity?
        tweener.Update(dt)

        asteroidAngle <- (asteroidAngle + dt * 0.15f) %  (MathF.PI*2f)
        asteroids1.SpawnAngle <- asteroidAngle

        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.Gray

        spriteBatch.Begin(transformMatrix = camera.GetViewMatrix())

        spriteBatch.DrawEllipse(bubble.Center, Vector2(bubble.RadiusX, bubble.RadiusY), 32, Color.Azure)

        let pointOnBoundary = asteroids1.PointOnBoundary
        
        spriteBatch.DrawCircle(pointOnBoundary, 5f, 12, Color.Black)
        let rect = asteroids1.SpawnRange()
        let topleft =(Vector2(rect.TopLeft.X, rect.TopLeft.Y)).Rotate(asteroids1.SpawnAngle) + pointOnBoundary
        let topright=(Vector2(rect.TopRight.X, rect.TopRight.Y)).Rotate(asteroids1.SpawnAngle)  + pointOnBoundary
        let bottomleft =(Vector2(rect.BottomLeft.X, rect.BottomLeft.Y) ).Rotate(asteroids1.SpawnAngle) + pointOnBoundary
        let bottomright =(Vector2(rect.BottomRight.X, rect.BottomRight.Y)).Rotate(asteroids1.SpawnAngle) + pointOnBoundary

        spriteBatch.DrawLine(topleft, topright, Color.Brown)
        spriteBatch.DrawLine(topleft, bottomleft, Color.Brown)
        spriteBatch.DrawLine(bottomright, topright, Color.Brown)
        spriteBatch.DrawLine(bottomright, bottomleft, Color.Brown)

        spriteBatch.DrawString(fira, "Space Game", Vector2(600f, 100f), Color.WhiteSmoke);

        spriteBatch.End()

        base.Draw gameTime
        ()