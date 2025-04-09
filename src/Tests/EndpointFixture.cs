using System.Diagnostics;
using NServiceBus.Pipeline;

namespace Repro.Integration.Tests
{
    public static class EndpointFixture
    {
        public static Task<IReadOnlyList<IOutgoingLogicalMessageContext>> ExecuteAndWaitForSent<TMessage>(
             Func<Task> testAction) =>
             ExecuteAndWait<IOutgoingLogicalMessageContext>(
                 testAction,
                 m => m.Message.MessageType == typeof(TMessage));

        private static async Task<IReadOnlyList<IOutgoingLogicalMessageContext>> ExecuteAndWait<TMessageContext>(
            Func<Task> testAction,
            Func<TMessageContext, bool> predicate)
            where TMessageContext : class, IPipelineContext
        {
            var outgoingMessageContexts = new List<IOutgoingLogicalMessageContext>();
            var messageReceivingTaskSource = new TaskCompletionSource<object>();

            using ActivityListener listener = new();
            listener.ActivityStopped = activitySource =>
            {
                if (activitySource.OperationName != "NServiceBus.Diagnostics.PublishMessage") return;
                var outgoingContext = activitySource.GetTagItem("testing.outgoing.message.context") as IOutgoingLogicalMessageContext;

                outgoingMessageContexts.Add(outgoingContext);

                if (outgoingContext is TMessageContext ctx2 && predicate(ctx2))
                {
                    messageReceivingTaskSource.SetResult(null);
                }
            };
            listener.ShouldListenTo = _ => true;
            listener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData;

            ActivitySource.AddActivityListener(listener);

            await testAction();

            await messageReceivingTaskSource.Task;

            return outgoingMessageContexts;
        }
    }
}