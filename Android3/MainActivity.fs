namespace Android3

open System

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Views
open Android.Widget
open Microsoft.Xna.Framework

open SpaceGame

//type Resources = Android3.Resource

[<Activity (Label = "Android3", MainLauncher = true, Icon = "@mipmap/icon")>]
type MainActivity () =
    inherit AndroidGameActivity ()
    
    let mutable game: Game1 = Unchecked.defaultof<Game1>
    let mutable view: View = null
    
    override this.OnCreate (bundle) =
    
        base.OnCreate (bundle)
        
        //for file in System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory()) do
        //    let _ = Android.Util.Log.Info(file, "")
        //    ()
        
        game <- new Game1 ()
        view <- ((game.Services.GetService (typedefof<View>)) :?> View)
    
        this.SetContentView (view)
    
        game.Run ()

