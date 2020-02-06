﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public class UserIdentifierParser : RoosterTypeParser<IdentifierInfo> {
		public override string TypeDisplayName => throw new NotImplementedException(); // TODO

		public async override ValueTask<RoosterTypeParserResult<IdentifierInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			bool byMention;
			IdentifierInfo? result;

			if (input.ToLower() == context.ServiceProvider.GetRequiredService<ResourceService>().GetString(context.Culture, "IdentifierInfoReader_Self")) {
				result = context.UserConfig.GetIdentifier();
				byMention = false;
			} else {
				RoosterTypeParserResult<IUser> userResult = await context.ServiceProvider.GetRequiredService<RoosterCommandService>().GetPlatformSpecificParser<IUser>().ParseAsync(parameter, input, context);
				if (userResult.IsSuccessful) {
					result = (await context.ServiceProvider.GetRequiredService<UserConfigService>().GetConfigAsync(userResult.Value.GetReference())).GetIdentifier();
					byMention = true;
				} else {
					// TODO update resource names
					return Unsuccessful(false, context, "#StudentSetInfoReader_CheckFailed_Direct");
				}
			}

			if (result is null) {
				string message;
				if (byMention) {
					message = "#StudentSetInfoReader_CheckFailed_MentionUser";
				} else {
					message = "#StudentSetInfoReader_CheckFailed_MentionSelf";
				}
				return Unsuccessful(true, context, message, context.ChannelConfig.CommandPrefix);
			} else {
				return Successful(result);
			}
		}
	}
}