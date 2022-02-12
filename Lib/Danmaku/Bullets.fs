module Bullets

open System.Linq

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Collisions

open Tools

open type System.MathF


type Bullet(velocity: Vector2, size: float32) =

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
        and set (value) = entered <- value


// updating and spawning system
type BulletSystem (spawnerTransform: Transform2, boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Bullet>))


    let collisionTreeBounds = RectangleF (0f,0f, 1500f, 1500f)
    let mutable collisionTree = Quadtree collisionTreeBounds

    let random = new FastRandom()

    // mappers for accessing components

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable bulletMapper: ComponentMapper<Bullet>

    // homing target of bullets, pull force magnitude corresponds to radius
    let mutable target = CircleF(Vector2.One, 1f)

    let firingDelay= 1f/10f
    let spawnVelocity = 10f
    let mutable firing = false

    let spawnerTransform = spawnerTransform

    let mutable firingTimer = 0f;

    // multiplier on gravity target radius
    let targetSteerMul = 0.2f

    // accessors

    member this.Target
        with set (value) = target <- value
        and get () = target
    member this.Firing 
        with set (value) = firing <- value


    //

    member this.CreateBullet (position: Vector2) velocity size =
        let entity = this.CreateEntity()
        let transform = Transform2(position)
        let bullet = Bullet(velocity, size)
        entity.Attach transform
        entity.Attach bullet
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.bulletMapper <- mapperService.GetMapper<Bullet>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // quadtree that will be used next update
        let nextCollisionTree = Quadtree collisionTreeBounds
        
        for entityId in this.ActiveEntities do
            let transform = this.transformMapper.Get(entityId)
            let bullet = this.bulletMapper.Get(entityId)

            // check if asteroid is inside the render boundary
            let inBoundary = boundaries.Contains(transform.Position)

            // mark asteroid as having entered boundary
            // if inBoundary then
            //     bullet.Entered <- true

            if bullet.Entered && (not inBoundary) then
                this.DestroyEntity(entityId)
            else
                if inBoundary then
                    bullet.Entered <- true

                // pull velocity towards gravity target
                // boid.Velocity <- boid.Velocity + Vector2.Normalize(target.Position - transform.Position) * target.Radius * targetSteerMul
                
                bullet.Velocity <- bullet.Velocity.FasterRotateTowards(target.Center.ToVector() - transform.Position) (target.Radius*targetSteerMul)

                // move bullet
                transform.Position <- transform.Position + bullet.Velocity * spawnVelocity * dt

                nextCollisionTree.Insert (QuadtreeData(Tools.TransformCollisionActor(transform, bullet.Size, (fun _ -> ()), bullet)))
                ()

            ()

        // removes unneccesary leaf nodes and simplifies the new quad tree
        nextCollisionTree.Shake()
        // replace the old quadtree with the new one in preparation for the next update
        collisionTree <- nextCollisionTree


        // spawning new entities

        // count down firing delay timer
        if firingTimer > 0f then
            firingTimer <- firingTimer - dt

        if firing && firingTimer <= 0f then

            firingTimer <- firingDelay

            let id =
                this.CreateBullet spawnerTransform.Position (Vector2.UnitY * -spawnVelocity) (random.NextSingle(2f, 4f))
            ()
        ()

    interface ICollidable with
        member this.CheckCollision other =
            (collisionTree.Query other).Any( fun boid -> boid.Bounds.Intersects other )

// rendering system
type BulletRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect.All(typedefof<Transform2>, typedefof<Bullet>))

    let graphicsDevice = graphicsDevice

    // reference to shared camera view, such as player camera
    let camera = camera

    // the boids have their own sprite batch
    let spriteBatch = new SpriteBatch(graphicsDevice)

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>

    [<DefaultValue>]
    val mutable bulletMapper: ComponentMapper<Bullet>


    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.bulletMapper <- mapperService.GetMapper<Bullet>()
        ()

    override this.Draw(gameTime: GameTime) =
        // let dt = gameTime.GetElapsedSeconds()

        // set the sprite batch view to match the position of the camera
        let transformMatrix = camera.GetViewMatrix()
        spriteBatch.Begin(samplerState = SamplerState.PointClamp, transformMatrix = transformMatrix)

        for entity in this.ActiveEntities do
            let transform = this.transformMapper.Get(entity)
            let boid = this.bulletMapper.Get(entity)

            // only draw boids if they have entered the boundary
            spriteBatch.FillRectangle(transform.Position, Size2(boid.Size, boid.Size), Color.Black)

            ()

        spriteBatch.End()
        ()