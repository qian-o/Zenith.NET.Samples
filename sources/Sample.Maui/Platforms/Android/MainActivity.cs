using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using AndroidX.Core.View;

namespace Sample.Maui.Platforms.Android;

[Activity(ConfigurationChanges = ConfigChanges.Density | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.UserLandscape, Theme = "@style/Maui.SplashTheme", MainLauncher = true)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        Window?.Attributes?.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;

        WindowInsetsControllerCompat? compat = WindowCompat.GetInsetsController(Window, Window?.DecorView);
        compat?.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
        compat?.Hide(WindowInsetsCompat.Type.SystemBars());

        EdgeToEdge.Enable(this);
    }
}
