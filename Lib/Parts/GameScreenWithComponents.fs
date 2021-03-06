module GameScreenWithComponents

open System
open Microsoft.Xna.Framework
open MonoGame.Extended.Screens


[<AbstractClass>]
type GameScreenWithComponents (game: Game, graphics: GraphicsDeviceManager) =
    inherit Screen()

    let game = game
    let graphics = graphics

    let mutable drawables = List<IDrawable>.Empty
    let mutable updateables = List<IUpdateable>.Empty
    let mutable components= new GameComponentCollection ()

    // TODO: check if working
    let rec remove (element : 'a, list: List<'a>, acc: List<'a>) =
        if list.Head = element then
            list.Tail @ acc
        else
            remove (element, list.Tail, list.Head :: acc)

    let remove (element : 'a, list: List<'a>) = remove (element, list, [])

    member this.Game with get () = game
    member this.Content with get () = game.Content
    member this.GraphicsDevice with get () = game.GraphicsDevice
    member this.Window with get () = game.Window
    member this.Services with get () = game.Services

    member this.Components with get () = components


    member this.CategorizeComponents () =
        this.DecategorizeComponents ()
        for component1 in components do
            this.CategorizeComponent component1;
        
    member this.DecategorizeComponents() =
        updateables <- []
        drawables <- []

    member this.CategorizeComponent (component1: IGameComponent ) =
        match component1 with
        | :? IUpdateable as a -> 
            // updateables <-  (component1 :?> IUpdateable) :: updateables
            updateables <- a :: updateables
        | _ -> ()

        // separate match for drawables in case component is both drawable and updateable
        match component1 with
        | :? IDrawable as b ->
            drawables <- b :: drawables
        | _ -> ()
    
        
    member this.DecategorizeComponent(component1: IGameComponent )=
        match component1 with
        | :? IUpdateable as a -> 
            updateables <- remove( a, updateables)
        | _ -> ()

        match component1 with
        | :? IDrawable as b ->
            drawables <-  remove (b, drawables)
        | _ -> ()
    
    override this.Initialize () =
        for component1 in components do
            component1.Initialize ()

        this.CategorizeComponents ()

        components.ComponentAdded.Add(
            fun e ->
                e.GameComponent.Initialize()
                this.CategorizeComponent e.GameComponent
                ()
        )
        
        components.ComponentRemoved.Add(
            fun e ->
                this.DecategorizeComponent e.GameComponent
                ()
        )

        graphics.PreferredBackBufferWidth <- graphics.GraphicsDevice.Viewport.Width
        graphics.PreferredBackBufferHeight <- graphics.GraphicsDevice.Viewport.Height
        graphics.ApplyChanges ()
        
        ()

    override this.Dispose () =
        for component1 in components do
            let disposable = component1 :?> IDisposable
            if disposable <> null then
                disposable.Dispose ()
        
        components <- null;
        ()

    override this.Update gameTime =
        for updateable in updateables do
            updateable.Update gameTime
        ()

    override this.Draw gameTime =
        for drawable in drawables do
            drawable.Draw gameTime
        ()
