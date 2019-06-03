using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Readers {
	public class TeacherInfoReader : TypeReader {
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			TeacherNameService tns = services.GetService<TeacherNameService>();
			TeacherInfo[] results = tns.Lookup(input);
			if (results.Length == 0) {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar."));
			} else {
				return Task.FromResult(TypeReaderResult.FromSuccess(results));
			}
		}
	}
}
