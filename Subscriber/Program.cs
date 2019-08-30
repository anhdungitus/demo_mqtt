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
using Newtonsoft.Json;

namespace Subscriber
{
    public class StatusPayload
    {
        public string ApplicationId { get; set; } // Wilink | HomeCare | QuoteTool
        public Guid EntityId { get; set; } // Guid---
        public string EntityType { get; set; } // AGE/CON
        public string IdentifyCode { get; set; } // Machine unique id
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{ApplicationId} - {EntityId} - {EntityType} - {IdentifyCode} - {Status}";
        }
    }

    internal class Program
    {
        private static IMqttClient _subscriberClient;
        public const string DeviceChangeStatusToPic = "wilink/device-status";

        private static void Main(string[] args)
        {
            var options = new MqttClientOptionsBuilder()
                    .WithClientId("clientId-pXiamU1MOP2")
                    .WithTcpServer("127.0.0.1", 8000)
                    .WithCredentials("mySecretUser", "mySecretPassword")
                    .WithCleanSession(false)
                .Build();

            _subscriberClient = new MqttFactory().CreateMqttClient();
            _subscriberClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnMqttClientConnected);
            _subscriberClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnMqttClientDisconnected);
            _subscriberClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnMqttClientApplicationMessageReceived);
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

            var mqttClientAuthenticateResult = _subscriberClient.ConnectAsync(options).Result;
            var mqttClientSubscribeResult = _subscriberClient.SubscribeAsync(
                new TopicFilterBuilder().WithTopic("testtopic/1").Build(),
                new TopicFilterBuilder().WithTopic("testtopic/2").Build(),
                new TopicFilterBuilder().WithTopic(DeviceChangeStatusToPic).Build()).Result;
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

        private static Task OnMqttClientApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs obj)
        {
            if (obj.ApplicationMessage.Topic == DeviceChangeStatusToPic)
            {
                var payloadStatus = JsonConvert.DeserializeObject<StatusPayload>(obj.ApplicationMessage?.ConvertPayloadToString());

                Console.WriteLine($"Client {obj.ClientId} change status");
                Console.WriteLine(payloadStatus.ToString());
            }
            else
            {
                Console.WriteLine($"Received message {obj.ApplicationMessage?.ConvertPayloadToString()} from " +
                                  $"client {obj.ClientId}");
            }

            return Task.CompletedTask;
        }

        private static void OnMqttClientConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
            Console.WriteLine("OnMqttClientConnectingFailed");
        }

        private static Task OnMqttClientDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            Console.WriteLine("OnMqttClientDisconnected");
            return Task.CompletedTask;
        }

        private static Task OnMqttClientConnected(MqttClientConnectedEventArgs obj)
        {
            Console.WriteLine("OnMqttClientConnected");
            return Task.CompletedTask;
        }
    }
}