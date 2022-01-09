namespace Danmaku

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems


type Player(maxVelocity: float32)=
    let mutable hp = 100f
    
    let maxVelocity = maxVelocity
    let mutable velocity = Vector2.Zero
    

    let capVelocity() =
        if(velocity.LengthSquared() > maxVelocity**2f) then
            velocity <- Vector2.Normalize(velocity) * maxVelocity
        ()

    member this.HP with get () = hp and set (value) = hp <-value
    member this.CurrentVelocity with get() = velocity
    
    member this.SetVelocity (newVelocity: Vector2) = 
        velocity <- newVelocity
        capVelocity()
        
    member this.SetVelocity (newVelocityMagnitude: float32) = 
        velocity <- Vector2.Normalize(velocity) * newVelocityMagnitude
        capVelocity()
        

type PlayerUpdateSystem()=
    inherit EntityUpdateSystem(Aspect.All(typedefof<Transform2>, typedefof<Player>))

    [<DefaultValue>]
    val mutable transformMapper: ComponentMapper<Transform2>
    [<DefaultValue>]
    val mutable playerMapper: ComponentMapper<Player>

    override this.Initialize(mapperService: IComponentMapperService) =
        this.transformMapper <- mapperService.GetMapper<Transform2>()
        this.playerMapper <- mapperService.GetMapper<Player>()
        ()

    override this.Update(gameTime: GameTime) =
        let dt = gameTime.GetElapsedSeconds()

        for entityId in this.ActiveEntities do
            let mutable transform = this.transformMapper.Get entityId
            let mutable player = this.playerMapper.Get entityId
            // transform.Rotation <- transform.Rotation + dt * 10f
            transform.Position <- transform.Position + player.CurrentVelocity*dt
        
        ()

    