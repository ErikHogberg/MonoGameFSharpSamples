// module Game
namespace SpaceGame

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

open MonoGame.Extended

type Game1() as self =
    inherit Game()

    do self.Content.RootDirectory <- "Content"

    let graphics = new GraphicsDeviceManager(self)
    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable rot = 0.0f

    [<DefaultValue>]
    val mutable dot: Texture2D

    [<DefaultValue>]
    val mutable font: SpriteFont

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
        this.dot <- self.Content.Load "1px"
        this.font <- self.Content.Load "Fira Code"
        ()

    override this.Update(gameTime) =
        if Keyboard.GetState().IsKeyDown Keys.Escape then
            self.Exit()

        rot <-
            rot
            + float32 gameTime.ElapsedGameTime.TotalSeconds

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
            self.dot,
            new Rectangle(100, 100, 100, 100),
            Nullable(),
            Color.Crimson,
            rot,
            Vector2.One * 0.5f,
            SpriteEffects.None,
            0f
        )

        spriteBatch.DrawString(
            self.font,
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
