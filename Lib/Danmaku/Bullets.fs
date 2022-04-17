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

// open type System.MathF


type Bullet(velocity: Vector2, size: float32, collisionLayers: list<string>, homing: bool) =

    let mutable velocity = velocity
    let mutable size = size

    let collisionLayers = collisionLayers

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
    
    member this.Homing = homing


// updating and spawning system
type BulletSystem (boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Bullet>))

    let boundaries = boundaries

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable bulletMapper: ComponentMapper<Bullet> = null


    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        bulletMapper <- mapperService.GetMapper<Bullet>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get(entityId)
            let bullet = bulletMapper.Get(entityId)

            // check if bullet is inside the render boundary
            let inBoundary = boundaries.Contains(transform.Position.ToPoint2())

            if bullet.Entered && (not inBoundary) then
                // despawn bullet if it left the boundary
                this.DestroyEntity(entityId)
            else
                // mark bullet as having entered boundary
                if inBoundary then
                    bullet.Entered <- true

                // pull velocity towards gravity target
                // boid.Velocity <- boid.Velocity + Vector2.Normalize(target.Position - transform.Position) * target.Radius * targetSteerMul

                // if bullet.Homing then                
                //     bullet.Velocity <- bullet.Velocity.FasterRotateTowards(target.Center.ToVector() - transform.Position) (target.Radius*targetSteerMul)

                // move bullet
                transform.Position <- transform.Position + bullet.Velocity * dt

                ()

            ()

        ()

    // interface ICollidable with
    //     member this.CheckCollision other =
    //         (collisionTree.Query other).Any( fun boid -> boid.Bounds.Intersects other )

type BulletSpawner (rate: float32, spawnVelocity: Vector2) = 
    let rate = rate/100f

    let mutable firing = false
    let mutable firingTimer = -1.0f

    let spawnVelocity = spawnVelocity

    member this.SpawnVelocity with get () = spawnVelocity
    member this.FiringRate with get () = rate

    member this.Firing 
        with set(value) = firing <- value
        and get () = firing

    member this.FiringTimer 
        with set(value) = firingTimer <- value
        and get () = firingTimer


type BulletSpawnerSystem () =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<BulletSpawner>))

    let random = new FastRandom()

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable bulletMapper: ComponentMapper<BulletSpawner> = null

    member this.CreateBullet (position: Vector2) velocity size layers homing =
        let entity = this.CreateEntity()
        let transform = Transform2(position)
        let bullet = Bullet(velocity, size, layers, homing)
        entity.Attach transform
        entity.Attach bullet
        entity.Attach (Dot(Size2(size,size), Color.Black))
        // entity.Attach (Collision.Collidable(5f, ["enemy"], fun () -> false))
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        bulletMapper <- mapperService.GetMapper<BulletSpawner>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()


        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get(entityId)
            let bulletSpawner = bulletMapper.Get(entityId)

            bulletSpawner.FiringTimer <- bulletSpawner.FiringTimer - dt
            if bulletSpawner.Firing then
                if bulletSpawner.FiringTimer <= 0f then
                    bulletSpawner.FiringTimer <- bulletSpawner.FiringTimer + bulletSpawner.FiringRate
                    let size = 2f + random.NextSingle(4f)
                    let _ = this.CreateBullet transform.Position bulletSpawner.SpawnVelocity size ["enemy"] false
                    ()
                ()
            else
                bulletSpawner.FiringTimer <- 0f
            
        ()
