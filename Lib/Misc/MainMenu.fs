namespace MainMenu

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
open Boids
open Bullets
open Tools
open UI

type MainMenu (game, graphics) =
    inherit GameScreenWithComponents (game, graphics)

    let mutable dot: Texture2D = null
    let mutable fira: SpriteFont = null

    let mutable camera: OrthographicCamera = null

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    
    override this.Initialize() =
        
        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera(viewportAdapter)
        
        base.Initialize()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        dot <- this.Content.Load "1px"
        fira <- this.Content.Load "Fira Code"


        let world =
            WorldBuilder()
                
                .AddSystem(new DotRenderSystem(this.GraphicsDevice, camera))
                .AddSystem(new ButtonSystem ())

                .Build()

        this.Components.Add world

        let buttonEntity = world.CreateEntity ()
        buttonEntity.Attach <| new Button(fun _ -> ())

        // TODO: add button which changes game screen

        base.LoadContent()
        ()

    override this.Update(gameTime) =
        let dt = gameTime.GetElapsedSeconds()

        base.Update gameTime
        ()

    override this.Draw(gameTime) =
        this.GraphicsDevice.Clear Color.PaleVioletRed

        spriteBatch.Begin(transformMatrix = camera.GetViewMatrix())

        spriteBatch.FillRectangle(0f,0f,1080f,1920f, Color.DarkCyan)


        spriteBatch.DrawString(fira, "menu", Vector2(600f, 100f), Color.WhiteSmoke);

        spriteBatch.End()

        base.Draw gameTime
        ()

