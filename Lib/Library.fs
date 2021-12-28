namespace SpaceGame

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

open MonoGame.Extended
open MonoGame.Extended.Input.InputListeners

open Ship
open Tools

type Game1() as x =
    inherit Game()

    do x.Content.RootDirectory <- "Content"

    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable rot = 0.0f

    [<DefaultValue>]
    val mutable dot: Texture2D

    [<DefaultValue>]
    val mutable ship: Texture2D

    [<DefaultValue>]
    val mutable font: SpriteFont

    let mutable mouseListener = new MouseListener()

    [<DefaultValue>]
    val mutable ship1: SpaceShip// = new SpaceShip(ship)


    override this.Initialize() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        this.IsFixedTimeStep <- false
        this.IsMouseVisible <- true

        graphics.PreferredBackBufferWidth <- 1280
        graphics.PreferredBackBufferHeight <- 720
        graphics.PreferMultiSampling <- false
        graphics.ApplyChanges()

        base.Initialize()
        ()

    override this.LoadContent() =
        // printfn "content root: %s" this.Content.RootDirectory
        this.dot <- this.Content.Load "1px"
        this.ship <- this.Content.Load "ship"
        this.font <- this.Content.Load "Fira Code"

        Singleton.Instance.Set("dot", this.dot)

        this.ship1 <- new SpaceShip(this.ship, new Point(100, 100) ,150)
        ()

    override this.Update(gameTime) =
        if Keyboard.GetState().IsKeyDown Keys.Escape then
            this.Exit()

        rot <-
            rot
            // + float32 gameTime.ElapsedGameTime.TotalSeconds
            + gameTime.GetElapsedSeconds()

        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise
        )

        spriteBatch.Draw(
            this.dot,
            new Rectangle(100, 100, 100, 100),
            Nullable(),
            Color.Crimson,
            rot,
            Vector2.One * 0.5f,
            SpriteEffects.None,
            0f
        )
        
        // spriteBatch.Draw(
        //     this.ship,
        //     new Rectangle(200, 100, 100, 100),
        //     Color.Crimson
        // )
        this.ship1.Draw spriteBatch

        spriteBatch.DrawString(
            this.font,
            "Fira Code",
            new Vector2(200f, 30f),
            Color.Gold,
            0f,
            Vector2.One * 0.5f,
            Vector2.One * 3f,
            SpriteEffects.FlipVertically,
            1f
        )

        spriteBatch.End()
        ()
