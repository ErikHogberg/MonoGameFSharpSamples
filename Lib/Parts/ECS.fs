module ecs

open MonoGame.Extended
open MonoGame.Extended.Entities
open MonoGame.Extended.Entities.Systems

type DelayedAction(action: unit -> unit, delay) =

    let mutable timer: float32 = delay

    member this.TickTimer dt = 
        timer <- timer - dt
        timer < 0f
    member this.OnExpire = action

type DelayedActionSystem () =
    inherit EntityUpdateSystem(Aspect.All(typedefof<DelayedAction>))

    let mutable mapper: ComponentMapper<DelayedAction> = null

    override this.Initialize (mapperService: IComponentMapperService) =
        mapper <- mapperService.GetMapper ()

    override this.Update gameTime =
        let dt = gameTime.GetElapsedSeconds ()

        for entityId in this.ActiveEntities do
            let delayedAction = mapper.Get entityId
            if delayedAction.TickTimer dt then 
                delayedAction.OnExpire ()
                (this.GetEntity entityId).Detach<DelayedAction> ()
                ()
            ()
        ()



