# Logo Setup

Your logo image should be placed at:

```
mobile/src/iNdex.Todo.Mobile/wwwroot/logo.png
```

The app references it as `logo.png` in:
- Splash screen (`wwwroot/index.html`)
- App header (`Components/Layout/MainLayout.razor`)
- Dashboard landing page (`Components/Pages/Dashboard.razor`)

## Steps

1. Copy your logo PNG to `wwwroot/logo.png`
2. For best results use a square PNG at 512×512 or higher

## Platform icons

For the MAUI native app icon (used by Android launcher, iOS home screen, Windows taskbar):

1. Place your logo at `Resources/Images/appicon.png` (1024×1024, with transparent background)
2. Add to the `.csproj`:

```xml
<MauiImage Include="Resources\Images\appicon.png">
    <Resize>true</Resize>
    <BaseSize>128,128</BaseSize>
</MauiImage>
```

Or use the MAUI Asset Studio in Visual Studio (Right-click project → Add → New Item → MAUI → App Icon).
