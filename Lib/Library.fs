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


open Ship
open Tools
open RenderSystem
open UpdateSystem
open Rain

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
    val mutable rain1: RainfallSystem

    // [<DefaultValue>]
    // val mutable ship: Texture2D

    // [<DefaultValue>]
    // val mutable font: SpriteFont


    let mutable mouseListener = new MouseListener()

    // [<DefaultValue>]
    // val mutable ship1: SpaceShip // = new SpaceShip(ship)


    override this.Initialize() =

        this.IsFixedTimeStep <- false
        this.IsMouseVisible <- true

        this.Window.AllowUserResizing <- true

        graphics.PreferredBackBufferWidth <- 1280
        graphics.PreferredBackBufferHeight <- 720
        graphics.PreferMultiSampling <- false
        graphics.ApplyChanges()

        let listenerComponent =
            new InputListenerComponent(this, mouseListener)

        this.Components.Add listenerComponent

        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720)

        this.camera <- new OrthographicCamera(viewportAdapter)


        base.Initialize()
        ()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // printfn "content root: %s" this.Content.RootDirectory
        this.dot <- this.Content.Load "1px"
        // this.ship <- this.Content.Load "ship"
        // this.font <- this.Content.Load "Fira Code"

        // let dot = this.Content.Load "1px"

        // Singleton.Instance.Set("dot", this.dot)

        // this.ship1 <- new SpaceShip(this.ship, new Point(100, 100), 150)

        this.rain1 <- new RainfallSystem(new Rectangle(100, 100, 1000, 400))
        this.rain1.WindStrength <- 60f

        let worldBuilder = new WorldBuilder()

        let world =
            worldBuilder
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, this.camera))
                .AddSystem(new TransformUpdateSystem())

                .AddSystem(this.rain1)
                .AddSystem(new RainExpirySystem())
                .AddSystem(new RainRenderSystem(this.GraphicsDevice, this.camera))

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

        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue

        base.Draw gameTime
        ()
