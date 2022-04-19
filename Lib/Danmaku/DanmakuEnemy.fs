namespace Danmaku

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems

open Tools

type Enemy()=
    let mutable hp = 100f
    let mutable entered = false
    
    member this.HP with get () = hp and set (value) = hp <-value
    member this.Entered with get () = entered and set (value) = entered <-value
    
type EnemySystem (boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Enemy>))

    let boundaries = boundaries

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable bulletMapper: ComponentMapper<Enemy> = null


    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        bulletMapper <- mapperService.GetMapper<Enemy>()
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
                // mark enemy as having entered boundary
                if inBoundary then
                    bullet.Entered <- true

                // transform.Position <- transform.Position + bullet.Velocity * dt

                ()

            ()

        ()
    
    type EnemySpawner (time: float32, maxCount: uint) =

        
        let mutable timer = 0f

        member this.Time = time
        member this.MaxCount = maxCount
        member this.TimerExpired () = timer < 0f
        member this.DecrementTimer time = timer <- timer - time
        member this.IncrementTimer () = timer <- timer + time
        

    type EnemySpawnerSystem (boundaries: RectangleF) =
        inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<EnemySpawner>))

        let boundaries = boundaries

        // mappers for accessing components

        let mutable transformMapper: ComponentMapper<Transform2> = null
        let mutable enemySpawnerMapper: ComponentMapper<EnemySpawner> = null


        override this.Initialize(mapperService: IComponentMapperService) =
            transformMapper <- mapperService.GetMapper<Transform2>()
            enemySpawnerMapper <- mapperService.GetMapper<EnemySpawner>()

        override this.Update(gameTime: GameTime) =
            let dt = gameTime.GetElapsedSeconds()

            for entityId in this.ActiveEntities do
                let transform = transformMapper.Get entityId
                let enemySpawner = enemySpawnerMapper.Get entityId

                enemySpawner.DecrementTimer dt
                while enemySpawner.TimerExpired () do
                    enemySpawner.IncrementTimer ()
                    let newEnemy = this.CreateEntity ()
                    newEnemy.Attach (Enemy ())
                    newEnemy.Attach (Transform2 transform.Position)
                    // TODO: collision
                    // TODO: bullet spawner
                    ()



    