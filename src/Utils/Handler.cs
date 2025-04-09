using System;
using System.Threading.Tasks;
using Repro.Domain.Communication;
using NServiceBus;
using ReproBug.Utils;

namespace Repro.Domain
{
    public class Handler(ILocationServiceAgent locationServiceAgent) :
        IHandleMessages<SAP.MalfunctionReportInternal>
    {
        public async Task Handle(SAP.MalfunctionReportInternal message, IMessageHandlerContext context)
        {
            await context.Publish<MalfunctionReportUpdatedPublished>(e =>
            {
                e.Name = locationServiceAgent.GetWorkCenterDetailAsync();
            });
        }
    }
}
