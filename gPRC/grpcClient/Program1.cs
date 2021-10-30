//using System;
//using System.Threading;
//using System.Threading.Tasks;

//using gPRC;

//using Grpc.Core;
//using Grpc.Net.Client;

//using NAudio.Wave;

//namespace grpcClient
//{
//    internal class Program1
//    {
//        private const int SampleBits = 16;
//        private const int SampleRate = 16000;

//        static async Task Main1(string[] args)
//        {
//            Console.WriteLine($"App started..");
//            var url = "http://localhost:5000";
//            Console.WriteLine("StreamProcessing");

//            var waveIn = new WaveInEvent
//            {
//                WaveFormat = new WaveFormat(SampleRate, SampleBits, 1)
//            };


//            var availableDevice = WaveIn.GetCapabilities(waveIn.DeviceNumber);
//            Console.WriteLine($"availableDevice = {availableDevice.ProductName}");

//            waveIn.RecordingStopped += (s, e) =>
//            {
//                Console.WriteLine($"Закончил записывать голос ...");
//                // await client.RequestStream.CompleteAsync();
//            };

//            var writer = new WaveFileWriter("D:\\Data\\output.wav", waveIn.WaveFormat);

//            waveIn.DataAvailable += async (s, a) =>
//            {
//                try
//                {
//                    using var channel = GrpcChannel.ForAddress(url);
//                    using var client = new Greeter.GreeterClient(channel).ChangeVoice();

//                    Console.WriteLine($"BytesRecorded = {a.BytesRecorded}");
//                    Console.WriteLine($"Buffer.Length = {a.Buffer.Length}");

//                    var request = new VoiceRequest
//                    {
//                        AudioSample = GetSample(a.Buffer)
//                    };

//                    Console.WriteLine("Before RequestStream.WriteAsync");
//                    await client.RequestStream.WriteAsync(request);
//                    Console.WriteLine("After RequestStream.WriteAsync");

//                    Console.WriteLine("Before RequestStream.WriteAsync");
//                    await client.RequestStream.CompleteAsync();
//                    Console.WriteLine("After RequestStream.WriteAsync");

//                    while (await client.ResponseStream.MoveNext())
//                    {
//                        var response = client.ResponseStream.Current;
                       
//                        writer.Write(response.AudioSample.Data.ToByteArray(),0, response.AudioSample.Data.Length);

//                        Console.WriteLine($"Приял пачку данных");
//                        Console.WriteLine($"{response.AudioSample.Data.Length}");
//                    }

//                    writer.Close();
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                }
//            };

//            try
//            {
//                waveIn.StartRecording();
//                await Task.Delay(TimeSpan.FromSeconds(5));
//                waveIn.StopRecording();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }

//            Console.ReadKey();

//          //  writer.Close();
//            using (var audioFile = new AudioFileReader("D:\\Data\\output.wav"))
//            using (var outputDevice = new WaveOutEvent())
//            {
//                outputDevice.Init(audioFile);
//                outputDevice.Play();
//                while (outputDevice.PlaybackState == PlaybackState.Playing)
//                {
//                    Thread.Sleep(1000);
//                }
//            }

//            Console.ReadKey();
//        }

//        private static AudioSample GetSample(byte[] audioBytes)
//        {
//            return new AudioSample
//            {
//                Data = Google.Protobuf.ByteString.CopyFrom(audioBytes),
//                Meta = new AudioMeta
//                {
//                    Encoding = AudioMeta.Types.Encoding.PcmSigned,
//                    SampleBits = SampleBits,
//                    SampleRate = SampleRate
//                }
//            };
//        }
//    }
//}
