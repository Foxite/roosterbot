using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[LogTag("UserClassModule"), Name("#UserClassModule_Name"), Summary("#UserClassModule_Summary")]
	public class UserClassModule : EditableCmdModuleBase {
		public IUserClassesService Classes { get; set; }
		public UserClassRoleService Roles { get; set; }
		public IdentifierValidationService Validation { get; set; }

		[Command("ik"), Summary("#UserClassModule_GetClassForUser_Summary")]
		public async Task GetClassForUser() {
			string clazz = (await Classes.GetClassForDiscordUserAsync(Context, Context.User))?.DisplayText;
			string response;
			if (clazz == null) {
				response = GetString("UserClassModule_GetClassForUser_UserNotKnown");
			} else {
				response = GetString("UserClassModule_GetClassForUser_UserIsInClass", clazz);
			}
			response += GetString("UserClassModule_GetClassForUser_ChangeHint");
			await ReplyAsync(response);
		}
		
		[Command("ik"), Summary("#UserClassModule_SetClassForUser_Summary")]
		public async Task SetClassForUser([Name("#UserClassModule_SetClassForUser_class_Name")] string clazzName) {
			StudentSetInfo studentSet = await Validation.ValidateAsync<StudentSetInfo>(Context, clazzName);
			if (studentSet != null) {
				StudentSetInfo oldStudentSet = await Classes.GetClassForDiscordUserAsync(Context, Context.User);
				StudentSetInfo newStudentSet = new StudentSetInfo() { ClassName = clazzName };

				await Classes.SetClassForDiscordUserAsync(Context, Context.User, newStudentSet);
				await ReplyAsync(GetString("UserClassModule_SetClassForUser_ConfirmUserIsInClass", studentSet.DisplayText));

				// Assign roles
				// TODO move out
				IGuildUser guildUser = Context.User as IGuildUser;

				try {
					IRole[] oldRoles = Roles.GetRolesForStudentSet(Context.Guild, oldStudentSet).ToArray();
					IRole[] newRoles = Roles.GetRolesForStudentSet(Context.Guild, newStudentSet).ToArray();

					await guildUser.RemoveRolesAsync(oldRoles);
					await guildUser.AddRolesAsync(newRoles);
				} catch (Exception) {
					// Ignore, either we did not have permission or the roles were not found. In either case, it doesn't matter.
				}
			} else {
				await ReplyAsync(GetString("UserClassModule_SetClassForUser_InvalidClass"));
			}
		}
	}
}
