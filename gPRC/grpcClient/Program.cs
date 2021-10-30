using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using gPRC;

using Grpc.Core;
using Grpc.Net.Client;

using NAudio.Wave;

namespace grpcClient
{
    internal class Program
    {
        private const int SampleBits = 16;
        private const int SampleRate = 16000;

        static async Task Main(string[] args)
        {
            Console.WriteLine($"App started..");
            var url = "http://localhost:5000";

            using var channel = GrpcChannel.ForAddress(url);
            using var call = new Greeter.GreeterClient(channel).ChangeVoice();
            var voiceData = new ConcurrentQueue<byte[]>();

            Console.WriteLine("Starting background task to receive messages");
            var readTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var response in call.ResponseStream.ReadAllAsync())
                    {
                        voiceData.Enqueue(response.AudioSample.Data.ToByteArray());
                        Console.WriteLine($"Приял пачку данных");
                    }

                    var writer = new WaveFileWriter("D:\\Data\\output.wav", new WaveFormat(SampleRate, SampleBits, 1));
                    var i = 0;
                    foreach (var item in voiceData)
                    {
                        writer.Write(item, 0, item.Length);
                        i++;
                    }
                    writer.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            Console.WriteLine("Starting to send messages");
            Console.WriteLine("Type a message to echo then press enter.");

            var waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(SampleRate, SampleBits, 1)
            };

            var availableDevice = WaveIn.GetCapabilities(waveIn.DeviceNumber);
            Console.WriteLine($"availableDevice = {availableDevice.ProductName}");

            waveIn.DataAvailable += async (s, a) =>
            {
                try
                {
                    Console.WriteLine($"BytesRecorded = {a.BytesRecorded}");

                    var request = new VoiceRequest
                    {
                        AudioSample = GetSample(a.Buffer)
                    };

                    Console.WriteLine("Before RequestStream.WriteAsync");
                    await call.RequestStream.WriteAsync(request);
                    Console.WriteLine("After RequestStream.WriteAsync");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };

            waveIn.RecordingStopped += async (s, e) =>
            {
                Console.WriteLine("Before RequestStream.WriteAsync");
                await call.RequestStream.CompleteAsync();
                Console.WriteLine("After RequestStream.WriteAsync");
            };

            waveIn.StartRecording();
            await Task.Delay(TimeSpan.FromSeconds(5));
            waveIn.StopRecording();

            //Console.WriteLine("Before RequestStream.WriteAsync");
            //await call.RequestStream.CompleteAsync();
            //Console.WriteLine("After RequestStream.WriteAsync");
            await readTask;

            using (var audioFile = new AudioFileReader("D:\\Data\\output.wav"))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }

            Console.ReadKey();
        }

        private static AudioSample GetSample(byte[] audioBytes)
        {
            return new AudioSample
            {
                Data = Google.Protobuf.ByteString.CopyFrom(audioBytes),
                Meta = new AudioMeta
                {
                    Encoding = AudioMeta.Types.Encoding.PcmSigned,
                    SampleBits = SampleBits,
                    SampleRate = SampleRate
                }
            };
        }
    }
}
