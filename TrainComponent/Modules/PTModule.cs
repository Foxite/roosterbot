using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Modules;
using TrainComponent.Services;

namespace TrainComponent.Modules {
	public class PTModule : EditableCmdModuleBase {
		public NSAPI NSAPI { get; set; }

		[Command("ov")]
		public async Task GetTrainRouteCommand([Remainder] string parameters) {
			
		}
	}
}
