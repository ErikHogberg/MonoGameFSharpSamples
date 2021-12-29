module RenderSystem

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Sprites


type SpriteRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera)=
    inherit EntityDrawSystem(Aspect.All(typedefof<Sprite>, typedefof<Transform2>))

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch (graphicsDevice)

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>
    [<DefaultValue>]
    val mutable spriteMapper: ComponentMapper<Sprite>

    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.spriteMapper<- mapperService.GetMapper<Sprite>()
        ()

    override this.Draw(gameTime: GameTime) =
        let transformMatrix = camera.GetViewMatrix();

        spriteBatch.Begin(transformMatrix = transformMatrix)

        for entityId in this.ActiveEntities do
            let transform = this.transformMapper.Get entityId
            let sprite = this.spriteMapper.Get entityId
            spriteBatch.Draw(sprite, transform)
        
        spriteBatch.End()
        ()