module GameScreenWithComponents

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended.Screens
open System



[<AbstractClass>]
type GameScreenWithComponents (game: Game) =
    inherit Screen()

    let game = game

    let mutable drawables = List<IDrawable>.Empty
    let mutable updateables = List<IUpdateable>.Empty
    let mutable components= new GameComponentCollection()

    // TODO: check if working
    let rec remove (element : 'a, list: List<'a>, acc: list<'a>) =
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


    // member this._drawables =
    //     new SortingFilteringCollection<IDrawable>(
    //         d => d.Visible,
    //         (d, handler) => d.VisibleChanged += handler,
    //         (d, handler) => d.VisibleChanged -= handler,
    //         (d1 ,d2) => Comparer<int>.Default.Compare(d1.DrawOrder, d2.DrawOrder),
    //         (d, handler) => d.DrawOrderChanged += handler,
    //         (d, handler) => d.DrawOrderChanged -= handler);

    // member this._updateables =
    //     new SortingFilteringCollection<IUpdateable>(
    //         u => u.Enabled,
    //         (u, handler) => u.EnabledChanged += handler,
    //         (u, handler) => u.EnabledChanged -= handler,
    //         (u1, u2) => Comparer<int>.Default.Compare(u1.UpdateOrder, u2.UpdateOrder),
    //         (u, handler) => u.UpdateOrderChanged += handler,
    //         (u, handler) => u.UpdateOrderChanged -= handler);

    member this.CategorizeComponents() =
        this.DecategorizeComponents()
        // for i in 0.. components.Count do
        //     this.CategorizeComponent(components[i]);
        for component1 in components do
            this.CategorizeComponent(component1);
        
        

    member this.DecategorizeComponents() =
        updateables <- []
        drawables <- []


    member this.CategorizeComponent (component1: IGameComponent ) =
        match component1 with
        | :? IUpdateable as a -> 
            // updateables <-  (component1 :?> IUpdateable) :: updateables
            updateables <-  a :: updateables
        | _ -> ()

        match component1 with
        | :? IDrawable as b ->
            drawables <-  b :: drawables
        | _ -> ()
    
        
    member this.DecategorizeComponent(component1: IGameComponent )=
        match component1 with
        | :? IUpdateable as a -> 
            updateables <-  remove( a, updateables)
        | _ -> ()

        match component1 with
        | :? IDrawable as b ->
            drawables <-  remove(b, drawables)
        | _ -> ()
    
    override this.Initialize() =
        // for i in 0 .. components.Count do
        //    components[i].Initialize();
        for component1 in components do
            component1.Initialize();

        this.CategorizeComponents()

        components.ComponentAdded.Add(
            // fun (sender: Object, e: GameComponentCollectionEventArgs) ->
            fun (e: GameComponentCollectionEventArgs) ->
                e.GameComponent.Initialize()
                this.CategorizeComponent(e.GameComponent)
                ()
        )
        
        components.ComponentRemoved.Add(
            // fun (sender: Object, e: GameComponentCollectionEventArgs) ->
            fun (e: GameComponentCollectionEventArgs) ->
                this.DecategorizeComponent(e.GameComponent)
                ()
        )
        
        ()

    override this.Dispose() =
        for i in 0 .. this.Components.Count do
        
            let disposable = this.Components[i] :?> IDisposable
            if disposable <> null then
                disposable.Dispose();
        
        components <- null;
        ()

    override this.Update(gameTime: GameTime) =
        for updateable in updateables do
            updateable.Update(gameTime)
        ()

    override this.Draw(gameTime: GameTime) =
        for drawable in drawables do
            drawable.Draw(gameTime)
        ()

        

    
