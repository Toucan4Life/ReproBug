using NServiceBus;

namespace Repro.Domain.Communication 
{
	public interface MalfunctionReportUpdatedPublished : IEvent 
	{
		string Name { get; set; }
	}
}