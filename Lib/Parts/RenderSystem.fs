module RenderSystem

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Sprites

open Tools

type SpriteRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect
        .All(typedefof<Sprite>, typedefof<Transform2>)
        )

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch(graphicsDevice)

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable spriteMapper: ComponentMapper<Sprite> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper()
        spriteMapper <- mapperService.GetMapper()
        ()

    override this.Draw(gameTime: GameTime) =
        let transformMatrix = camera.GetViewMatrix()

        spriteBatch.Begin (transformMatrix = transformMatrix)

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let sprite = spriteMapper.Get entityId
            spriteBatch.Draw (sprite, transform)

        spriteBatch.End ()
        ()


type EllipseRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect
        .All(typedefof<EllipseF>, typedefof<Transform2>)
        )

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch(graphicsDevice)

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable ellipseMapper: ComponentMapper<EllipseF> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper()
        ellipseMapper <- mapperService.GetMapper()
        ()

    override this.Draw(gameTime: GameTime) =
        let transformMatrix = camera.GetViewMatrix()

        spriteBatch.Begin(transformMatrix = transformMatrix)

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let ellipse = ellipseMapper.Get entityId
            // spriteBatch.Draw(sprite, transform)
            // TODO: apply transform
            spriteBatch.DrawEllipse (ellipse.Center, Vector2(ellipse.RadiusX, ellipse.RadiusY),16, Color.Gold)
            ()

        spriteBatch.End()
        ()

type Dot (color: Color) =
    let mutable color = color
    member this.Color with get () = color and set(value) = color <- value

type SizeComponent(size) =
    member this.Size: Size2 = size
    new (width, height) = SizeComponent(Size2(width,height))
    new (size) = SizeComponent(Size2(size,size))

// type ColorComponent(color) =
//     member this.Color: Color = color

type DotRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect
        .All(typedefof<Transform2>, typedefof<Dot>, typedefof<SizeComponent> )
        )

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch(graphicsDevice)

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable dotMapper: ComponentMapper<Dot> = null
    let mutable sizeMapper: ComponentMapper<SizeComponent> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper()
        dotMapper <- mapperService.GetMapper()
        sizeMapper <- mapperService.GetMapper()
        ()

    override this.Draw(gameTime: GameTime) =
        let transformMatrix = camera.GetViewMatrix()

        spriteBatch.Begin(transformMatrix = transformMatrix)

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let dot = dotMapper.Get entityId
            let size = (sizeMapper.Get entityId).Size
            
            spriteBatch.FillRectangle (transform.Position - size.ToVector(), size * 2f, dot.Color)
            ()

        spriteBatch.End ()
        ()
