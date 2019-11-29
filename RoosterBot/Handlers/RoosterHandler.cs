using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public abstract class RoosterHandler {
		protected RoosterHandler(IServiceProvider isp) {
			IEnumerable<PropertyInfo> props = GetType().GetProperties().Where(prop => prop.SetMethod != null && prop.SetMethod.IsPublic);
			foreach (PropertyInfo prop in props) {
				prop.SetValue(this, isp.GetRequiredService(prop.PropertyType));
			}
		}
	}
}
