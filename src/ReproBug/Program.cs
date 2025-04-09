using Microsoft.AspNetCore.Builder;
using NServiceBus;
using NServiceBus.Pipeline;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using NServiceBus.Features;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseNServiceBus(_ => {
    var endpointConfiguration = new EndpointConfiguration("ReproBug");
    endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
    endpointConfiguration.EnableInstallers();
    endpointConfiguration.DisableFeature<Audit>();
    endpointConfiguration.EnableOpenTelemetry();
    endpointConfiguration.Pipeline.Register(
        new AttachOutgoingLogicalMessageContextToActivity(),
        "Attach Outgoing Logical Message Context as OpenTelemetry tags");
    // RMQ transport configuration
    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
    transport.UseConventionalRoutingTopology(QueueType.Classic);
    transport.ConnectionString("host=rabbitmq;virtualhost=demo;username=guest;password=guest");
    return endpointConfiguration;
});

var app = builder.Build();
app.Run();

public partial class Program { }
internal class AttachOutgoingLogicalMessageContextToActivity : Behavior<IOutgoingLogicalMessageContext>
{
    public override Task Invoke(
        IOutgoingLogicalMessageContext context,
        Func<Task> next
    )
    {
        Activity.Current?.AddTag("testing.outgoing.message.context", context);
        return next();
    }
}