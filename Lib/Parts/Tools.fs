module Tools

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
