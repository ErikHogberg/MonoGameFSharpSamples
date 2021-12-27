// module Game
namespace SpaceGame

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

// open MonoGame.Extended.Input.InputListeners

type Game1 () as self =
    inherit Game()

    do self.Content.RootDirectory <- "Content"

    let graphics = new GraphicsDeviceManager(self)
    let mutable spriteBatch : SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable rot = 0.0f

    [<DefaultValue>]
    val mutable dot : Texture2D

    override this.Initialize() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        this.IsFixedTimeStep <- false
        this.IsMouseVisible <- true

        graphics.PreferredBackBufferWidth <- 1280
        graphics.PreferredBackBufferHeight <- 720
        graphics.ApplyChanges ()

        base.Initialize ()
        ()

    override this.LoadContent() =
        // printfn "content root: %s" this.Content.RootDirectory
        // this.dot <- self.Content.Load "1px"
        this.dot <- this.Content.Load "1px"
        ()

    override this.Update(gameTime) =
        if Keyboard.GetState().IsKeyDown Keys.Escape then
            self.Exit()

        rot <- rot + float32 gameTime.ElapsedGameTime.Seconds

        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue
        let pos = new Vector2(0f, 0f) 
        spriteBatch.Draw (self.dot,pos, Color.Cornsilk)
        ()