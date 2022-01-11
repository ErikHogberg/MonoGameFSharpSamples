module Tools
open Microsoft.Xna.Framework
open MonoGame.Extended

module Singleton =
  open Microsoft.Xna.Framework.Graphics

  type Product internal () =
    // let mutable state = 0
    let mutable dict :Map<string, Texture2D> = Map []

    member this.Set(key: string, value: Texture2D) =
        // state <- state + 1
        // printfn "Doing something for the %i time" state
        dict <- dict.Add (key, value)

    member this.Get(key: string) =
        // state <- state + 1
        // printfn "Doing something for the %i time" state
        dict[key]


  let Instance = Product()

type Point2 with
  member this.ToVector () = Vector2(this.X, this.Y)

type TransformCollisionActor(transform: Transform2, radius: float32, onCollision: Collisions.CollisionEventArgs -> unit, data) =
  let transform = transform
  let radius = radius
  let data = data

  member this.Data with get () = data
  member this.Transform with get () = transform

  interface Collisions.ICollisionActor with
    member this.Bounds with get () = CircleF(transform.Position, radius)
    member this.OnCollision(args) = onCollision(args)


// type QuadtreeNode(bounds: RectangleF, levelsLeft: int, splitSize: int) =
//   let bounds = bounds
//   let levelsLeft = levelsLeft
//   let splitSize = splitSize
//   let mutable data = List.Empty

//   let mutable NW = null
//   let mutable NE = null
//   let mutable SW = null
//   let mutable SE = null

//   member this.Nodes with get ()= [|NW, NE, SW, SE|]

// type Quadtree (bounds: RectangleF, maxLevels: int, splitSize: int) =
//   let root = new QuadtreeNode(bounds, maxLevels, splitSize)

  

