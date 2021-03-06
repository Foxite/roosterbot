RoosterBot.Meta provides essential features for RoosterBot. The reason these are not built into RoosterBot is twofold:
- RoosterBot is *intended* not to have any features exposed to the user, only to facilitate components using its services.
- Certain provisions should be optional, as explained below.

This component provides:
- A file-based implementation of UserConfigService and ChannelConfigService. These can be disabled in the config file for this component, if you want to use another implementation.
- A module for bot managers and channel staff members to change the ChannelConfig for a channel, and a module for users to change their preferred language.
- A command that shows a list of all commands by module.
- An info command which shows the installed components and a link to the source code repository.
- An uptime command.
- A module that allows a bot manager to manually send or delete commands via the bot, shut it down, and restart it.