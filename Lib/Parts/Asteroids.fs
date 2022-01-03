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
    let mutable entered = false

    member this.Velocity
        with get () = velocity
        and set (value) = velocity <- value

    member this.Size
        with get () = size
        and set (value) = size <- value

    member this.Entered
        with get () = entered
        and set (value) = entered <-value


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
type AsteroidShowerSystem(boundaries: EllipseF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Asteroid>))


    let random = new FastRandom()

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable asteroidMapper: ComponentMapper<Asteroid>

    [<DefaultValue>]
    val mutable expiryMapper: ComponentMapper<AsteroidExpiry>

    let mutable bubble = new EllipseF(Vector2.One, 1f, 1f)

    let MinSpawnDelay = 0.0f
    let MaxSpawnDelay = 0.1f
    let mutable spawnDelay = MaxSpawnDelay

    let boundaries = boundaries

    let mutable windStrength = 0f

    member this.Bubble
        with set (value) = bubble <- value

    member this.WindStrength
        with set (value) = windStrength <- value

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


            transform.Position <-
                transform.Position
                + (asteroid.Velocity + new Vector2(windStrength, 0f))
                  * dt

            let dropHitBox = bubble.Contains(transform.Position)

            let inBoundary =
                boundaries.Contains(transform.Position)

            if inBoundary then asteroid.Entered <- true

            let hasExpiry = this.expiryMapper.Has(entityId)

            if ((asteroid.Entered && not inBoundary) || dropHitBox) && (not hasExpiry) then
                for i in 0 .. 3 do
                    let velocity =
                        new Vector2(
                            random.NextSingle(-100f, 100f),
                            -asteroid.Velocity.Y
                            * random.NextSingle(0.1f, 0.2f)
                        )

                    // var id = CreateRaindrop(transform.Position.SetY(splashSpawnY), velocity, (i + 1) * 0.5f);
                    let id =
                        this.CreateAsteroid(
                            transform.Position.SetY(transform.Position.Y - 1f),
                            velocity,
                            (float32 i + 1f) * 0.5f
                        )

                    this.expiryMapper.Put(id, new AsteroidExpiry(1f))

                this.DestroyEntity(entityId)

            ()

        spawnDelay <- spawnDelay - dt

        // TODO: make frame-indipendent
        if spawnDelay <= 0f then
            for q in 0 .. 50 do
                let position =
                    new Vector2(
                        random.NextSingle(float32 boundaries.Left, float32 boundaries.Right),
                        float32 boundaries.Top
                        + random.NextSingle(-240f, -480f)
                    )

                let id =
                    this.CreateAsteroid(position, Vector2.One * 100f, random.NextSingle(2f, 4f))

                ()

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
        let transformMatrix = camera.GetViewMatrix()
        spriteBatch.Begin(samplerState = SamplerState.PointClamp, transformMatrix = transformMatrix)

        for entity in this.ActiveEntities do
            let transform = this.transformMapper.Get(entity)
            let asteroid = this.asteroidMapper.Get(entity)

            // if transform.Position.Y > asteroid.StartY then
            if asteroid.Entered then
                spriteBatch.FillRectangle(transform.Position, new Size2(asteroid.Size, asteroid.Size), Color.LightBlue)

            ()

        spriteBatch.End()
        ()
