using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoosterBot.Tests {
	public static class Extensions {
		public static void DoesNotThrowException(this Assert _, Action action) {
			try {
				action();
			} catch (Exception e) {
				Assert.Fail("Delegate threw exception: " + e.ToString());
			}
		}

		public static void DoesNotThrowException<T>(this Assert _, Action action) where T : Exception {
			try {
				action();
			} catch (T e) {
				Assert.Fail("Delegate threw exception: " + e.ToString());
			}
		}

		public static async Task DoesNotThrowExceptionAsync(this Assert _, Func<Task> action) {
			try {
				await action();
			} catch (Exception e) {
				Assert.Fail("Delegate threw exception: " + e.ToString());
			}
		}

		public static async Task DoesNotThrowException<T>(this Assert _, Func<Task> action) where T : Exception {
			try {
				await action();
			} catch (T e) {
				Assert.Fail("Delegate threw exception: " + e.ToString());
			}
		}
	}
}
