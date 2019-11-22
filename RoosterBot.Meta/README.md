RoosterBot.Meta provides essential features for RoosterBot. The reason these are not built into RoosterBot is twofold:
- RoosterBot is *intended* not to have any public functionality, only to facilitate components using its services.
- Certain provisions should be optional, as explained below.

This means that commands such as 

This component provides:
- A file-based implementation of UserConfigService and GuildConfigService. These can be disabled in the config file for this component, if you want to use another implementation.
- A module for bot managers and guild staff members to change the GuildConfig for a guild, and a module for users to change their preferred language.
- A help command, which uses the built-in HelpService.
- A command that shows a list of all commands by module.
- An info command which shows the installed components and a link to the source code repository.
- An uptime command.
- A module that allows a bot manager to manually send or delete commands via the bot, shut it down, and restart it.