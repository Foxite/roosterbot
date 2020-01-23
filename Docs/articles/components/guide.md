## Components
The foundation for a component is a .NET assembly that defines exactly one class deriving from [Component](xref:RoosterBot.Component). It also has some restrictions on its location relative to RoosterBot.exe, namely, it must in `Components/{name}/{name}.dll` relative to the executable. For the best results, you can copy-paste the csproj file for one of the official components, and modify it with your name and remove the unnecessary references. You should keep the reference to RoosterBot, if it wasn't obvious.

[Component](xref:RoosterBot.Component) has a number of abstract or virtual members:

## ComponentVersion

## Tags

## CheckDependencies

## AddServices

## AddModules

## Dispose
