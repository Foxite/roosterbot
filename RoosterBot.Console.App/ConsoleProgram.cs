using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace RoosterBot.ConsoleApp {
	public class ConsoleProgram {
		private static void Main() {
			try {
				using var pipeClient = new NamedPipeClientStream(".", "roosterBotConsolePipe", PipeDirection.Out, PipeOptions.WriteThrough);
				pipeClient.Connect();
				using var sw = new StreamWriter(pipeClient, Encoding.UTF8, 2047, true);

				string input;
				Console.WriteLine("Ready");
				do {
					input = Console.ReadLine();
					sw.WriteLine(input);
					sw.Flush();
				} while (input != "!quit");
			} catch (Exception e) {
				Console.WriteLine("Program has crashed: " + e.ToString());
				Console.ReadKey();
			}
		}
	}
}
