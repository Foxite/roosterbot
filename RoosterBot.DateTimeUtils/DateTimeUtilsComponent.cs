﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DateTimeUtils {
	public class DateTimeUtilsComponent : Component {
		public override Version ComponentVersion => new Version(1, 3, 1);

		public override IEnumerable<string> Tags => new[] { "DayOfWeekReader" };
		
#nullable disable
		internal static ResourceService ResourceService { get; private set; }
#nullable restore

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			ResourceService = services.GetRequiredService<ResourceService>();
			ResourceService.RegisterResources("RoosterBot.DateTimeUtils.Resources");

			commandService.AddTypeParser(new DayOfWeekParser(), true);
		}
	}
}
