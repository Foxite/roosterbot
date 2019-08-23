using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoosterBot.Tests {
	[TestClass]
	public class VersionTests {
		[TestMethod]
		public void OperatorTest() {
			// == operator
			Assert.IsTrue(new Version(1, 0, 0) == new Version(1, 0, 0));
			Assert.IsFalse(new Version(1, 1, 0) == new Version(1, 0, 0));
			Assert.IsFalse(new Version(1, 1, 0) == new Version(1, 0, 0));
			Assert.IsFalse(new Version(1, 0, 1) == new Version(1, 0, 0));

			// > operator
			Assert.IsTrue(new Version(1, 0, 1) > new Version(1, 0, 0));
			Assert.IsTrue(new Version(1, 1, 0) > new Version(1, 0, 0));
			Assert.IsTrue(new Version(2, 0, 0) > new Version(1, 2, 3));
			Assert.IsFalse(new Version(1, 2, 3) > new Version(2, 0, 0));

			// < operator
			Assert.IsFalse(new Version(1, 0, 1) < new Version(1, 0, 0));
			Assert.IsFalse(new Version(1, 1, 0) < new Version(1, 0, 0));
			Assert.IsFalse(new Version(2, 0, 0) < new Version(1, 2, 3));
			Assert.IsTrue(new Version(1, 2, 3) < new Version(2, 0, 0));
			
			// >=, <=, and != simply call the other operators, so we don't really need to test these.
		}
	}
}