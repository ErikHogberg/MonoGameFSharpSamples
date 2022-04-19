module TransformUpdater

open Microsoft.Xna.Framework
open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems

open Tools

type ConstrainedTransform() =
    let mutable velocity = Vector2.Zero
    let mutable speed = 1f
    member this.Velocity with get () = velocity and set(value) = velocity <- value
    member this.Speed with get () = speed and set(value) = speed <- value

type ConstrainedTransformSystem (boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<ConstrainedTransform>))

    let boundaries = boundaries

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable moverMapper: ComponentMapper<ConstrainedTransform> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        moverMapper <- mapperService.GetMapper<ConstrainedTransform>()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get(entityId)
            let mover = moverMapper.Get(entityId)

            let inBoundary = boundaries.Contains(transform.Position.ToPoint2())

            transform.Position <- transform.Position + mover.Velocity * mover.Speed * dt
            if not inBoundary then
                transform.Position <- boundaries.ClosestPointTo(transform.Position.ToPoint2()).ToVector()
