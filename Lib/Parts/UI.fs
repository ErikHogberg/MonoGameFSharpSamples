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

    let camera = camera
    

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable buttonMapper: ComponentMapper<Button> = null
    let mutable dotMapper: ComponentMapper<Dot> = null
    let mutable sizeMapper: ComponentMapper<SizeComponent> = null

    let mutable hoverPos = Point2.Zero
    let mutable queuedClick = false

    member this.HoverPos with set (value) = hoverPos <- value
    member this.QueuedClick with set (value) = queuedClick <- value

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper()
        buttonMapper <- mapperService.GetMapper()
        dotMapper <- mapperService.GetMapper()
        sizeMapper <- mapperService.GetMapper()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        let transformMatrix = camera.GetViewMatrix()
        // let currentHoverPos = Vector2.Transform(hoverPos, transformMatrix)
        // let currentHoverPos = Vector2.Transform(hoverPos, transformMatrix)
        let mutable currentHoverPos = hoverPos
        // TODO: inverse transform mouse position
        // let success, scale, rotation, translation = transformMatrix.Decompose()
        // if success then 
        //     currentHoverPos <-  currentHoverPos - Vector2( translation.X, translation.Y)
        //     // currentHoverPos <-  Quaternion. currentHoverPos 

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let button = buttonMapper.Get entityId
            let dot = dotMapper.Get entityId
            let size = (sizeMapper.Get entityId).Size

            let pos = transform.Position.ToPoint2 - size//Vector2.Transform(transform.Position.ToPoint2 - size, transformMatrix)
            let fullSize = size*2f//Vector2.Transform(size *2f, transformMatrix)
            
            let rect = RectangleF( pos , fullSize).Transform(transformMatrix)
            


            // TODO: call click event on release instead of press

            if rect.Contains currentHoverPos then
                if queuedClick then
                    button.OnClick ()
                    queuedClick <- false
                    dot.Color <- Color.White
                else
                    dot.Color <- Color.DarkBlue
            else
                dot.Color <- Color.DarkGray

            ()

        ()