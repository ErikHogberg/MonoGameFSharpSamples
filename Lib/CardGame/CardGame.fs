namespace CardGame

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
open MonoGame.Extended.Tiled
open MonoGame.Extended.Tiled.Renderers

open Myra
open Myra.Graphics2D.UI

open GameScreenWithComponents
open Tools      
open TransformUpdater
open RenderSystem
open System.IO

type CardGame (game, graphics) =
    inherit GameScreenWithComponents (game, graphics)

    let mutable fira: SpriteFont = null
    let mutable camera: OrthographicCamera = null
    let mutable spriteBatch: SpriteBatch = null//Unchecked.defaultof<SpriteBatch>

    let mutable desktop: Desktop = null;

    let mouseListener = MouseListener ()
    let touchListener = TouchListener ()
    let kbdListener = KeyboardListener ()


    override this.Initialize () =
        
        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera viewportAdapter


        let listenerComponent =
            new InputListenerComponent(this.Game, mouseListener, touchListener, kbdListener)

        this.Components.Add listenerComponent

        kbdListener.KeyPressed.Add <| fun args  ->
            match args.Key with 
            | Keys.E ->
                ()
            | _ -> ()


        // kbdListener.KeyReleased.Add <| fun args ->
        //     match args.Key with 
        //     | Keys.E ->
        //         ()
        //     | _ -> ()
            
        base.Initialize ()


    override this.LoadContent () =

        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        fira <- this.Content.Load "Fira Code"

        // ecs

        let world =
            WorldBuilder()
                .AddSystem(new SpriteRenderSystem(this.GraphicsDevice, camera))
                .AddSystem(new DotRenderSystem(this.GraphicsDevice, camera))

                .AddSystem(new TransformFollowerSystem ())
                .AddSystem(new TweenTransformerSystem())

                .AddSystem(new ecs.DelayedActionSystem())

                .Build ()


        this.Components.Add world

        // myra ui

        MyraEnvironment.Game <- this.Game

        let grid = Grid(
                RowSpacing = 8, 
                ColumnSpacing = 8
            )

        grid.ColumnsProportions.Add <| Proportion(ProportionType.Auto)
        grid.ColumnsProportions.Add <| Proportion(ProportionType.Auto)
        grid.RowsProportions.Add <| Proportion(ProportionType.Auto)
        grid.RowsProportions.Add <| Proportion(ProportionType.Auto)

        let helloWorld = Label (
            Id = "label",
            Text = "Hello, World!"
            )
        grid.Widgets.Add(helloWorld);

        // ComboBox
        let combo = ComboBox (
            GridColumn = 1,
            GridRow = 0
            )

        combo.Items.Add <| ListItem("Red", Color.Red)
        combo.Items.Add <| ListItem("Green", Color.Green)
        combo.Items.Add <| ListItem("Blue", Color.Blue)
        grid.Widgets.Add combo

        // Button
        let button = TextButton (
            GridColumn = 0,
            GridRow = 1,
            Text = "Show"
        )

        button.Click.Add(fun a ->
            let messageBox = Dialog.CreateMessageBox("Message", "Some message!")
            messageBox.ShowModal(desktop)
            )

        grid.Widgets.Add button

        // Spin button
        let spinButton = SpinButton (
            GridColumn = 1,
            GridRow = 1,
            Width = 100,
            Nullable = true
        )
        grid.Widgets.Add spinButton

        let panel = Panel ()

        grid.Left <- 200
        grid.Top <- 100
        let _ = panel.AddChild grid

        let label = Label ()
        label.Text <- "Card Game"
        label.Left <- 150
        label.Top <- 50

        let _ = panel.AddChild label

        let data = System.IO.File.ReadAllText(AppContext.BaseDirectory + "/Raw/myra/card.xmmp")
        
        let portraits = [
            (this.Content.Load<Texture2D> "ship");
            (this.Content.Load<Texture2D> "1px");
            (this.Content.Load<Texture2D> "ship");
        ]
        
        let horLayout = HorizontalStackPanel ()
        
        horLayout.Top <- 400
        horLayout.Left <- 400

        for portrait in portraits do
            let project = Project.LoadFromXml data
            
            let portraitImage = (project.Root.FindWidgetById "portrait") :?> Image
            portraitImage.Renderable <- Myra.Graphics2D.TextureAtlases.TextureRegion (portrait)

            let _ = horLayout.AddChild project.Root
            ()

        let _ = panel.AddChild horLayout

        // Add it to the desktop
        desktop <- Desktop ()
        desktop.Root <- panel

        base.LoadContent ()


    override this.Draw gameTime =
        
        this.GraphicsDevice.Clear Color.PaleVioletRed

        // spriteBatch.Begin ()
        // // spriteBatch.Begin (transformMatrix = camera.GetViewMatrix())
        // spriteBatch.DrawString (fira, "Card game", Vector2(100f, 100f), Color.WhiteSmoke)
        // spriteBatch.End ()
        
        desktop.Render ()

        base.Draw gameTime

