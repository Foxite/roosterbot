﻿using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Meta {
	[Group("#ChannelConfigModule_Group"), HiddenFromList]
	public class ChannelConfigModule : RoosterModule {
		public CultureNameService CultureNameService { get; set; } = null!;

		[Command("#ChannelConfigModule_Prefix"), RequireBotManager]
		public async Task<CommandResult> CommandPrefix(string? prefix = null) {
			if (prefix == null) {
				return TextResult.Info(GetString("ChannelConfigModule_GetPrefix", ChannelConfig.CommandPrefix));
			} else {
				ChannelConfig.CommandPrefix = prefix;
				await ChannelConfig.UpdateAsync();
				return TextResult.Success(GetString("ChannelConfigModule_SetPrefix", ChannelConfig.CommandPrefix));
			}
		}

		[Command("#ChannelConfigModule_Language"), RequireBotManager]
		public async Task<CommandResult> Language(CultureInfo? culture = null) {
			if (culture == null) {
				return TextResult.Info(GetString("ChannelConfigModule_GetLanguage", CultureNameService.GetLocalizedName(ChannelConfig.Culture, ChannelConfig.Culture)));
			} else {
				ChannelConfig.Culture = culture;
				await ChannelConfig.UpdateAsync();
				return TextResult.Success(GetString("ChannelConfigModule_SetLanguage", CultureNameService.GetLocalizedName(ChannelConfig.Culture, ChannelConfig.Culture)));
			}
		}

		[Command("set string"), RequireBotManager]
		public async Task<CommandResult> SetValue(string key, string value) {
			ChannelConfig.SetData(key, value);
			await ChannelConfig.UpdateAsync();
			return TextResult.Success($"Set {key} to {value} for this channel.");
		}

		[Command("set int"), RequireBotManager]
		public async Task<CommandResult> SetValue(string key, int value) {
			ChannelConfig.SetData(key, value);
			await ChannelConfig.UpdateAsync();
			return TextResult.Success($"Set {key} to {value} for this channel.");
		}
	}

	// Should be a nested class, temporary workaround for submodules not working
	[Group("config module"), HiddenFromList, RequireBotManager]
	public class ModuleDisableModule : RoosterModule {
		[Command("disable")]
		public async Task<CommandResult> DisableModule(string typeName) {
			if (typeName == nameof(ChannelConfigModule) || typeName == nameof(ModuleDisableModule)) {
				return TextResult.Warning("I would not recommend that.");
			} else {
				ChannelConfig.DisabledModules.Add(typeName);
				await ChannelConfig.UpdateAsync();
				return TextResult.Success("Module has been disabled in this channel.");
			}
		}

		[Command("enable")]
		public async Task<CommandResult> EnableModule(string typeName) {
			ChannelConfig.DisabledModules.Remove(typeName);
			await ChannelConfig.UpdateAsync();
			return TextResult.Success("Module has been enabled in this channel.");
		}

		[Command("reset")]
		public async Task<CommandResult> EnableAllModules() {
			ChannelConfig.DisabledModules.Clear();
			await ChannelConfig.UpdateAsync();
			return TextResult.Success("All disabled modules have been re-enabled for this channel.");
		}

		[Command("list")]
		public CommandResult ListDisabledModules() {
			if (ChannelConfig.DisabledModules.Count == 0) {
				return TextResult.Info("No modules are disabled for this channel.");
			} else {
				return TextResult.Info("Disabled modules for this channel:\n- " + string.Join("\n-", ChannelConfig.DisabledModules));
			}
		}
	}
}
