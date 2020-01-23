## Platform components
A Platform component is a [Component] that derives from [PlatformComponent](xref:RoosterBot.PlatformComponent) instead of [Component](xref:RoosterBot.Component). It is not possible to define both a "regular" component and a PlatformComponent in the same assembly, as they both derive from [Component](xref:RoosterBot.Component), which is illegal.

PlatformComponent defines two additional abstract methods:

## Connect

## Disconnect
