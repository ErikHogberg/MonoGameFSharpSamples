module Boids

open System.Linq

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Collisions

open Tools

open type System.MathF
open RenderSystem

// Boids system where entites are pulled towards a defined point, but with flocking behaviour affecting their path to the target
// Shows using quadtrees to make the entities spatially aware of eachother, like a collision broad-phase

type Boid(velocity: Vector2, size: float32, timeRemaining: float32) =

    // timed life, time until despawn
    let mutable timeRemaining = timeRemaining

    // current velocity
    let mutable velocity = velocity
    
    // size of boid, only used for rendering
    // let mutable size = size

    member this.Velocity
        with get () = velocity
        and set (value) = velocity <- value

    // member this.Size
    //     with get () = size
    //     and set (value) = size <- value

    member this.TimeRemaining
        with get () = timeRemaining
        and set (value) = timeRemaining <- value


// updating and spawning system
type BoidsSystem (boundaries: EllipseF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Boid>))

    // IDEA: use tweening lib to define gravity target pull force drop off over distance

    // IDEA: re-add wind

    let visualRange = 100f
    let visualSize = 1f

    // size and position of quadtree, no collision/spatial awareness with neigbor entities happen for entities outside of these bounds
    let collisionTreeBounds = RectangleF (0f,0f, 1500f, 1500f)
    let mutable collisionTree = Quadtree collisionTreeBounds

    let random = new FastRandom()

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable boidMapper: ComponentMapper<Boid> = null

    // gravity target of boids, pull force magnitude corresponds to radius
    let mutable target = CircleF(Point2(1f,1f), 1f)

    // random range for boid spawn delay
    let MinSpawnDelay = 0.0f
    let MaxSpawnDelay = 0.1f
    let mutable spawnDelay = MaxSpawnDelay

    // max velocity of entities
    let velocityCap = 100f

    // ellipse boundary only used for path of spawner box
    let mutable boundaries = boundaries

    // cached spawner box angle from center
    let mutable spawnAngle = 0f
    let mutable spawnSpeed = 100f

    // size of random spawn box
    let spawnOffsetRange = Point2(MathHelper.Max(boundaries.RadiusX, boundaries.RadiusY), 50f)

    // limit on boids active at same time
    let mutable spawnCount = 0
    let maxBoids = 1000

    // boid flocking settings
    let separationSteerSpeed = 2f
    let cohesionSteerSpeed = 2f
    let alignmentSteerSpeed = 2f

    // multiplier on gravity target radius
    let targetSteerMul = 0.06f

    // define target flocking distance range
    let maxDistanceSqr = 30f
    let minDistanceSqr = 25f

    // accessors

    member this.Target
        with set (value) = target <- value

    member this.SpawnAngle
        with get () = spawnAngle
        and set (value) = spawnAngle <- value
    member this.SpawnSpeed
        with set (value) = spawnSpeed <- value

    // spawner box calculations

    member this.SpawnVelocity (angle: float32, speed: float32) = Vector2.UnitY.Rotate(angle) * speed
    member this.SpawnVelocity() = this.SpawnVelocity(spawnAngle, spawnSpeed)

    member this.PointOnBoundary
        with get() = boundaries.Center
            + Vector2(
                boundaries.RadiusX * Cos(spawnAngle + PI*0.5f), 
                boundaries.RadiusY * Sin(spawnAngle + PI*0.5f));

    member this.RandomSpawnPos
        with get() = Vector2(
                random.NextSingle(-spawnOffsetRange.X, spawnOffsetRange.X),
                random.NextSingle(-spawnOffsetRange.Y, spawnOffsetRange.Y)
            ).Rotate(spawnAngle)

    member this.SpawnRange () = 
        let center = spawnOffsetRange.Inverse
        RectangleF(center, spawnOffsetRange.ToSize * 2f)    
        // RectangleF(spawnOffsetRange, (spawnOffsetRange * 2f).ToSize())

    //

    member this.CreateBoid (position: Vector2) velocity size =
        let entity = this.CreateEntity ()
        let transform = Transform2 position
        let boid = Boid(velocity, size, random.NextSingle(20f, 30f))
        entity.Attach<| transform
        entity.Attach<| boid
        entity.Attach<| Dot Color.Orange
        entity.Attach<| SizeComponent size
        spawnCount <- spawnCount + 1
        entity.Id

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        boidMapper <- mapperService.GetMapper<Boid>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // quadtree that will be used next update
        let nextCollisionTree = Quadtree collisionTreeBounds
        
        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let boid = boidMapper.Get entityId

            // get all neighbor entities in "visual" range
            let nearby = collisionTree.Query <| CircleF(transform.Position.ToPoint2, visualRange)

            if nearby.Count > 0 then

                // let mutable closestNearby = nearby[0]
                // let mutable closestDistanceSqr = (closestNearby.Bounds.Position.ToVector() - transform.Position).LengthSquared()

                // find closest neighbor

                // average of normalized velocity directions of nearby boids
                let mutable averageNearbyFacing = Vector2.Zero
                // average position of nearby boids
                let mutable averageNearbyCenter = Vector2.Zero
                // average direction to nearby boids from current boid
                let mutable averageNearbyDir = Vector2.Zero

                for i in 1..(nearby.Count-1) do
                    let otherBoidEntry = nearby.[i]
                    let otherBoidActor = otherBoidEntry.Target :?> Collision.TransformCollisionActor
                    let otherBoidTransform = otherBoidActor.Transform

                    // averageNearbyFacing <- averageNearbyFacing + otherBoid.Velocity.FastNormalizedCopy()
                    averageNearbyCenter <- averageNearbyCenter + otherBoidTransform.Position
                    averageNearbyDir <- averageNearbyDir + (otherBoidTransform.Position - transform.Position).FastNormalizedCopy()
                    
                    ()

                let nearbyCountF = float32 nearby.Count
                averageNearbyFacing <- averageNearbyFacing / nearbyCountF
                averageNearbyCenter <- averageNearbyCenter / nearbyCountF
                averageNearbyDir <- averageNearbyDir / nearbyCountF

            
                // flocking behaviour

                // TODO: toggle behaviours depending on distance to nearby

                // separation
                // let velocityMagnitude = boid.Velocity.Length()
                // let otherBoid = (closestNearby.Target :?> Tools.TransformCollisionActor).Data :?> Boid
                // let dir = boid.Velocity.NormalizedCopy().FasterRotateTowards (otherBoid.Velocity.NormalizedCopy()) (-separationSteerSpeed *dt)
                // boid.Velocity <- dir*velocityMagnitude
                boid.Velocity <- boid.Velocity.FasterRotateTowards averageNearbyDir (-separationSteerSpeed*dt)

                // alignment                
                // let otherBoid = (closestNearby.Target :?> Tools.TransformCollisionActor).Data :?> Boid
                // boid.Velocity <- boid.Velocity.MoveTowards(otherBoid.Velocity, (alignmentSpeed*dt))
                boid.Velocity <- boid.Velocity.FasterRotateTowards averageNearbyFacing (alignmentSteerSpeed*dt)


                // cohesion
                // let velocityMagnitude = boid.Velocity.Length()
                // let otherBoid = (closestNearby.Target :?> Tools.TransformCollisionActor).Data :?> Boid
                // let dir = boid.Velocity.FasterRotateTowards (otherBoid.Velocity) (cohesionSteerSpeed*dt)
                // boid.Velocity <- dir*velocityMagnitude
                boid.Velocity <- boid.Velocity.FasterRotateTowards (averageNearbyCenter-transform.Position) (cohesionSteerSpeed*dt)
                        

            // pull velocity towards gravity target
            boid.Velocity <- boid.Velocity + Vector2.Normalize(target.Position.ToVector - transform.Position) * target.Radius * targetSteerMul

            // cap velocity
            if boid.Velocity.LengthSquared() > velocityCap ** 2f then
                boid.Velocity <- boid.Velocity.NormalizedCopy() * velocityCap

            // move boid
            transform.Position <- transform.Position + boid.Velocity * dt

            // count down entity life timer
            boid.TimeRemaining <-
                boid.TimeRemaining
                - gameTime.GetElapsedSeconds()

            // remove expired entities
            if boid.TimeRemaining <= 0f then
                this.DestroyEntity(entityId)
                spawnCount <- spawnCount - 1
            else
                // insert updated surviving entities into new quadtree
                nextCollisionTree.Insert (QuadtreeData(Collision.TransformCollisionActor(transform, visualSize, "boid")))
                ()

            // boid.Nearby <- []

            ()

        // removes unneccesary leaf nodes and simplifies the new quad tree
        nextCollisionTree.Shake()
        // replace the old quadtree with the new one in preparation for the next update
        collisionTree <- nextCollisionTree


        // spawning new entities

        // count down spawn delay timer
        spawnDelay <- spawnDelay - dt

        // TODO: make frame-indipendent

        while spawnDelay <= 0f do

            // calculate current spawn velocity and spawn box position using accessors
            let spawnVelocity = this.SpawnVelocity(random.NextSingle(MathHelper.TwoPi), random.NextSingle(-50f, 10f) + spawnSpeed)
            let pointOnBoundary = this.PointOnBoundary

            // spawn up to 50 boids on each timer expiration
            for q in 0 .. System.Math.Min(50, maxBoids - spawnCount)  do

                // position is a randomized point in spawn box
                let position =
                    this.RandomSpawnPos
                    + pointOnBoundary

                let id =
                    this.CreateBoid <| position <| spawnVelocity <| random.NextSingle(2f, 4f)

                ()

            // spawn delay is randomized on each expiration
            spawnDelay <- spawnDelay + random.NextSingle(MinSpawnDelay, MaxSpawnDelay)

        ()

    // interface ICollidable with
        // member this.CheckCollision other =
            // (collisionTree.Query other).Any( fun boid -> boid.Bounds.Intersects other )


