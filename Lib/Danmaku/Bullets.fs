module Bullets

open System.Linq

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Collisions

open Tools
open RenderSystem

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

type PlayerBullet(velocity: Vector2, size: float32, homing: bool) =
    inherit Bullet (velocity, size)

    member this.Homing = homing


// updating and spawning system
type PlayerBulletSystem (spawnerTransform: Transform2, boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<PlayerBullet>))


    let collisionTreeBounds = RectangleF (0f,0f, 1500f, 1500f)
    let mutable collisionTree = Quadtree collisionTreeBounds

    let random = new FastRandom()

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable bulletMapper: ComponentMapper<PlayerBullet> = null

    // homing target of bullets, pull force magnitude corresponds to radius
    let mutable target = CircleF(Point2(1f,1f), 1f)

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

    member this.CreateBullet (position: Vector2) velocity size homing =
        let entity = this.CreateEntity()
        let transform = Transform2(position)
        let bullet = PlayerBullet(velocity, size, homing)
        entity.Attach transform
        entity.Attach bullet
        entity.Attach (Dot(Size2(size,size), Color.Black))
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        bulletMapper <- mapperService.GetMapper<PlayerBullet>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // quadtree that will be used next update
        let nextCollisionTree = Quadtree collisionTreeBounds
        
        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get(entityId)
            let bullet = bulletMapper.Get(entityId)

            // check if asteroid is inside the render boundary
            let inBoundary = boundaries.Contains(transform.Position.ToPoint2())

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

                if bullet.Homing then                
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
                this.CreateBullet spawnerTransform.Position (Vector2.UnitY * -spawnVelocity) (random.NextSingle(2f, 4f)) true
            ()
        ()

    interface ICollidable with
        member this.CheckCollision other =
            (collisionTree.Query other).Any( fun boid -> boid.Bounds.Intersects other )

type EnemyBullet(velocity: Vector2, size: float32) =
    inherit Bullet (velocity, size)


// updating and spawning system
type EnemyBulletSystem (spawnerTransform: Transform2, boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<EnemyBullet>))


    let collisionTreeBounds = RectangleF (0f,0f, 1500f, 1500f)
    let mutable collisionTree = Quadtree collisionTreeBounds

    let random = new FastRandom()

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable bulletMapper: ComponentMapper<EnemyBullet> = null


    let firingDelay= 1f/10f
    let spawnVelocity = 10f

    let spawnerTransform = spawnerTransform

    let mutable firingTimer = 0f;


    member this.CreateBullet (position: Vector2) velocity size =
        let entity = this.CreateEntity()
        let transform = Transform2(position)
        let bullet = EnemyBullet(velocity, size)
        entity.Attach transform
        entity.Attach bullet
        entity.Attach (Dot(Size2(size,size), Color.Cyan))
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        bulletMapper <- mapperService.GetMapper<EnemyBullet>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // quadtree that will be used next update
        let nextCollisionTree = Quadtree collisionTreeBounds
        
        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get(entityId)
            let bullet = bulletMapper.Get(entityId)

            // check if asteroid is inside the render boundary
            let inBoundary = boundaries.Contains(transform.Position.ToPoint2())

            // mark asteroid as having entered boundary
            // if inBoundary then
            //     bullet.Entered <- true

            if bullet.Entered && (not inBoundary) then
                this.DestroyEntity(entityId)
            else
                if inBoundary then
                    bullet.Entered <- true

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

        // TODO: make enemies spawn enemy bullets instead of the enemy bullet updater

        // count down firing delay timer
        if firingTimer > 0f then
            firingTimer <- firingTimer - dt

        if firingTimer <= 0f then

            firingTimer <- firingDelay

            let id =
                this.CreateBullet spawnerTransform.Position (Vector2.UnitY * spawnVelocity) (random.NextSingle(2f, 4f))
            ()
        ()

    interface ICollidable with
        member this.CheckCollision other =
            (collisionTree.Query other).Any( fun boid -> boid.Bounds.Intersects other )

// rendering system
type BulletRenderSystem(graphicsDevice: GraphicsDevice, camera: OrthographicCamera) =
    inherit EntityDrawSystem(Aspect.All(typedefof<Transform2>, typedefof<PlayerBullet>))
    // inherit EntityDrawSystem(Aspect.All(typedefof<Transform2>).One(typedefof<PlayerBullet>,typedefof<EnemyBullet>))

    let graphicsDevice = graphicsDevice

    // reference to shared camera view, such as player camera
    let camera = camera

    // the boids have their own sprite batch
    let spriteBatch = new SpriteBatch(graphicsDevice)

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable bulletMapper: ComponentMapper<PlayerBullet> = null


    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        bulletMapper <- mapperService.GetMapper<PlayerBullet>()
        ()

    override this.Draw(gameTime: GameTime) =
        // let dt = gameTime.GetElapsedSeconds()

        // set the sprite batch view to match the position of the camera
        let transformMatrix = camera.GetViewMatrix()
        spriteBatch.Begin(samplerState = SamplerState.PointClamp, transformMatrix = transformMatrix)

        for entity in this.ActiveEntities do
            let transform = transformMapper.Get(entity)
            let bullet = bulletMapper.Get(entity)

            // only draw boids if they have entered the boundary
            spriteBatch.FillRectangle(transform.Position, Size2(bullet.Size, bullet.Size), Color.Black)

            ()

        spriteBatch.End()
        ()
