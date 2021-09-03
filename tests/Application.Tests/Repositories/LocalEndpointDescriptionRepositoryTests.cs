using NUnit.Framework;
using OMP.Connector.Application.Repositories;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Application.Tests.Repositories
{
    [TestFixture]
    public class LocalEndpointDescriptionRepositoryTests
    {
        private const string EndpointUrl = "url";
        private const string EndpointUrlDifferent = "url/DifferentEndpoint";

        private readonly LocalEndpointDescriptionRepository _repository;

        public LocalEndpointDescriptionRepositoryTests()
        {
            this._repository = new LocalEndpointDescriptionRepository();
        }

        [Test]
        public void Should_Update_Details_When_Same_Url_Used()
        {
            // Arrange
            var initialDetails = new ServerDetails
            {
                Name = "initialName",
                Route = "initialRoute"
            };
            var updateDetails = new ServerDetails
            {
                Name = "initialName",
                Route = "updatedRoute"
            };
            this._repository.Add(new EndpointDescriptionDto
            {
                EndpointUrl = EndpointUrl,
                ServerDetails = initialDetails
            });
            this._repository.Add(new EndpointDescriptionDto
            {
                EndpointUrl = EndpointUrl,
                ServerDetails = updateDetails
            });

            // Test
            var actualEndpointDescriptionDto = this._repository.GetByEndpointUrl(EndpointUrl);

            // Verify
            Assert.AreEqual(updateDetails, actualEndpointDescriptionDto.ServerDetails);
        }

        [Test]
        public void Should_Preserve_Details_When_Different_Urls_Used()
        {
            // Arrange
            var serverDetails = new ServerDetails
            {
                Name = "name",
                Route = "route"
            };
            var serverDetailsDifferent = new ServerDetails
            {
                Name = "DifferentName",
                Route = "DifferentRoute"
            };
            this._repository.Add(new EndpointDescriptionDto
            {
                EndpointUrl = EndpointUrl,
                ServerDetails = serverDetails
            });
            this._repository.Add(new EndpointDescriptionDto
            {
                EndpointUrl = EndpointUrlDifferent,
                ServerDetails = serverDetailsDifferent
            });
            // Test
            var actualEndpointDescription = this._repository.GetByEndpointUrl(EndpointUrl);
            var actualEndpointDescriptionDifferent = this._repository.GetByEndpointUrl(EndpointUrlDifferent);

            // Verify
            Assert.AreEqual(serverDetails, actualEndpointDescription.ServerDetails);
            Assert.AreEqual(serverDetailsDifferent, actualEndpointDescriptionDifferent.ServerDetails);
        }
    }
}