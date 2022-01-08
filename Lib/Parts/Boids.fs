module Boids

open Microsoft.Xna.Framework
open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open Microsoft.Xna.Framework.Graphics

open type System.MathF


type Boid(velocity: Vector2, size: float32, timeRemaining: float32) =

    let mutable timeRemaining = timeRemaining

    let mutable velocity = velocity
    let mutable size = size

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

    // mappers for accessing components

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable boidMapper: ComponentMapper<Boid>

    // gravity target of boids, force magnitude corresponds to radius
    let mutable target = CircleF(Vector2.One, 1f)

    // random range for asteroid spawn delay
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


    member this.CreateAsteroid(position: Vector2, velocity: Vector2, size: float32) =
        let entity = this.CreateEntity()
        entity.Attach(Transform2(position))
        entity.Attach(Boid(velocity, size, random.NextSingle(2f, 15f)))
        spawnCount <- spawnCount + 1
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.boidMapper <- mapperService.GetMapper<Boid>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = this.transformMapper.Get(entityId)
            let asteroid = this.boidMapper.Get(entityId)

            asteroid.Velocity <- asteroid.Velocity + Vector2.Normalize(target.Position - transform.Position) * target.Radius

            if asteroid.Velocity.LengthSquared() > velocityCap ** 2f then
                asteroid.Velocity <- asteroid.Velocity.NormalizedCopy() * velocityCap

            // move asteroid
            transform.Position <- transform.Position + asteroid.Velocity * dt

            asteroid.TimeRemaining <-
                asteroid.TimeRemaining
                - gameTime.GetElapsedSeconds()

            if asteroid.TimeRemaining <= 0f then
                this.DestroyEntity(entityId)
                spawnCount <- spawnCount - 1

            ()

        spawnDelay <- spawnDelay - dt

        // TODO: make frame-indipendent
        if spawnDelay <= 0f then

            // calculate current spawn velocity and position using accessors
            let spawnVelocity = this.SpawnVelocity(random.NextSingle(MathHelper.Tau), random.NextSingle(-50f, 10f) + spawnSpeed)
            let pointOnBoundary = this.PointOnBoundary

            // spawn 50 asteroid on each timer expiration
            for q in 0 .. System.Math.Min(50, maxBoids - spawnCount)  do
                // position is randomized along width of boundary

                let position =
                    // randomize displacement of each asteroid along width of boundary
                    // IDEA: adjust spawn rate depending on current spawn random width compared to min and max width
                    this.RandomSpawnPos
                    + pointOnBoundary

                let id =
                    this.CreateAsteroid(position, spawnVelocity, random.NextSingle(2f, 4f))

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
    val mutable asteroidMapper: ComponentMapper<Boid>


    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.asteroidMapper <- mapperService.GetMapper<Boid>()
        ()

    override this.Draw(gameTime: GameTime) =
        // let dt = gameTime.GetElapsedSeconds()

        // set the sprite batch view to match the position of the camera
        let transformMatrix = camera.GetViewMatrix()
        spriteBatch.Begin(samplerState = SamplerState.PointClamp, transformMatrix = transformMatrix)

        for entity in this.ActiveEntities do
            let transform = this.transformMapper.Get(entity)
            let asteroid = this.asteroidMapper.Get(entity)

            // only draw asteroids if they have entered the boundary
            spriteBatch.FillRectangle(transform.Position, Size2(asteroid.Size, asteroid.Size), Color.Orange)

            ()

        spriteBatch.End()
        ()
