using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoosterBot.Tests {
	[TestClass]
	public class VersionPredicateTests {
		[TestMethod]
		public void ConstructorTest() {
			Assert.That.DoesNotThrowException(() => { new VersionPredicate(1, 0, 0); });
			Assert.That.DoesNotThrowException(() => { new VersionPredicate(1, 0, null); });
			Assert.That.DoesNotThrowException(() => { new VersionPredicate(1, null, null); });
			Assert.ThrowsException<ArgumentException>(() => { new VersionPredicate(1, null, 0); });
		}

		[TestMethod]
		public void MatchTest() {
			var predicate = new VersionPredicate(1, 2, 3);

			Assert.IsTrue (predicate.Matches(new Version(1, 2, 3)));

			Assert.IsFalse(predicate.Matches(new Version(1, 2, 4)));
			Assert.IsFalse(predicate.Matches(new Version(1, 3, 3)));
			Assert.IsFalse(predicate.Matches(new Version(2, 2, 3)));
			Assert.IsFalse(predicate.Matches(new Version(2, 3, 3)));
			Assert.IsFalse(predicate.Matches(new Version(2, 2, 4)));
			Assert.IsFalse(predicate.Matches(new Version(2, 4, 3)));
			
			predicate = new VersionPredicate(1, 2, null);
			Assert.IsTrue (predicate.Matches(new Version(1, 2, 3)));
			Assert.IsTrue (predicate.Matches(new Version(1, 2, 4)));
			Assert.IsFalse(predicate.Matches(new Version(1, 3, 3)));
			Assert.IsFalse(predicate.Matches(new Version(2, 2, 3)));
			Assert.IsFalse(predicate.Matches(new Version(2, 3, 3)));
			
			predicate = new VersionPredicate(1, null, null);
			Assert.IsTrue (predicate.Matches(new Version(1, 2, 3)));
			Assert.IsTrue (predicate.Matches(new Version(1, 2, 4)));
			Assert.IsTrue (predicate.Matches(new Version(1, 3, 3)));
			Assert.IsFalse(predicate.Matches(new Version(2, 2, 3)));
			Assert.IsFalse(predicate.Matches(new Version(2, 3, 3)));
		}
	}
}