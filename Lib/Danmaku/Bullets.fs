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


type Bullet(velocity: Vector2, target: Option<Transform2>) =

    let mutable velocity = velocity
    let mutable entered = false

    member this.Target = target

    member this.Velocity
        with get () = velocity
        and set (value) = velocity <- value

    member this.Entered
        with get () = entered
        and set (value) = entered <- value
    

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
                // TODO: check if despawning entities causes exception here
                match bullet.Target with
                    | Some target ->
                        bullet.Velocity <- bullet.Velocity + Vector2.Normalize(target.Position - transform.Position) * 5f
                    | None ->
                        ()

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

type BulletSpawner (rate: float32, spawnVelocity: Vector2, tag: string, onCollision: Collision.TransformCollisionActor -> bool) = 
    let rate = 1f/rate

    let mutable firing = false
    let mutable firingTimer = -1.0f

    let spawnVelocity = spawnVelocity

    let tag = tag
    let onCollision = onCollision

    member this.SpawnVelocity with get () = spawnVelocity
    member this.FiringRate with get () = rate
    member this.OnCollision = onCollision
    member this.Tag = tag

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

    member this.CreateBullet (position: Vector2) velocity size (bulletSpawner: BulletSpawner) =
        let entity = this.CreateEntity()
        let transform = Transform2(position)
        let bullet = Bullet(velocity, None)
        entity.Attach transform
        entity.Attach bullet
        entity.Attach (Dot(Size2(size,size), Color.Black))
        entity.Attach (Collision.TransformCollisionActor(transform, 5f, bulletSpawner.Tag, onCollision = bulletSpawner.OnCollision))
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
                    let _ = this.CreateBullet transform.Position bulletSpawner.SpawnVelocity size bulletSpawner
                    ()
            else
                bulletSpawner.FiringTimer <- 0f
            
        ()
