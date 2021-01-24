using System;

namespace RoosterBot {
	/// <summary>
	/// Specifies that a command, or all commands in a module, should be responded to in a private channel with the user.
	/// 
	/// Regardless of the response, the command should be acknowledged in the channel it was received.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RespondInPrivateAttribute : Attribute { }
}
