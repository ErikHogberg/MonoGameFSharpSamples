module TransformUpdater

open Microsoft.Xna.Framework
open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems

open Tools
open MonoGame.Extended.Tweening

type ConstrainedTransform() =
    let mutable velocity = Vector2.Zero
    let mutable speed = 1f
    member this.Velocity with get () = velocity and set (value) = velocity <- value
    member this.Speed with get () = speed and set (value) = speed <- value

type ConstrainedTransformSystem (boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<ConstrainedTransform>))

    let boundaries = boundaries

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable moverMapper: ComponentMapper<ConstrainedTransform> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper ()
        moverMapper <- mapperService.GetMapper ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds ()

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let mover = moverMapper.Get entityId

            let inBoundary = boundaries.Contains <| transform.Position.ToPoint2

            transform.Position <- transform.Position + mover.Velocity * mover.Speed * dt
            if not inBoundary then
                transform.Position <- (boundaries.ClosestPointTo <| transform.Position.ToPoint2).ToVector

type TransformFollower(target: Transform2, offset: Vector2) =

    // let mutable velocity = Vector2.Zero
    let mutable speed = 300f


    // let target = target

    member this.offset with get () = offset
    // member this.Velocity with get () = velocity and set(value) = velocity <- value
    member this.Speed with get () = speed and set(value) = speed <- value
    member this.Target with get () = target

type TransformFollowerSystem () =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<TransformFollower>))

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable moverMapper: ComponentMapper<TransformFollower> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        moverMapper <- mapperService.GetMapper<TransformFollower>()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get(entityId)
            let mover = moverMapper.Get(entityId)

            // IDEA: accelerate towards target instead

            transform.Position <- transform.Position.MoveTowards(mover.Target.Position + mover.offset, mover.Speed * dt)


type TweenTransformer(target, toTarget, duration, repeatDelay, moveFn, easingFn: float32 -> float32 ) =

    let tweener = new Tweener()

    let _ = 
        tweener.TweenTo(
            target, 
            moveFn,
            toTarget, 
            duration, 
            0f
            )
                .RepeatForever(repeatDelay) 
                .AutoReverse()
                .Easing(easingFn)

    member this.Tweener = tweener
    new (transform: Transform2, target: Vector2, duration, repeatDelay, easingFn: float32 -> float32 ) =
        TweenTransformer(transform, target, duration, repeatDelay, (fun player -> transform.Position), easingFn)
    // new (transform: Transform2, target: Vector2, duration, repeatDelay, easingFn: float32 -> float32 ) =
        // TweenTransformer(transform, target, duration, repeatDelay, (fun player -> transform.Position), easingFn)


type TweenTransformerSystem () =
    inherit EntityUpdateSystem(Aspect.All(typedefof<TweenTransformer>))

    let mutable moverMapper: ComponentMapper<TweenTransformer> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        moverMapper <- mapperService.GetMapper<TweenTransformer>()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let mover = moverMapper.Get(entityId)
            mover.Tweener.Update dt
            ()  

