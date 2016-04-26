namespace RequestService
{
    using System;
    using System.Configuration;
    using MassTransit;
    using MassTransit.RabbitMqTransport;
    using Topshelf;
    using Topshelf.Logging;


    class RequestService :
        ServiceControl
    {
        readonly LogWriter _log = HostLogger.Get<RequestService>();

        IBusControl _busControl;

        public bool Start(HostControl hostControl)
        {
            _log.Info("Creating bus...");//creating bus is the same on both the sides 

            _busControl = Bus.Factory.CreateUsingRabbitMq(x =>
            {
                IRabbitMqHost host = x.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
                {
                    h.Username(ConfigurationManager.AppSettings["RabitUser"]);
                    h.Password(ConfigurationManager.AppSettings["RabitPw"]);
                });

                x.ReceiveEndpoint(host, ConfigurationManager.AppSettings["ServiceQueueName"],
                    e => { e.Consumer<RequestConsumer>(); });
            });

            _log.Info("Starting bus...");

            _busControl.Start();

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("Stopping bus...");

            _busControl?.Stop();

            return true;
        }
    }
}