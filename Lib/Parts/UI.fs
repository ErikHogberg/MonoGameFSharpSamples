module UI

open Microsoft.Xna.Framework
open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems

open RenderSystem
open Tools
/// <summary>A clickable button. Requires Dot, Transform and SizeComponent </summary>
type Button(onClick: unit->unit) =

    member this.OnClick = onClick

type ButtonSystem (camera: OrthographicCamera) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Button>, typedefof<Transform2>, typedefof<Dot>, typedefof<SizeComponent>))    

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable buttonMapper: ComponentMapper<Button> = null
    let mutable dotMapper: ComponentMapper<Dot> = null
    let mutable sizeMapper: ComponentMapper<SizeComponent> = null

    let mutable hoverPos = Point2.Zero
    let mutable queuedClick = false
    let mutable mouseDown = false

    member this.HoverPos with set (value) = hoverPos <- value
    member this.QueuedClick with set (value) = queuedClick <- value
    member this.MouseDown with set (value) = mouseDown <- value

    override this.Initialize (mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper ()
        buttonMapper <- mapperService.GetMapper ()
        dotMapper <- mapperService.GetMapper ()
        sizeMapper <- mapperService.GetMapper ()

    override this.Update gameTime =
        // let dt = gameTime.GetElapsedSeconds()

        let currentHoverPos = (camera.ScreenToWorld hoverPos).ToPoint2

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let button = buttonMapper.Get entityId
            let dot = dotMapper.Get entityId
            let size = (sizeMapper.Get entityId).Size

            let pos = transform.Position.ToPoint2 - size
            let fullSize = size * 2f
            
            let rect = RectangleF(pos , fullSize)
            
            if rect.Contains currentHoverPos then
                if queuedClick then
                    button.OnClick ()
                    queuedClick <- false
                    dot.Color <- Color.White
                else
                    if mouseDown then
                        dot.Color <- Color.White
                    else
                        dot.Color <- Color.DarkBlue
            else
                dot.Color <- Color.DarkGray

            ()

        queuedClick <- false

        ()