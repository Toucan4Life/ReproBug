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
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Repro.Integration.Tests
{
    //From https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
    public class MalfunctionReportTestsNotMatch(WebApplicationFactory<Program> factory)
        : IClassFixture<WebApplicationFactory<Program>>
    {
        [Fact]
        public async Task TestMalfunctionReportMatch()
        {
            var mockLocation = new Mock<ILocationServiceAgent>();

            mockLocation.Setup(t => t.GetWorkCenterDetailAsync())
                          .Returns("MAC Substations Namur");

            var session = factory.WithWebHostBuilder(c =>
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

        [Fact]
        public async Task TestMalfunctionReportNotMatch()
        {
            var mockLocation = new Mock<ILocationServiceAgent>();

            mockLocation.Setup(t => t.GetWorkCenterDetailAsync())
                .Returns("NOTAGOODNAME");

            var session = factory.WithWebHostBuilder(c =>
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