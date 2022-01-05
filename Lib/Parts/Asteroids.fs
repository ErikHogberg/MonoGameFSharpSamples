module Asteroids

open Microsoft.Xna.Framework
open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open Microsoft.Xna.Framework.Graphics

type AsteroidExpiry =
    val mutable TimeRemaining: float32

    new(timeRemaining: float32) = { TimeRemaining = timeRemaining }


type Asteroid(velocity: Vector2, size: float32) =

    let mutable velocity = velocity
    let mutable size = size

    // whether or not the asteroid has entered the rendering boundary of the asteroid shower system yet
    // used for triggering the impact effect (and despawing) upon exiting the rendering boundary
    let mutable entered = false

    member this.Velocity
        with get () = velocity
        and set (value) = velocity <- value

    member this.Size
        with get () = size
        and set (value) = size <- value

    member this.Entered
        with get () = entered
        and set (value) = entered <- value


type AsteroidExpirySystem() =
    inherit EntityProcessingSystem(Aspect.All(typedefof<AsteroidExpiry>))

    [<DefaultValue>]
    val mutable expiryMapper: ComponentMapper<AsteroidExpiry>

    override this.Initialize(mapperService: IComponentMapperService) =
        this.expiryMapper <- mapperService.GetMapper<AsteroidExpiry>()
        ()

    override this.Process(gameTime: GameTime, enityId: int) =
        let mutable expiry = this.expiryMapper.Get(enityId)

        expiry.TimeRemaining <-
            expiry.TimeRemaining
            - gameTime.GetElapsedSeconds()

        if expiry.TimeRemaining <= 0f then
            this.DestroyEntity(enityId)

        ()

// type RainfallSystem(boundaries: Rectangle, minRate: float, maxRate: float, rateIsPerWidth: bool) =
type AsteroidShowerSystem(boundaries: EllipseF, startVelocity: Vector2) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Asteroid>))


    let random = new FastRandom()

    // mappers for accessing components

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable asteroidMapper: ComponentMapper<Asteroid>

    [<DefaultValue>]
    val mutable expiryMapper: ComponentMapper<AsteroidExpiry>

    // asteroid shield/obstruction
    let mutable bubble = new EllipseF(Vector2.One, 1f, 1f)

    // random range for asteroid spawn delay
    let MinSpawnDelay = 0.0f
    let MaxSpawnDelay = 0.1f
    let mutable spawnDelay = MaxSpawnDelay

    // asteroid shower render/despawn boundaries
    // let mutable boundaries = boundaries
    let mutable boundaries = boundaries

    let mutable startVelocity = startVelocity


    // accessors

    member this.Bubble
        with set (value) = bubble <- value

    member this.StartVelocity
        with set (value) = startVelocity <- value

    // member this.Boundaries
        // wiht set (value) = boundaries <- value

    // method for spawning new asteroid
    member this.CreateAsteroid(position: Vector2, velocity: Vector2, size: float32) =
        let entity = this.CreateEntity()
        entity.Attach(new Transform2(position))
        entity.Attach(new Asteroid(velocity, size))
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.asteroidMapper <- mapperService.GetMapper<Asteroid>()
        this.expiryMapper <- mapperService.GetMapper<AsteroidExpiry>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = this.transformMapper.Get(entityId)
            let asteroid = this.asteroidMapper.Get(entityId)

            // move asteroid
            transform.Position <- transform.Position + asteroid.Velocity * dt

            // check if asteroid hit the shield
            let dropHitBox = bubble.Contains(transform.Position)

            // check if asteroid is inside the render boundary
            let inBoundary = boundaries.Contains(transform.Position)

            // mark asteroid as having entered boundary
            if inBoundary then
                asteroid.Entered <- true

            // check if the asteroid is an impact effect asteroid fragment as opposed to a normal asteroid
            let hasExpiry = this.expiryMapper.Has(entityId)

            // spawn 3 asteroids (with a set lifetime) upon either hitting the shield or leaving the boundary, also despawns old asteroid
            if ((asteroid.Entered && not inBoundary) || dropHitBox)
               && (not hasExpiry) then

                for i in 0 .. 3 do
                    // set a velocity for the new asteroid fragment to go randomly left or right, and back upwards
                    let velocity =
                        new Vector2(
                            random.NextSingle(-100f, 100f),
                            -asteroid.Velocity.Y
                            * random.NextSingle(0.1f, 0.2f)
                        )

                    // spawn the asteroid fragment, one pixel above the impact location
                    let id =
                        this.CreateAsteroid(
                            transform.Position.SetY(transform.Position.Y - 1f),
                            velocity,
                            (float32 i + 1f) * 0.5f
                        )

                    // add a time expiration of 1 second for the new asteroids, making them despawn by timer instead of collision
                    this.expiryMapper.Put(id, new AsteroidExpiry(1f))

                // destroy the old asteroid
                this.DestroyEntity(entityId)

            ()

        spawnDelay <- spawnDelay - dt

        // TODO: make frame-indipendent
        if spawnDelay <= 0f then
            // spawn 50 asteroid on each timer expiration
            for q in 0 .. 50 do
                // position is randomized along width of boundary
                let position =
                    new Vector2(
                        random.NextSingle(float32 boundaries.Left, float32 boundaries.Right),
                        float32 boundaries.Top
                        + random.NextSingle(-240f, -480f)
                    )

                let id =
                    this.CreateAsteroid(position, startVelocity, random.NextSingle(2f, 4f))

                ()

            // spawn delay is randomized on each expiration
            spawnDelay <- random.NextSingle(MinSpawnDelay, MaxSpawnDelay)

        ()

type AsteroidRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect.All(typedefof<Transform2>, typedefof<Asteroid>))

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch(graphicsDevice)

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable asteroidMapper: ComponentMapper<Asteroid>


    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.asteroidMapper <- mapperService.GetMapper<Asteroid>()
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
            if asteroid.Entered then
                spriteBatch.FillRectangle(transform.Position, new Size2(asteroid.Size, asteroid.Size), Color.LightBlue)

            ()

        spriteBatch.End()
        ()
