
namespace CarrotMessenger.Api
{
    public class ListenerBackgroundService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service Started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service Stopped");
            return Task.CompletedTask;
        }
    }
}
