module UpdateSystem

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Sprites


type TransformUpdateSystem () =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>).Exclude(typedefof<Asteroids.Asteroid>, typedefof<Boids.Boid>))

    let mutable transformMapper: ComponentMapper<Transform2> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            transform.Rotation <- transform.Rotation + dt * 10f
            
        ()