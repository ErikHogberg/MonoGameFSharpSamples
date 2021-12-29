module Rain

open Microsoft.Xna.Framework
open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open Microsoft.Xna.Framework.Graphics

type Expiry =
    val mutable TimeRemaining: float32

    new(timeRemaining: float32) = { TimeRemaining = timeRemaining }


type Raindrop(velocity: Vector2, size: float32, startY: float32) =

    let mutable velocity = velocity
    let mutable size = size
    let mutable startY = startY

    member this.Velocity
        with get () = velocity
        and set (value) = velocity <- value

    member this.Size
        with get () = size
        and set (value) = size <- value

    member this.StartY
        with get () = startY
        and set (value) = startY <- value

type RainExpirySystem() =
    inherit EntityProcessingSystem(Aspect.All(typedefof<Expiry>))

    [<DefaultValue>]
    val mutable expiryMapper: ComponentMapper<Expiry>

    override this.Initialize(mapperService: IComponentMapperService) =
        this.expiryMapper <- mapperService.GetMapper<Expiry>()
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
type RainfallSystem(boundaries: Rectangle) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Raindrop>))


    let random = new FastRandom()

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable raindropMapper: ComponentMapper<Raindrop>

    [<DefaultValue>]
    val mutable expiryMapper: ComponentMapper<Expiry>

    let mutable Box = new RectangleF()

    let MinSpawnDelay = 0.0f
    let MaxSpawnDelay = 0.1f
    let mutable spawnDelay = MaxSpawnDelay

    let boundaries = boundaries

    // TODO: make public
    let mutable windStrength = 0f

    member this.WindStrength
        with set (value) = windStrength <- value

    member this.CreateRaindrop(position: Vector2, velocity: Vector2, size: float32) =
        let entity = this.CreateEntity()
        entity.Attach(new Transform2(position))
        entity.Attach(new Raindrop(velocity, size, float32 boundaries.Top))
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.raindropMapper <- mapperService.GetMapper<Raindrop>()
        this.expiryMapper <- mapperService.GetMapper<Expiry>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = this.transformMapper.Get(entityId)
            let raindrop = this.raindropMapper.Get(entityId)

            raindrop.Velocity <- raindrop.Velocity + new Vector2(0f, 30f) * dt

            transform.Position <-
                transform.Position
                + (raindrop.Velocity + new Vector2(windStrength, 0f))
                  * dt

            let dropHitBox =
                Box.Contains(transform.Position.ToPoint())

            let mutable splashSpawnY = boundaries.Bottom - 1

            if dropHitBox then
                let boxTop = Box.Top
                splashSpawnY <- int boxTop - 1

            let hasExpiry = this.expiryMapper.Has(entityId)

            if ((transform.Position.Y >= float32 boundaries.Bottom
                 || dropHitBox)
                && (not hasExpiry)) then
                for i in 0 .. 3 do
                    let velocity =
                        new Vector2(
                            random.NextSingle(-100f, 100f),
                            -raindrop.Velocity.Y
                            * random.NextSingle(0.1f, 0.2f)
                        )
                    // var id = CreateRaindrop(transform.Position.SetY(splashSpawnY), velocity, (i + 1) * 0.5f);
                    let id =
                        this.CreateRaindrop(
                            transform.Position.SetY(transform.Position.Y - 1f),
                            velocity,
                            (float32 i + 1f) * 0.5f
                        )

                    this.expiryMapper.Put(id, new Expiry(1f))

                this.DestroyEntity(entityId)

            ()

        spawnDelay <- spawnDelay - dt

        if spawnDelay <= 0f then
            for q in 0 .. 50 do
                let position =
                    new Vector2(
                        random.NextSingle(float32 boundaries.Left, float32 boundaries.Right),
                        float32 boundaries.Top
                        + random.NextSingle(-240f, -480f)
                    )

                let id =
                    this.CreateRaindrop(position, Vector2.Zero, random.NextSingle(2f, 4f))

                ()

            spawnDelay <- random.NextSingle(MinSpawnDelay, MaxSpawnDelay)

        ()

type RainRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect.All(typedefof<Transform2>, typedefof<Raindrop>))

    let graphicsDevice = graphicsDevice
    let camera = camera
    let spriteBatch = new SpriteBatch(graphicsDevice)

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable raindropMapper: ComponentMapper<Raindrop>


    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.raindropMapper <- mapperService.GetMapper<Raindrop>()
        ()

    override this.Draw(gameTime: GameTime) =
        // let dt = gameTime.GetElapsedSeconds()
        let transformMatrix = camera.GetViewMatrix()
        spriteBatch.Begin(samplerState = SamplerState.PointClamp, transformMatrix = transformMatrix)

        for entity in this.ActiveEntities do
            let transform = this.transformMapper.Get(entity)
            let raindrop = this.raindropMapper.Get(entity)

            if transform.Position.Y > raindrop.StartY then
                spriteBatch.FillRectangle(transform.Position, new Size2(raindrop.Size, raindrop.Size), Color.LightBlue)

            ()

        spriteBatch.End()
        ()
