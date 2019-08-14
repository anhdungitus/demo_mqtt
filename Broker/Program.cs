using System;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
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
                        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedIdentifierRejected;
                        return;
                    }

                    if (c.Username != "mySecretUser")
                    {
                        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                        return;
                    }

                    if (c.Password != "mySecretPassword")
                    {
                        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                        return;
                    }

                    c.ReturnCode = MqttConnectReturnCode.ConnectionAccepted;
                });

            var options = new MqttServerOptions();
            options.DefaultEndpointOptions.Port = 1884;
            options.DefaultEndpointOptions.ConnectionBacklog = 100;
            options.DefaultEndpointOptions.BoundInterNetworkAddress = System.Net.IPAddress.Parse("127.0.0.1");

            _mqttServer = new MqttFactory().CreateMqttServer();
            await _mqttServer.StartAsync(optionsBuilder.Build());

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
            await _mqttServer.StopAsync();
        }
    }
}