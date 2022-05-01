namespace Danmaku

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems

open Tools
open RenderSystem
open TransformUpdater
open MonoGame.Extended.Tweening

type Enemy()=
    let mutable hp = 100f
    let mutable entered = false
    
    member this.HP with get () = hp and set (value) = hp <- value
    member this.Entered with get () = entered and set (value) = entered <- value
    
type EnemySystem (boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Enemy>))

    let boundaries = boundaries

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable bulletMapper: ComponentMapper<Enemy> = null


    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper()
        bulletMapper <- mapperService.GetMapper()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds ()

        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let bullet = bulletMapper.Get entityId

            // check if bullet is inside the render boundary
            let inBoundary = boundaries.Contains transform.Position.ToPoint2

            if bullet.Entered && (not inBoundary) then
                // despawn bullet if it left the boundary
                this.DestroyEntity entityId
            else
                // mark enemy as having entered boundary
                if inBoundary then
                    bullet.Entered <- true

                // transform.Position <- transform.Position + bullet.Velocity * dt

                ()

            ()

        ()
    
    type EnemySpawner (time: float32, maxCount: uint) =

        let mutable count = 0u        
        let mutable timer = 0f

        member this.Time = time
        // member this.Count with get() = count and set(value) = count <- value
        member this.IncrementCount () = count <- count + 1u
        member this.DecrementCount () = if count > 0u then count <- count - 1u
        member this.SpawnAllowed = count < maxCount
        member this.MaxCount = maxCount
        member this.TimerExpired = timer < 0f
        member this.TickTimer time = 
            timer <- timer - time
            this.TimerExpired
        member this.RewindTimer () = 
            timer <- timer + time


    type EnemySpawnerSystem () =
        inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<EnemySpawner>))

        // mappers for accessing components

        let mutable transformMapper: ComponentMapper<Transform2> = null
        let mutable enemySpawnerMapper: ComponentMapper<EnemySpawner> = null


        override this.Initialize(mapperService: IComponentMapperService) =
            transformMapper <- mapperService.GetMapper()
            enemySpawnerMapper <- mapperService.GetMapper()

        override this.Update gameTime =
            let dt = gameTime.GetElapsedSeconds ()

            for entityId in this.ActiveEntities do
                let transform = transformMapper.Get entityId
                let enemySpawner = enemySpawnerMapper.Get entityId

                
                while enemySpawner.TickTimer dt do
                    enemySpawner.RewindTimer ()
                    if enemySpawner.SpawnAllowed then
                        enemySpawner.IncrementCount ()
                        
                        let newEnemy = this.CreateEntity ()
                        
                        newEnemy.Attach <| Enemy ()
                        newEnemy.Attach <| Dot Color.AliceBlue
                        newEnemy.Attach <| SizeComponent (20f, 15f)

                        let newTransform = Transform2 transform.Position
                        // System.Console.WriteLine $"spawned new enemy at {newTransform.Position}"
                        newEnemy.Attach newTransform
                        newEnemy.Attach <| TweenTransformer (TweenTransformer.MoveTweener(
                            newTransform, 
                            transform.Position + Vector2.UnitY * 120f, 
                            0.7f, 
                            0.3f, 
                            EasingFunctions.CircleOut,
                            once = true
                            ))

                        
                        newEnemy.Attach <| Collision.TransformCollisionActor(newTransform, 10f, "enemy", 
                            onCollision = (fun other -> 
                            let dead = other.Tag = "player"
                            if dead then
                                enemySpawner.DecrementCount ()
                            not dead
                        ))

                        let spawner = Bullets.BulletSpawner(
                            3f, 
                            Vector2.UnitY * 150f, 
                            "enemy", 
                            fun other -> 
                                let hit = other.Tag = "player"
                                // if hit then
                                    // System.Console.WriteLine "enemy bullet hit"
                                not hit
                            )

                        newEnemy.Attach spawner
                        newEnemy.Attach <| ecs.DelayedAction((fun  _ -> spawner.Firing <- true), 1.2f)
                    ()



    