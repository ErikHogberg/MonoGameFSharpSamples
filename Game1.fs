module Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

type Game1 () as x =
    inherit Game()

    do x.Content.RootDirectory <- "Content"
    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    override x.Initialize() =
        spriteBatch <- new SpriteBatch(x.GraphicsDevice)
        base.Initialize()
        ()

    override this.LoadContent() =
        ()

    override this.Update(gameTime) =
        ()

    override this.Draw(gameTime) =
        x.GraphicsDevice.Clear Color.CornflowerBlue
        
        ()