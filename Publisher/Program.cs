using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading;

namespace Publisher
{
    internal class Program
    {
        private static IManagedMqttClient _publisherClient;

        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("clientId-pXiamU1MOP3")
                    .WithTcpServer("127.0.0.1", 1884)
                    .WithCredentials("mySecretUser", "mySecretPassword")
                    .WithCleanSession())
                .Build();

            _publisherClient = new MqttFactory().CreateManagedMqttClient();
            await _publisherClient.StartAsync(options);

            var count = 1;
            while (true)
            {
                Thread.Sleep(500);
                var topic2Message = new MqttApplicationMessageBuilder()
                    .WithTopic("testtopic/2")
                    .WithPayload("Topic2 publish message " + count++)
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();
                await _publisherClient.PublishAsync(topic2Message);
            }
        }
    }
}