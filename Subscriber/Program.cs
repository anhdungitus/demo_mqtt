using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading.Tasks;

namespace Subscriber
{
    internal class Program
    {
        private static IManagedMqttClient _subscriberClient;

        private static async Task Main(string[] args)
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("clientId-pXiamU1MOP2")
                    .WithTcpServer("127.0.0.1", 1884)
                    .WithCredentials("mySecretUser", "mySecretPassword")
                    .WithCleanSession())
                .Build();

            _subscriberClient = new MqttFactory().CreateManagedMqttClient();
            _subscriberClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(s => Console.WriteLine(s.ApplicationMessage?.ConvertPayloadToString()));

            await _subscriberClient.SubscribeAsync(
                new TopicFilterBuilder().WithTopic("testtopic/1").Build(),
                new TopicFilterBuilder().WithTopic("testtopic/2").Build());
            await _subscriberClient.StartAsync(options);
            var result = Console.ReadLine();
        }
    }
}