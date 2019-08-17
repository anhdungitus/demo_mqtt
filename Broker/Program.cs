using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Broker
{
    internal class Program
    {
        private static IMqttServer _mqttServer;

        private static async Task Main(string[] args)
        {
            // Setup client validator.
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionBacklog(100)
                .WithDefaultEndpointPort(1884)
                .WithConnectionValidator(c =>
                {
                    if (c.ClientId.Length < 10)
                    {
                        c.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                        return;
                    }

                    if (c.Username != "mySecretUser")
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return;
                    }

                    if (c.Password != "mySecretPassword")
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return;
                    }

                    c.ReasonCode = MqttConnectReasonCode.Success;
                })
                .WithStorage(new RetainedMessageHandler())
                .WithPersistentSessions();

            _mqttServer = new MqttFactory().CreateMqttServer();
            await _mqttServer.StartAsync(optionsBuilder.Build());

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
            await _mqttServer.StopAsync();
        }

        // The implementation of the storage:
        // This code uses the JSON library "Newtonsoft.Json".
        public class RetainedMessageHandler : IMqttServerStorage
        {
            private const string Filename = "C:\\MQTT\\RetainedMessages.json";

            public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
            {
                File.WriteAllText(Filename, JsonConvert.SerializeObject(messages));
                return Task.FromResult(0);
            }

            public Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
            {
                IList<MqttApplicationMessage> retainedMessages;
                if (File.Exists(Filename))
                {
                    var json = File.ReadAllText(Filename);
                    retainedMessages = JsonConvert.DeserializeObject<List<MqttApplicationMessage>>(json);
                }
                else
                {
                    retainedMessages = new List<MqttApplicationMessage>();
                }

                return Task.FromResult(retainedMessages);
            }
        }
    }
}