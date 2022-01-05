namespace SpaceGame

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


open Ship
open Asteroids
open Tools
open RenderSystem
open UpdateSystem

type Game1() as x =
    inherit Game()

    do x.Content.RootDirectory <- "Content"

    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    [<DefaultValue>]
    val mutable camera: OrthographicCamera

    [<DefaultValue>]
    val mutable dot: Texture2D
    [<DefaultValue>]
    val mutable asteroids1: AsteroidShowerSystem


    let box = new RectangleF(600f, 200f, 50f,80f)
    let bubble = new EllipseF(new Vector2(600f, 300f), 50f,80f)

    let tweener = new Tweener()
    

    let mutable mouseListener = new MouseListener()
    let mutable touchListener = new TouchListener()


    override this.Initialize() =

        this.IsFixedTimeStep <- false
        this.IsMouseVisible <- true

        this.Window.AllowUserResizing <- true

        graphics.PreferredBackBufferWidth <- 1280
        graphics.PreferredBackBufferHeight <- 720
        graphics.PreferMultiSampling <- false
        graphics.ApplyChanges()

        let listenerComponent =
            new InputListenerComponent(this, mouseListener, touchListener)

        this.Components.Add listenerComponent

        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)

        this.camera <- new OrthographicCamera(viewportAdapter)

        let tween = 
            tweener.TweenTo(bubble, (fun bubble -> bubble.RadiusX), 100f, 1f, 1f)
                .RepeatForever(0.5f)
                .AutoReverse()
                // .Easing(EasingFunctions.Linear)

        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // printfn "content root: %s" this.Content.RootDirectory
        this.dot <- this.Content.Load "1px"

        // Singleton.Instance.Set("dot", this.dot)


        this.asteroids1 <- new AsteroidShowerSystem(new EllipseF(bubble.Center, 400f, 250f), Vector2.UnitY * 100f)
        this.asteroids1.Bubble <- bubble

        let worldBuilder = new WorldBuilder()

        let world =
            worldBuilder
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, this.camera))
                .AddSystem(new TransformUpdateSystem())

                .AddSystem(this.asteroids1)
                .AddSystem(new AsteroidExpirySystem())
                .AddSystem(new AsteroidRenderSystem(this.GraphicsDevice, this.camera))

                .Build()

        this.Components.Add(world)

        let testEntity = world.CreateEntity()
        testEntity.Attach(new Transform2(100f, 300f, 0f, 100f, 100f))
        let mutable dotSprite = new Sprite(this.dot)
        dotSprite.Color <- Color.Goldenrod
        testEntity.Attach(dotSprite)

        ()

    override this.Update(gameTime) =
        if Keyboard.GetState().IsKeyDown Keys.Escape then
            this.Exit()

        let dt = gameTime.GetElapsedSeconds()

        tweener.Update(dt)

        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue

        spriteBatch.Begin(transformMatrix = this.camera.GetViewMatrix())

        // spriteBatch.DrawRectangle(box, Color.AliceBlue)
        spriteBatch.DrawEllipse(bubble.Center, new Vector2(bubble.RadiusX, bubble.RadiusY), 32, Color.Azure)

        spriteBatch.End()

        base.Draw gameTime
        ()
