namespace CardGame

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.ViewportAdapters

open Myra
open Myra.Graphics2D.UI

open GameScreenWithComponents
open Tools      
open TransformUpdater
open RenderSystem

type CardGame (game, graphics) =
    inherit GameScreenWithComponents (game, graphics)

    let mutable fira: SpriteFont = null
    let mutable camera: OrthographicCamera = null
    let mutable spriteBatch: SpriteBatch = null

    let mutable desktop: Desktop = null;

    override this.Initialize () =
        
        let viewportAdapter =
            new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1920, 1080)

        camera <- OrthographicCamera viewportAdapter

        base.Initialize ()


    override this.LoadContent () =

        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        fira <- this.Content.Load "Fira Code"

        MyraEnvironment.Game <- this.Game

        // Example from myra docs of adding ui elements programmatically

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

        let combo = 
            ComboBox (
                GridColumn = 1,
                GridRow = 0
            )

        combo.Items.Add <| ListItem ("Red", Color.Red)
        combo.Items.Add <| ListItem ("Green", Color.Green)
        combo.Items.Add <| ListItem ("Blue", Color.Blue)
        grid.Widgets.Add combo

        let button = 
            TextButton (
                GridColumn = 0,
                GridRow = 1,
                Text = "Show"
            )

        button.Click.Add(fun a ->
            let messageBox = Dialog.CreateMessageBox("Message", "Some message!")
            messageBox.ShowModal(desktop)
            )

        grid.Widgets.Add button

        let spinButton = SpinButton (
            GridColumn = 1,
            GridRow = 1,
            Width = 100,
            Nullable = true
        )
        grid.Widgets.Add spinButton

        // create a panel as UI root
        let panel = Panel ()

        // set a position offset to the example ui and add it to the panel
        grid.Left <- 200
        grid.Top <- 100
        let _ = panel.AddChild grid

        // add a label to the root
        let label = Label ()
        label.Text <- "Card Game"
        label.Left <- 150
        label.Top <- 50
        let _ = panel.AddChild label

        // load a myra project from disk
        let data = System.IO.File.ReadAllText $"{AppContext.BaseDirectory}/Raw/myra/card.xmmp"
        
        // load a few textures 
        let portraits = [
            this.Content.Load<Texture2D> "ship";
            this.Content.Load<Texture2D> "1px";
            this.Content.Load<Texture2D> "ship";
        ]
        
        // create a automatic horizantal arranging panel for containing the loaded UI project(s)
        let horLayout = HorizontalStackPanel ()
        
        // move it
        horLayout.Top <- 400
        horLayout.Left <- 400

        // create an instance of the loaded ui for each prepared texture, add the instances to the horizontally stacking panel
        for portrait in portraits do
            let project = Project.LoadFromXml data
            
            // set the "portrait" image from the project to the texture
            let portraitImage = (project.Root.FindWidgetById "portrait") :?> Image
            portraitImage.Renderable <- Graphics2D.TextureAtlases.TextureRegion portrait

            // set the "name" label from the project to the name of the texture
            let cardName = (project.Root.FindWidgetById "name") :?> Label
            cardName.Text <- portrait.Name

            // make clicking anywhere on the instance do something (write to console in this case)
            project.Root.TouchDown.Add(fun a -> Console.WriteLine $"pressed {portrait.Name}")

            let _ = horLayout.AddChild project.Root
            ()

        let _ = panel.AddChild horLayout

        // Add the root panel to the desktop
        desktop <- Desktop ()
        desktop.Root <- panel

        base.LoadContent ()


    override this.Draw gameTime =
        
        this.GraphicsDevice.Clear Color.PaleVioletRed
        
        // draw the ui
        desktop.Render ()

        base.Draw gameTime

