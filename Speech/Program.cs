using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Speech
{
	class Program
	{
		private static SpeechConfig CurrentSpeechConfig = SpeechConfig.FromSubscription("f6cxyz", "australiaeast");

		static async Task Main(string[] args)
		{
			foreach(string path in Directory.EnumerateFiles(@"C:\temp\", "*.wav"))
			{
				string name = Path.GetFileName(path);

				string[] text = await RecogniseAsync(path);

				Console.WriteLine($"---- File {name} ----");
				Console.WriteLine(string.Join('\n', text));
				Console.WriteLine($"----------------------\n");
			}
			Console.ReadLine();
		}

		private static async Task<string[]> RecogniseAsync(string filename)
		{
			using (AudioConfig ac = AudioConfig.FromWavFileInput(filename))
			using (SpeechRecognizer sr = new SpeechRecognizer(CurrentSpeechConfig, ac))
			{
				TaskCompletionSource<string[]> tcs = new TaskCompletionSource<string[]>();
				ConcurrentQueue<string> result = new ConcurrentQueue<string>();
				sr.Recognized += (object sender, SpeechRecognitionEventArgs recognized) => result.Enqueue(recognized.Result.Text);
				sr.Canceled += (object sender, SpeechRecognitionCanceledEventArgs done) =>
				{
					if (done.Reason != CancellationReason.EndOfStream)
						tcs.SetException(new Exception(done.Reason + ": " + done.ErrorDetails));
					else
						tcs.SetResult(result.ToArray());
				};
				await sr.StartContinuousRecognitionAsync();
				return await tcs.Task;
			}
		}
	}
}
