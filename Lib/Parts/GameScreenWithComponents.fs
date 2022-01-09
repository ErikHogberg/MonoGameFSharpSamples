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

    member this.Game with get () = game
    member this.Content with get () = game.Content
    member this.GraphicsDevice with get () = game.GraphicsDevice
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
        for i in 0.. components.Count do
            this.CategorizeComponent(components[i]);
        
        

    member this.DecategorizeComponents() =
        updateables <- []
        drawables <- []

    member this.CategorizeComponent (component1: IGameComponent ) =
        // match component1 with
        // | IUpdateable a -> 
        //     updateables <-  (component1 :?> IUpdateable) :: updateables
        // | IDrawable b ->
        //     updateables <-  (component1 :?> IUpdateable) :: updateables
    
        // if (component1 is IUpdateable) then
        //     updateables <-  (component1 :?> IUpdateable) :: updateables
        // if (component1 is IDrawable) then
        //     _drawables.Add((IDrawable)component);
        ()

    member this.DecategorizeComponent(component1: IGameComponent )=
        // if (component is IUpdateable)
        //     _updateables.Remove((IUpdateable)component);
        // if (component is IDrawable)
        //     _drawables.Remove((IDrawable)component);
        ()

    override this.Initialize() =
        for i in 0 .. components.Count do
            components[i].Initialize();
        
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
        // base.Update(gameTime)
        ()

    override this.Draw(gameTime: GameTime) =
        for drawable in drawables do
            drawable.Draw(gameTime)
        ()
        

    
