module Ship

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open Tools
open Microsoft.FSharp.Collections

type Tile(tilePos: Point) =
    let pos = tilePos

    let texture = Singleton.Instance.Get("dot")

    member this.Draw(spriteBatch: SpriteBatch) =
        spriteBatch.Draw(texture, new Rectangle(pos.X, pos.Y, 10,10), Color.Gold)


type SpaceShip(shipTexture: Texture2D, startPos: Point, size: int) =

    let texture = shipTexture
    let pos = startPos


    let tiles = [new Tile(new Point(0,0))]


    member this.Draw(spriteBatch: SpriteBatch) =
        spriteBatch.Draw(texture, new Rectangle(pos.X, pos.Y, int size, int size), Color.Gold)
        for tile in tiles do
            tile.Draw(spriteBatch)


