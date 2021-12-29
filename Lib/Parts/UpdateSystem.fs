module UpdateSystem

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Sprites


type TransformUpdateSystem()=
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>))


    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        ()

    override this.Update(gameTime: GameTime) =

        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let mutable transform = this.transformMapper.Get entityId
            transform.Rotation <- transform.Rotation + dt * 10f
        
        ()