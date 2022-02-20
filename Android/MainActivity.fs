namespace SpaceStratAndroid

open System

open Android.App
// open Android.Content
open Android.Content.PM
open Android.OS
// open Android.Runtime
open Android.Views
// open Android.Widget
open Microsoft.Xna.Framework

type Resources = Android.Resource

[<Activity (
    // Label = "Android", 
    Label = "@string/app_name",
    MainLauncher = true,
    // Icon = "@mipmap/icon"
    Icon = "@drawable/icon",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.Landscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )>]
type MainActivity () =
    inherit AndroidGameActivity ()

    let mutable game: Game1 = null
    let mutable view: View = null

    override this.OnCreate (bundle) =

        base.OnCreate (bundle)

        game = Game1 ()
        view = game.Services.GetService (typeof(View)) :> View

        this.SetContentView (view)

        game.Run ()

