using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoosterBot.Attributes {
	public class PartOfComponentAttribute : Attribute {
		public Type ComponentType { get; set; }

		public PartOfComponentAttribute(Type componentType) {
			ComponentType = componentType;
		}
	}
}
