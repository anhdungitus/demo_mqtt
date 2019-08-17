using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading;
using MQTTnet.Protocol;

namespace Publisher
{
    internal class Program
    {
        private static IManagedMqttClient _publisherClient;
        private static IManagedMqttClient _publisherClient2;

        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("clientId-pXiamU1MOP3")
                    .WithTcpServer("103.28.39.18", 1884)
                    .WithCredentials("mySecretUser", "mySecretPassword")
                    .WithCleanSession())
                .Build();

            _publisherClient = new MqttFactory().CreateManagedMqttClient();
            await _publisherClient.StartAsync(options);

            var options2 = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("clientId-pXiamU1MOP36")
                    .WithTcpServer("103.28.39.18", 1884)
                    .WithCredentials("mySecretUser", "mySecretPassword")
                    .WithCleanSession())
                .Build();

            _publisherClient2 = new MqttFactory().CreateManagedMqttClient();
            await _publisherClient2.StartAsync(options2);

            var count = 1;
            while (true)
            {
                Thread.Sleep(500);
                var topic2Message = new MqttApplicationMessageBuilder()
                    .WithTopic("testtopic/2")
                    .WithPayload("Topic2 publish message " + count++)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag()
                    .Build();
                await _publisherClient.PublishAsync(topic2Message);

                //Thread.Sleep(500);
                //var topic1Message = new MqttApplicationMessageBuilder()
                //    .WithTopic("testtopic/2")
                //    .WithPayload("Test multiple client " + count)
                //    .WithExactlyOnceQoS()
                //    .WithRetainFlag()
                //    .Build();
                //await _publisherClient2.PublishAsync(topic1Message);

            }
        }
    }
}