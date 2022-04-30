module Collision

open System.Linq

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems
open MonoGame.Extended.Collisions

open Tools
open System.Collections.Generic

// FIXME: remove self collision
// IDEA: store id in TransformCollisionActor? use entity id or generate own unique id?

type TransformCollisionActor
    (
        transform: Transform2,
        radius: float32,
        tag: string,
        ?onCollision: TransformCollisionActor -> bool
    ) =

    let transform = transform
    let radius = radius
    let tag = tag
    let onCollision = defaultArg onCollision (fun _ -> true)

    member this.Transform = transform
    member this.Radius with get () = radius
    member this.Tag with get () = tag

    member this.CallCollision other = 
        onCollision other

    interface Collisions.ICollisionActor with
        member this.Bounds = CircleF(Point2(transform.Position.X, transform.Position.Y), radius)
        member this.OnCollision args = ()// onCollision (args)

// interface for collision checking support
// type ICollidable =
    // abstract member CheckCollision: IShapeF -> bool // true if supplied shape collides with interface implementer


// updating and spawning system
type CollisionSystem (boundaries: RectangleF) =
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<TransformCollisionActor>))

    let collisionTreeBounds = boundaries

    let mutable collisionTree = Quadtree collisionTreeBounds

    // mappers for accessing components

    let mutable transformMapper: ComponentMapper<Transform2> = null
    let mutable collidableMapper: ComponentMapper<TransformCollisionActor> = null

    //

    let GetTree (layers:Dictionary<string, Quadtree>) key = 
        let found, tree = layers.TryGetValue key
        if found then 
            tree
        else
            let newTree = Quadtree collisionTreeBounds
            newTree

    override this.Initialize(mapperService: IComponentMapperService) =
        transformMapper <- mapperService.GetMapper()
        collidableMapper <- mapperService.GetMapper()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds ()

        // quadtree that will be used next update
        let nextCollisionTree = Quadtree collisionTreeBounds
        // let newCollisionLayers = Dictionary<string, Quadtree> ()
        
        for entityId in this.ActiveEntities do
            let transform = transformMapper.Get entityId
            let collidable = collidableMapper.Get entityId

            // let inBoundary = boundaries.Contains(transform.Position.ToPoint2())

            let shape = CircleF (transform.Position.ToPoint(), collidable.Radius)

            // for layer in collidable.CollisionLayers do
                // (GetTree newCollisionLayers layer).Insert (QuadtreeData(collidable))
            nextCollisionTree.Insert <| QuadtreeData collidable
            if not <| this.CheckCollision shape collidable then
                collisionTree.Insert <| QuadtreeData collidable
                this.DestroyEntity entityId
            ()


        // removes unneccesary leaf nodes and simplifies the new quad tree
        nextCollisionTree.Shake ()
        // replace the old quadtree with the new one in preparation for the next update
        collisionTree <- nextCollisionTree

        ()

    member this.CheckCollision other collidable  =
        let mutable survivedCollision = true
        for collidable2 in (collisionTree.Query other).Where(fun boid -> boid.Bounds.Intersects other).Select(fun c -> c.Target :?> TransformCollisionActor) do
            survivedCollision <- collidable.CallCollision collidable2

        // for layer in collidable.CollisionLayers do
            // for collidable in ((GetTree collisionLayers layer).Query other).Where(fun boid -> boid.Bounds.Intersects other).Select(fun c -> (c.Target :?> TransformCollisionActor)) do
                // survivedCollision <- collidable.CallCollision()
                // ()
        
        survivedCollision

