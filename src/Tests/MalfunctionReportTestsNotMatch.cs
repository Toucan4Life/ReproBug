using Repro.Domain;
using Repro.Domain.Communication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using Moq;
using Newtonsoft.Json;
using SAP;
using Microsoft.AspNetCore.Mvc.Testing;
using ReproBug.Utils;

namespace Repro.Integration.Tests
{
    [TestClass]
    public class MalfunctionReportTestsNotMatch
    {

        [TestMethod]
        public async Task TestMalfunctionReportMatch()
        {
            var mockLocation = new Mock<ILocationServiceAgent>();

            mockLocation.Setup(t => t.GetWorkCenterDetailAsync())
                          .Returns("MAC Substations Namur");

            var session = new WebApplicationFactory<Program>().WithWebHostBuilder(c =>
            {
                c.ConfigureTestServices(services =>
                {
                    services.AddTransient(t => mockLocation.Object);
                });
            }).Services.GetService<IMessageSession>();

            var result =
                await EndpointFixture.ExecuteAndWaitForSent<MalfunctionReportUpdatedPublished>(() =>
                    session.Publish(new MalfunctionReportInternalImpl()));

            var message = result.Select(c => c.Message.Instance).OfType<MalfunctionReportUpdatedPublished>().Single();

            Assert.AreEqual("MAC Substations Namur", message.Name);
        }

        [TestMethod]
        public async Task TestMalfunctionReportNotMatch()
        {
            var mockLocation = new Mock<ILocationServiceAgent>();

            mockLocation.Setup(t => t.GetWorkCenterDetailAsync())
                .Returns("NOTAGOODNAME");

            var session = new WebApplicationFactory<Program>().WithWebHostBuilder(c =>
            {
                c.ConfigureTestServices(services =>
                {
                    services.AddTransient(t => mockLocation.Object);
                });
            }).Services.GetService<IMessageSession>();

            var result =
                await EndpointFixture.ExecuteAndWaitForSent<MalfunctionReportUpdatedPublished>(() =>
                    session.Publish(new MalfunctionReportInternalImpl()));

            var message = result.Select(c => c.Message.Instance).OfType<MalfunctionReportUpdatedPublished>().Single();
            Assert.AreEqual("NOTAGOODNAME", message.Name);
        }
    }

    public class MalfunctionReportInternalImpl : MalfunctionReportInternal
    {
    }
}