using MQTTnet;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Publishing;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace Publisher
{
    public class StatusPayload
    {
        public string ApplicationId { get; set; } // Wilink | HomeCare | QuoteTool
        public Guid EntityId { get; set; } // Guid---
        public string EntityType { get; set; } // AGE/CON
        public string IdentifyCode { get; set; } // Machine unique id
        public string Status { get; set; }
    }

    internal class Program
    {
        private static IMqttClient _publisherClient;
        public const string DeviceChangeStatusToPic = "wilink/device-status";

        private static void Main(string[] args)
        {
            var turnOffPayLoad = new StatusPayload
            {
                ApplicationId = "wilink",
                EntityId = Guid.NewGuid(),
                EntityType = "AGE",
                IdentifyCode = "ec3cc6d0-829a-11e9-bc42-526af7764f64",
                Status = "OFF"
            };
            var turnOffPayloadJson = JsonConvert.SerializeObject(turnOffPayLoad);
            var lastWillMessage = new MqttApplicationMessage
            {
                Topic = DeviceChangeStatusToPic,
                Payload = Encoding.UTF8.GetBytes(turnOffPayloadJson),
                QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce,
                Retain = true
            };

            var options = new MqttClientOptionsBuilder()
                    .WithClientId("clientId-pXiamU1MOP33")
                    .WithTcpServer("127.0.0.1", 8000)
                    .WithCredentials("mySecretUser", "mySecretPassword")
                    .WithCleanSession()
                    .WithWillMessage(lastWillMessage)
                    .WithRequestResponseInformation()
                .Build();

            _publisherClient = new MqttFactory().CreateMqttClient();
            _publisherClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(ConnectedHandler);
            //_publisherClient.UseDisconnectedHandler(Handler)
            //_publisherClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(DisconnectedHandler);
            var mqttClientAuthenticateResult = _publisherClient.ConnectAsync(options).Result;
            //var turnOnPayloadJson = JsonConvert.SerializeObject(new StatusPayload()
            //{
            //    ApplicationId = "wilink",
            //    EntityId = Guid.NewGuid(),
            //    EntityType = "AGE",
            //    IdentifyCode = "ec3cc6d0-829a-11e9-bc42-526af7764f64",
            //    Status = "ON"
            //});

            //var connectMessage = new MqttApplicationMessageBuilder()
            //    .WithTopic(DeviceChangeStatusToPic)
            //    .WithPayload(turnOnPayloadJson)
            //    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
            //    .WithRetainFlag()
            //    .Build();
            //await _publisherClient.PublishAsync(connectMessage);

            while (true)
            {
                Console.WriteLine("Press 0 to exit, 1 to connect again, type message to publish.");
                var command = Console.ReadLine();

                switch (command)
                {
                    case "0":
                    {
                        var connectMessage = new MqttApplicationMessageBuilder()
                            .WithTopic(DeviceChangeStatusToPic)
                            .WithPayload(turnOffPayloadJson)
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                            .WithRetainFlag()
                            .Build();
                         _publisherClient.PublishAsync(connectMessage).ContinueWith(s => _publisherClient.DisconnectAsync());
                    }
                        break;
                    case "1":
                    {
                        if (!_publisherClient.IsConnected)
                             _publisherClient.ConnectAsync(options);
                        break;
                    }
                    default:
                    {
                        if (_publisherClient.IsConnected)
                        {
                            var topic2Message = new MqttApplicationMessageBuilder()
                                .WithTopic("testtopic/2")
                                .WithPayload(command)
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                                .WithRetainFlag(false)
                                .Build();
                            var result = _publisherClient.PublishAsync(topic2Message);
                            if (result.IsCompleted)
                            {
                                Console.WriteLine("Publish success!");
                            }
                        }
                        break;
                    }
                }
            }
        }

        private static async Task DisconnectedHandler(MqttClientDisconnectedEventArgs obj)
        {
            
        }

        private static async Task ConnectedHandler(MqttClientConnectedEventArgs obj)
        {
            var turnOnPayloadJson = JsonConvert.SerializeObject(new StatusPayload()
            {
                ApplicationId = "wilink",
                EntityId = Guid.NewGuid(),
                EntityType = "AGE",
                IdentifyCode = "ec3cc6d0-829a-11e9-bc42-526af7764f64",
                Status = "ON"
            });

            var connectMessage = new MqttApplicationMessageBuilder()
                .WithTopic(DeviceChangeStatusToPic)
                .WithPayload(turnOnPayloadJson)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .WithRetainFlag()
                .Build();
            await _publisherClient.PublishAsync(connectMessage);
        }
    }
}