module Collision

open System.Linq

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Collisions

open Tools
open RenderSystem
open System.Collections.Generic

// open type System.MathF


type Collidable(size: float32, collisionLayers: list<string>, onCollide: unit -> bool) =

    let mutable size = size
    let collisionLayers = collisionLayers

    // returns true if collision did not destroy entity
    let onCollide = onCollide

    member this.Size
        with get () = size
        and set (value) = size <- value

    member this.CallCollision other =
        onCollide other

    member this.CollisionLayers
        with get () = collisionLayers



// updating and spawning system
type CollisionSystem (boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Collidable>))

    let collisionTreeBounds = boundaries //RectangleF (0f,0f, 1500f, 1500f)

    let collisionLayers = Dictionary<string, Quadtree> ()
    // let mutable collisionTree = Quadtree collisionTreeBounds


    let random = new FastRandom()

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable collidableMapper: ComponentMapper<Collidable> = null

    //

    let GetTree (layers:Dictionary<string, Quadtree>) key = 
        let found, tree = layers.TryGetValue key
        if found then 
            tree
        else
            let newTree = Quadtree collisionTreeBounds
            collisionLayers[key] <- newTree
            newTree

    member this.GetOldTree key =
        GetTree collisionLayers key

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper<Transform2>()
        collidableMapper <- mapperService.GetMapper<Collidable>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        // quadtree that will be used next update
        // let nextCollisionTree = Quadtree collisionTreeBounds
        let newCollisionLayers = Dictionary<string, Quadtree> ()
        
        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get(entityId)
            let collidable = collidableMapper.Get(entityId)

            // check if asteroid is inside the render boundary
            let inBoundary = boundaries.Contains(transform.Position.ToPoint2())
            let x1 = transform.Position.X
            let y1 = transform.Position.X
            let pos = new Point2(x1, y1)
            let x = (CircleF(pos, collidable.Size))
            if this.CheckCollision x collidable.CollisionLayers then
                for layer in collidable.CollisionLayers do
                  (GetTree newCollisionLayers layer).Insert (QuadtreeData(Tools.TransformCollisionActor(transform, collidable.Size, (fun _ -> ()), collidable)))

            ()


        // replace the old quadtree with the new one in preparation for the next update
        // collisionTree <- nextCollisionTree
        // removes unneccesary leaf nodes and simplifies the new quad tree
        // nextCollisionTree.Shake()

        ()

    member this.CheckCollision other (layers: list<string>) =
        let mutable survivedCollision = true
        for layer in layers do
            for collidable in ((GetTree collisionLayers layer).Query other).Where(fun boid -> boid.Bounds.Intersects other).Select(fun c -> (c.Target :?> TransformCollisionActor).Data :?> Collidable) do
                survivedCollision <- collidable.CallCollision ()
        
        // if (collisionTree.Query other).Any( fun boid -> boid.Bounds.Intersects other ) then
        //     null
        // else
        //     null

        survivedCollision

    // interface ICollidable with
        // member this.CheckCollision other =
            // (collisionTree.Query other).Any( fun boid -> boid.Bounds.Intersects other )

