using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[Name("#UserClassModule_Name"), Summary("#UserClassModule_Summary")]
	public class UserClassModule : RoosterModuleBase {
		private IUserClassesService Classes { get; }
		private IdentifierValidationService Validation { get; }

		public UserClassModule(IUserClassesService classes, IdentifierValidationService validation) {
			Classes = classes;
			Validation = validation;
		}

		[Command("ik", RunMode = RunMode.Async), Summary("#UserClassModule_GetClassForUser_Summary")]
		public async Task GetClassForUser() {
			string? clazz = (await Classes.GetClassForDiscordUserAsync(Context, Context.User))?.DisplayText;
			string response;
			if (clazz == null) {
				response = GetString("UserClassModule_GetClassForUser_UserNotKnown");
			} else {
				response = GetString("UserClassModule_GetClassForUser_UserIsInClass", clazz);
			}
			response += GetString("UserClassModule_GetClassForUser_ChangeHint", GuildConfig.CommandPrefix);
			await ReplyAsync(response);
		}
		
		[Command("ik", RunMode = RunMode.Async), Summary("#UserClassModule_SetClassForUser_Summary"), RequireContext(ContextType.Guild, ErrorMessage = "Deze command moet binnen een server worden gebruikt.")]
		public async Task SetClassForUser([Name("#UserClassModule_SetClassForUser_class_Name")] string clazzName) {
			StudentSetInfo? newStudentSet = await Validation.ValidateAsync<StudentSetInfo>(Context, clazzName);
			if (newStudentSet != null) {
				StudentSetInfo? oldStudentSet = await Classes.SetClassForDiscordUserAsync(Context, (IGuildUser) Context.User, newStudentSet);
				if (oldStudentSet == null) {
					await ReplyAsync(GetString("UserClassModule_SetClassForUser_ConfirmUserIsInClass", newStudentSet.DisplayText));
				} else {
					await ReplyAsync(GetString("UserClassModule_SetClassForUser_ConfirmUserIsInClassWithOld", newStudentSet.DisplayText, oldStudentSet.DisplayText));
				}
			} else {
				await ReplyAsync(GetString("UserClassModule_SetClassForUser_InvalidClass"));
			}
		}
	}
}
