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
        transformMapper <- mapperService.GetMapper<Transform2>()
        spriteMapper <- mapperService.GetMapper<Sprite>()
        ()

    override this.Draw(gameTime: GameTime) =
        let transformMatrix = camera.GetViewMatrix()

        spriteBatch.Begin(transformMatrix = transformMatrix)

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let sprite = spriteMapper.Get entityId
            spriteBatch.Draw(sprite, transform)

        spriteBatch.End()
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
        transformMapper <- mapperService.GetMapper<Transform2>()
        ellipseMapper <- mapperService.GetMapper<EllipseF>()
        ()

    override this.Draw(gameTime: GameTime) =
        let transformMatrix = camera.GetViewMatrix()

        spriteBatch.Begin(transformMatrix = transformMatrix)

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let sprite = ellipseMapper.Get entityId
            // spriteBatch.Draw(sprite, transform)
            // TODO: apply transform
            spriteBatch.DrawEllipse(sprite.Center, Vector2( sprite.RadiusX, sprite.RadiusY),16, Color.Gold)
            ()

        spriteBatch.End()
        ()

type Dot (size: Size2, color: Color) =

    new (size:float32, color:Color) =
        Dot (Size2(size,size), color)

    member this.Size = size
    member this.Color = color

type DotRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect
        .All(typedefof<Transform2>, typedefof<Dot>)
        )

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch(graphicsDevice)

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable dotMapper: ComponentMapper<Dot> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        dotMapper <- mapperService.GetMapper<Dot>()
        ()

    override this.Draw(gameTime: GameTime) =
        let transformMatrix = camera.GetViewMatrix()

        spriteBatch.Begin(transformMatrix = transformMatrix)

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let dot = dotMapper.Get entityId
            spriteBatch.FillRectangle(transform.Position - dot.Size.ToVector(), dot.Size * 2f, dot.Color)
            ()

        spriteBatch.End()
        ()
