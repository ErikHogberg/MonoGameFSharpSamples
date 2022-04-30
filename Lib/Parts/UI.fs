module UI

open Microsoft.Xna.Framework
open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open RenderSystem

type Button(onClick: unit->unit) =

    member this.OnClick = onClick

type ButtonSystem () =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Button>, typedefof<Transform2>, typedefof<Dot>, typedefof<SizeComponent>))

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable buttonMapper: ComponentMapper<Button> = null
    let mutable dotMapper: ComponentMapper<Dot> = null
    let mutable sizeMapper: ComponentMapper<SizeComponent> = null

    let mutable hoverPos = Point2.Zero
    let mutable queuedClick: Option<Point2> = None

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper()
        buttonMapper <- mapperService.GetMapper()
        dotMapper <- mapperService.GetMapper()
        sizeMapper <- mapperService.GetMapper()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let button = buttonMapper.Get entityId
            let dot = dotMapper.Get entityId
            let size = (sizeMapper.Get entityId).Size

            let rect = RectangleF(transform.Position, size)
            
            if rect.Contains hoverPos then
                dot.Color <- Color.DarkBlue
            else
                dot.Color <- Color.DarkGray

            match queuedClick with
            | Some pos ->
                if rect.Contains pos then
                    button.OnClick  ()
                    ()
                ()
            | None -> ()

            ()  