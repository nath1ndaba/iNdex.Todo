# Fonts

Place the following font files in this directory:

- `OpenSans-Regular.ttf`
- `OpenSans-Semibold.ttf`

Download from: https://fonts.google.com/specimen/Open+Sans

Then ensure the `.csproj` references them:

```xml
<MauiFont Include="Resources\Fonts\OpenSans-Regular.ttf" />
<MauiFont Include="Resources\Fonts\OpenSans-Semibold.ttf" />
```

These entries are already declared in `iNdex.Todo.Mobile.csproj` via the
`MauiFont` glob pattern. Simply dropping the `.ttf` files here is enough.
