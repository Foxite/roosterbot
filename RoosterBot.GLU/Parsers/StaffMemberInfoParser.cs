﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class StaffMemberInfoParser : IdentifierInfoParserBase<StaffMemberInfo> {
		public override string TypeDisplayName => "#StaffMemberInfoReader_TypeDisplayName";

		public async override ValueTask<RoosterTypeParserResult<StaffMemberInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			RoosterTypeParserResult<StaffMemberInfo> baseResult = await base.ParseAsync(parameter, input, context);
			if (baseResult.IsSuccessful) {
				return baseResult;
			} else {
				StaffMemberService tns = context.ServiceProvider.GetRequiredService<StaffMemberService>();
				StaffMemberInfo? result = null;

				IUser? user = null;
				TypeParserResult<IUser> userResult = await context.ServiceProvider.GetRequiredService<RoosterCommandService>().GetPlatformSpecificParser<IUser>().ParseAsync(parameter, input, context);
				if (userResult.IsSuccessful) {
					user = userResult.Value;
				}

				if (user == null) {
					if (input.Length >= 3) {
						IReadOnlyCollection<StaffMemberMatch> lookupResults = tns.Lookup(context.ChannelConfig.ChannelReference, input);
						if (lookupResults.Count == 1) {
							result = lookupResults.First().StaffMember;
						} else if (lookupResults.Count > 1) {
							return Unsuccessful(true, "#StaffMemberInfoReader_MultipleMatches", string.Join(", ", lookupResults.Select(match => match.StaffMember.DisplayText)));
						}
					}
				} else if (context.IsPrivate) {
					result = tns.GetStaffMemberByDiscordUser(context.Channel.GetReference(), user);
				}

				if (result == null) {
					return Unsuccessful(false, "#StaffMemberInfoReader_CheckFailed");
				} else {
					return Successful(result);
				}
			}
		}
	}
}
