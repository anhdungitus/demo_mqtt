using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Diagnostics;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading.Tasks;
using MQTTnet.Client;

namespace Subscriber
{
    internal class Program
    {
        private static IMqttClient _subscriberClient;

        private static async Task Main(string[] args)
        {
            var options = new MqttClientOptionsBuilder()
                //.WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                //.WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("clientId-pXiamU1MOP2")
                    .WithTcpServer("103.28.39.18", 1884)
                    .WithCredentials("mySecretUser", "mySecretPassword")
                    .WithCleanSession(false)
                .Build();

            _subscriberClient = new MqttFactory().CreateMqttClient();
            // Register events
            _subscriberClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnMqttClientConnected);
            _subscriberClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnMqttClientDisconnected);
            //_subscriberClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnMqttClientConnectingFailed);
            _subscriberClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnMqttClientApplicationMessageReceived);
            //_subscriberClient.ApplicationMessageSkippedHandler = new ApplicationMessageSkippedHandlerDelegate(ApplicationMessageSkippedHandlerMethod);
            //_subscriberClient.ApplicationMessageProcessedHandler = new ApplicationMessageProcessedHandlerDelegate(ApplicationMessageProcessedHandlerMethod);

            MqttNetGlobalLogger.LogMessagePublished += (s, e) =>
            {
                var trace = $"MqttNetLog >> [{e.TraceMessage.Timestamp:O}] [{e.TraceMessage.ThreadId}] [{e.TraceMessage.Source}] [{e.TraceMessage.Level}]: {e.TraceMessage.Message}";
                if (e.TraceMessage.Exception != null)
                {
                    if (e.TraceMessage.Exception is MqttCommunicationTimedOutException)
                        Console.WriteLine("=========================================");
                    trace += Environment.NewLine + e.TraceMessage.Exception.ToString();
                    Console.WriteLine(trace);
                }
            };

            await _subscriberClient.ConnectAsync(options);
            await _subscriberClient.SubscribeAsync(
                new TopicFilterBuilder().WithTopic("testtopic/1").Build(),
                new TopicFilterBuilder().WithTopic("testtopic/2").Build());
            Console.WriteLine("PRESS ENTER TO FINISH");
            var result = Console.ReadLine();
        }

        private static void ApplicationMessageProcessedHandlerMethod(ApplicationMessageProcessedEventArgs obj)
        {
            Console.WriteLine("ApplicationMessageProcessedHandlerMethod");
        }

        private static void ApplicationMessageSkippedHandlerMethod(ApplicationMessageSkippedEventArgs obj)
        {
            Console.WriteLine("ApplicationMessageSkippedHandlerMethod");
        }

        private static void OnMqttClientApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs obj)
        {
            Console.WriteLine(obj.ApplicationMessage?.ConvertPayloadToString());
        }

        private static void OnMqttClientConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
            Console.WriteLine("OnMqttClientConnectingFailed");
        }

        private static void OnMqttClientDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            Console.WriteLine("OnMqttClientDisconnected");
        }

        private static void OnMqttClientConnected(MqttClientConnectedEventArgs obj)
        {
            Console.WriteLine("OnMqttClientConnected");
        }
    }
}