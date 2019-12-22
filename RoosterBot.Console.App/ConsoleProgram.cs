using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace RoosterBot.ConsoleApp {
	public class ConsoleProgram {
		private static void Main() {
			try {
				using var pipeClient = new NamedPipeClientStream(".", "roosterBotConsolePipe", PipeDirection.InOut, PipeOptions.WriteThrough);
				pipeClient.Connect();
				using var sw = new StreamWriter(pipeClient, Encoding.UTF8, 2047, true);
				using var sr = new StreamReader(pipeClient, Encoding.UTF8, true, 2047, true);

				string input;
				char[] buffer = new char[2047];
				do {
					Console.Write("Input: ");
					input = Console.ReadLine();
					sw.WriteLine(input);
					sw.Flush();
					var response = new StringBuilder();
					int c = 0;
					while ((c = sr.Read()) != '\0') {
						response.Append((char) c);
					}
					Console.WriteLine("Response: " + response.ToString());
					Console.WriteLine("----------");
				} while (input != "!quit");
			} catch (Exception e) {
				Console.WriteLine("Program has crashed: " + e.ToString());
				Console.ReadKey();
			}
		}
	}
}
