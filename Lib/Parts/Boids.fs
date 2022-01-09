module Boids

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Collisions

open type System.MathF


type Boid(velocity: Vector2, size: float32, timeRemaining: float32) =

    let mutable timeRemaining = timeRemaining

    let mutable velocity = velocity
    let mutable size = size

    let mutable nearby = List.Empty

    member this.Nearby with get()= nearby and set(value ) = nearby <- value

    member this.Velocity
        with get () = velocity
        and set (value) = velocity <- value

    member this.Size
        with get () = size
        and set (value) = size <- value

    member this.TimeRemaining
        with get () = timeRemaining
        and set (value) = timeRemaining <- value



// type RainfallSystem(boundaries: Rectangle, minRate: float, maxRate: float, rateIsPerWidth: bool) =
type BoidsSystem(boundaries: EllipseF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Boid>))

    // IDEA: use tweening lib to define gravity target pull force drop off over distance

    let random = new FastRandom()

    let collisionComponent = new CollisionComponent(new RectangleF(0f,0f, 500f, 500f))



    // mappers for accessing components

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable boidMapper: ComponentMapper<Boid>

    // gravity target of boids, force magnitude corresponds to radius
    let mutable target = CircleF(Vector2.One, 1f)

    // random range for boid spawn delay
    let MinSpawnDelay = 0.0f
    let MaxSpawnDelay = 0.1f
    let mutable spawnDelay = MaxSpawnDelay

    let velocityCap = 100f

    // let mutable boundaries = boundaries
    let mutable boundaries = boundaries

    let mutable spawnAngle = 0f
    let mutable spawnSpeed = 100f

    // size of random spawn box
    let spawnOffsetRange = Vector2(MathHelper.Max(boundaries.RadiusX, boundaries.RadiusY), 50f)

    let mutable spawnCount = 0
    let maxBoids = 10000


    // accessors

    member this.Target
        with set (value) = target <- value

    member this.SpawnAngle
        with get () = spawnAngle
        and set (value) = spawnAngle <- value
    member this.SpawnSpeed
        with set (value) = spawnSpeed <- value


    member this.SpawnVelocity(angle: float32, speed: float32) = Vector2.UnitY.Rotate(angle) * speed
    member this.SpawnVelocity() = this.SpawnVelocity(spawnAngle, spawnSpeed)

    member this.PointOnBoundary
        with get() = boundaries.Center
            + Vector2(
                boundaries.RadiusX * Cos(spawnAngle + PI*0.5f), 
                boundaries.RadiusY * Sin(spawnAngle + PI*0.5f) );


    member this.RandomSpawnPos
        with get() = Vector2(
                random.NextSingle(-spawnOffsetRange.X, spawnOffsetRange.X),
                random.NextSingle(-spawnOffsetRange.Y, spawnOffsetRange.Y)
            ).Rotate(spawnAngle)

    member this.SpawnRange () = 
        RectangleF(spawnOffsetRange, (spawnOffsetRange * 2f).ToSize())


    member this.CreateBoid(position: Vector2, velocity: Vector2, size: float32) =
        let entity = this.CreateEntity()
        let transform = Transform2(position)
        let boid = Boid(velocity, size, random.NextSingle(2f, 15f))
        entity.Attach(transform)
        entity.Attach(boid)
        let onCollision (args: CollisionEventArgs) = 
            let otherBoid = (args.Other :?> Tools.TransformCollisionActor).Data
            boid.Nearby <- otherBoid :: boid.Nearby 
            ()
        collisionComponent.Insert(Tools.TransformCollisionActor(transform, 100f, onCollision, boid))
        spawnCount <- spawnCount + 1
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.boidMapper <- mapperService.GetMapper<Boid>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        collisionComponent.Update gameTime

        for entityId in this.ActiveEntities do
            let transform = this.transformMapper.Get(entityId)
            let boid = this.boidMapper.Get(entityId)

            for otherBoid in boid.Nearby do
                // TODO: separation
                // TODO: alignment
                // TODO: cohesion
                ()

            boid.Velocity <- boid.Velocity + Vector2.Normalize(target.Position - transform.Position) * target.Radius

            if boid.Velocity.LengthSquared() > velocityCap ** 2f then
                boid.Velocity <- boid.Velocity.NormalizedCopy() * velocityCap

            // move boid
            transform.Position <- transform.Position + boid.Velocity * dt

            boid.TimeRemaining <-
                boid.TimeRemaining
                - gameTime.GetElapsedSeconds()

            if boid.TimeRemaining <= 0f then
                this.DestroyEntity(entityId)
                spawnCount <- spawnCount - 1

            boid.Nearby <- []

            ()

        spawnDelay <- spawnDelay - dt

        // TODO: make frame-indipendent
        if spawnDelay <= 0f then

            // calculate current spawn velocity and position using accessors
            let spawnVelocity = this.SpawnVelocity(random.NextSingle(MathHelper.Tau), random.NextSingle(-50f, 10f) + spawnSpeed)
            let pointOnBoundary = this.PointOnBoundary

            // spawn 50 boids on each timer expiration
            for q in 0 .. System.Math.Min(50, maxBoids - spawnCount)  do
                // position is randomized along width of boundary

                let position =
                    // randomize displacement of each boid along width of boundary
                    // IDEA: adjust spawn rate depending on current spawn random width compared to min and max width
                    this.RandomSpawnPos
                    + pointOnBoundary

                let id =
                    this.CreateBoid(position, spawnVelocity, random.NextSingle(2f, 4f))

                ()

            // spawn delay is randomized on each expiration
            spawnDelay <- random.NextSingle(MinSpawnDelay, MaxSpawnDelay)

        ()

type BoidsRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect.All(typedefof<Transform2>, typedefof<Boid>))

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch(graphicsDevice)

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable boidMapper: ComponentMapper<Boid>


    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.boidMapper <- mapperService.GetMapper<Boid>()
        ()

    override this.Draw(gameTime: GameTime) =
        // let dt = gameTime.GetElapsedSeconds()

        // set the sprite batch view to match the position of the camera
        let transformMatrix = camera.GetViewMatrix()
        spriteBatch.Begin(samplerState = SamplerState.PointClamp, transformMatrix = transformMatrix)

        for entity in this.ActiveEntities do
            let transform = this.transformMapper.Get(entity)
            let boid = this.boidMapper.Get(entity)

            // only draw boids if they have entered the boundary
            spriteBatch.FillRectangle(transform.Position, Size2(boid.Size, boid.Size), Color.Orange)

            ()

        spriteBatch.End()
        ()
