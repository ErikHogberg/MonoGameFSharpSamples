module Asteroids

open Microsoft.Xna.Framework
open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open Microsoft.Xna.Framework.Graphics

open type System.MathF

type AsteroidExpiry (timeRemaining: float32) =
    let mutable timeRemaining = timeRemaining

    member this.TimeRemaining with get() = timeRemaining and set(value) = timeRemaining <- value

    


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

    let mutable expiryMapper: ComponentMapper<AsteroidExpiry> = null

    override this.Initialize(mapperService: IComponentMapperService) =
        expiryMapper <- mapperService.GetMapper<AsteroidExpiry>()
        ()

    override this.Process(gameTime: GameTime, enityId: int) =
        let mutable expiry = expiryMapper.Get(enityId)

        expiry.TimeRemaining <-
            expiry.TimeRemaining
            - gameTime.GetElapsedSeconds()

        if expiry.TimeRemaining <= 0f then
            this.DestroyEntity(enityId)

        ()

// type RainfallSystem(boundaries: Rectangle, minRate: float, maxRate: float, rateIsPerWidth: bool) =
type AsteroidShowerSystem(boundaries: EllipseF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Asteroid>))


    let random = new FastRandom()

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable asteroidMapper: ComponentMapper<Asteroid> = null
    let mutable expiryMapper: ComponentMapper<AsteroidExpiry> = null

    // asteroid shield/obstruction
    let mutable bubble = EllipseF(Vector2.One, 1f, 1f)

    // random range for asteroid spawn delay
    let MinSpawnDelay = 0.0f
    let MaxSpawnDelay = 0.1f
    let mutable spawnDelay = MaxSpawnDelay

    // asteroid shower render/despawn boundaries
    // let mutable boundaries = boundaries
    let boundaries = boundaries


    let mutable spawnAngle = 0f
    let mutable spawnSpeed = 100f

    // how far away an asteroid can be from the center of the boundary before despawning
    let cullDistance = 1000f

    // size of random spawn box
    let spawnOffsetRange = Vector2(MathHelper.Max(boundaries.RadiusX, boundaries.RadiusY), 50f)
    // height offset of spawn box from point on boundary to center of box
    let spawnHeightOffset = 100f


    // accessors

    member this.Bubble
        with set (value) = bubble <- value

    member this.SpawnAngle
        with get () = spawnAngle
        and set (value) = spawnAngle <- value
    member this.SpawnSpeed
        with set (value) = spawnSpeed <- value

    member this.SpawnVelocity
        with get () = Vector2.UnitY.Rotate(spawnAngle) * spawnSpeed

    member this.PointOnBoundary
        with get() = boundaries.Center
            + Vector2(
                boundaries.RadiusX * Cos(spawnAngle + MathHelper.PiOver2), 
                boundaries.RadiusY * Sin(spawnAngle + MathHelper.PiOver2) 
            );


    member this.RandomSpawnPos
        with get() = 
            Vector2(
                random.NextSingle(-spawnOffsetRange.X, spawnOffsetRange.X),
                random.NextSingle(-spawnOffsetRange.Y, spawnOffsetRange.Y) + spawnHeightOffset
            ).Rotate(spawnAngle)

    member this.SpawnRange () = 
        let center = Point2(0f, spawnHeightOffset) - spawnOffsetRange
        RectangleF(center , (spawnOffsetRange * 2f).ToSize())
            

    // member this.Boundaries
        // with set (value) = boundaries <- value

    // method for spawning new asteroid
    member this.CreateAsteroid(position: Vector2, velocity: Vector2, size: float32) =
        let entity = this.CreateEntity()
        entity.Attach(Transform2(position))
        entity.Attach(Asteroid(velocity, size))
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        asteroidMapper <- mapperService.GetMapper<Asteroid>()
        expiryMapper <- mapperService.GetMapper<AsteroidExpiry>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get(entityId)
            let asteroid = asteroidMapper.Get(entityId)

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
            let hasExpiry = expiryMapper.Has(entityId)

            // spawn 3 asteroids (with a set lifetime) upon either hitting the shield or leaving the boundary, also despawns old asteroid
            if ((asteroid.Entered && not inBoundary) || dropHitBox 
                || Vector2.DistanceSquared(transform.Position, boundaries.Center) > cullDistance*cullDistance 
                )
               && (not hasExpiry) then

                for i in 0 .. 3 do
                    // set a velocity for the new asteroid fragment to go randomly left or right, and back upwards
                    // TODO: rotate fragment velocity with impact direction
                    let impactAngle = System.MathF.Atan2(asteroid.Velocity.Y, asteroid.Velocity.X) - MathHelper.PiOver2
                    let velocity =
                        Vector2(
                            random.NextSingle(-100f, 100f),
                            -asteroid.Velocity.Length()
                            * random.NextSingle(0.1f, 0.2f)
                        ).Rotate(impactAngle)

                    // spawn the asteroid fragment, one pixel above the impact location
                    let id =
                        this.CreateAsteroid(
                            transform.Position.SetY(transform.Position.Y - 1f),
                            velocity,
                            (float32 i + 1f) * 0.5f
                        )

                    // add a time expiration of 1 second for the new asteroids, making them despawn by timer instead of collision
                    expiryMapper.Put(id, AsteroidExpiry(1f))

                // destroy the old asteroid
                this.DestroyEntity(entityId)

            ()

        spawnDelay <- spawnDelay - dt

        // TODO: make frame-indipendent
        if spawnDelay <= 0f then

            // calculate current spawn velocity and position using accessors
            let spawnVelocity = -this.SpawnVelocity
            let pointOnBoundary = this.PointOnBoundary

            // spawn 50 asteroid on each timer expiration
            for q in 0 .. 50 do
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

// Asteroids have their own draw system due to the hiding outside of boundary feature
type AsteroidRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect.All(typedefof<Transform2>, typedefof<Asteroid>))

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch(graphicsDevice)

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable asteroidMapper: ComponentMapper<Asteroid> = null


    let mutable alwaysShow = true
    member this.AlwaysShow
        with get () = alwaysShow
        and set (value) = alwaysShow <- value


    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        asteroidMapper <- mapperService.GetMapper<Asteroid>()
        ()

    override this.Draw(gameTime: GameTime) =
        // let dt = gameTime.GetElapsedSeconds()

        // set the sprite batch view to match the position of the camera
        let transformMatrix = camera.GetViewMatrix()
        spriteBatch.Begin(samplerState = SamplerState.PointClamp, transformMatrix = transformMatrix)

        for entity in this.ActiveEntities do
            let transform = transformMapper.Get(entity)
            let asteroid = asteroidMapper.Get(entity)

            // only draw asteroids if they have entered the boundary
            if alwaysShow || asteroid.Entered then
                spriteBatch.FillRectangle(transform.Position, Size2(asteroid.Size, asteroid.Size), Color.LightBlue)

            ()

        spriteBatch.End()
        ()
