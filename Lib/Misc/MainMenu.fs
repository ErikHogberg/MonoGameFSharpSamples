namespace MainMenu

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Input.InputListeners
open MonoGame.Extended.Entities
open MonoGame.Extended.ViewportAdapters

open RenderSystem
open GameScreenWithComponents
open Tools
open UI
open MonoGame.Extended.Screens.Transitions

type MainMenu (game, graphics, screenCalls: List<unit -> GameScreenWithComponents>) =
    inherit GameScreenWithComponents (game, graphics)

    let screenCalls = screenCalls

    // let mutable dot: Texture2D = null
    let mutable fira: SpriteFont = null

    let mutable camera: OrthographicCamera = null

    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    
    override this.Initialize () =
        
        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera(viewportAdapter)


        base.Initialize ()

    override this.LoadContent () =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // dot <- this.Content.Load "1px"
        fira <- this.Content.Load "Fira Code"

        let mouseListener = MouseListener ()
        let touchListener = TouchListener ()
        let kbdListener = KeyboardListener ()

        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        let buttonSystem = new ButtonSystem(camera)
        mouseListener.MouseMoved.Add <| fun e -> buttonSystem.HoverPos <- e.Position 
        mouseListener.MouseClicked.Add <| fun _ -> buttonSystem.QueuedClick <- true
        mouseListener.MouseDown.Add <| fun _ -> buttonSystem.MouseDown <- true
        mouseListener.MouseUp.Add <| fun _ -> buttonSystem.MouseDown <- false

        this.Components.Add listenerComponent


        let world =
            WorldBuilder()
                
                .AddSystem(new DotRenderSystem(this.GraphicsDevice, camera))
                .AddSystem(buttonSystem)

                .Build()

        this.Components.Add world
        
        let buttonEntity = world.CreateEntity ()
        buttonEntity.Attach <| new Button(fun _ -> this.ScreenManager.LoadScreen(screenCalls.[1] (), new FadeTransition(this.GraphicsDevice, Color.Black, 0.5f)))
        buttonEntity.Attach <| new Transform2(300f, 300f)
        buttonEntity.Attach <| new SizeComponent(200f, 70f)
        buttonEntity.Attach <| new Dot(Color.Magenta)

        base.LoadContent ()

    override this.Update gameTime =
        // let dt = gameTime.GetElapsedSeconds ()

        base.Update gameTime

    override this.Draw gameTime =
        this.GraphicsDevice.Clear Color.PaleVioletRed

        spriteBatch.Begin (transformMatrix = camera.GetViewMatrix())

        spriteBatch.FillRectangle (50f,50f,1500f,900f, Color.DarkCyan)

        spriteBatch.DrawString (fira, "menu", Vector2(600f, 100f), Color.WhiteSmoke)

        spriteBatch.End ()

        base.Draw gameTime

