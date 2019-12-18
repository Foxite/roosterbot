using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace RoosterBot.ConsoleApp {
	public class ConsoleProgram {
		private static void Main() {
			try {
				using var pipeClient = new NamedPipeClientStream(".", "roosterBotConsolePipe", PipeDirection.InOut);
				using var sw = new StreamWriter(pipeClient, Encoding.UTF8, 2047, true);
				using var sr = new StreamReader(pipeClient, Encoding.UTF8, true, 2047, true);
				pipeClient.Connect();

				string input;
				var buffer = new char[2047].AsSpan();
				do {
					Console.Write("Input: ");
					input = Console.ReadLine();
					sw.Write(input);
					sr.Read(buffer);
					Console.WriteLine(buffer.ToString());
					Console.WriteLine("----------");
				} while (input != "!quit");
			} catch (Exception e) {
				Console.WriteLine("Program has crashed: " + e.ToString());
				Console.ReadKey();
			}
		}
	}
}
