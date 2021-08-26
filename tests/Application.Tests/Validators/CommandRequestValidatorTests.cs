using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using NSubstitute;
using OMP.Connector.Application.Validators;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.MetaData.Message;
using NUnit.Framework;

namespace OMP.Connector.Application.Tests.Validators
{
    [TestFixture]
    public class CommandRequestValidatorTests
    {
        private const string MetaDataFieldName = "MetaData";
        private const string DestinationIdentifiersFieldName = "DestinationIdentifiers";
        private const string ParticipantFieldName = nameof(Participant);
        private const string IdFieldName = nameof(Participant.Id);

        private IOptions<ConnectorConfiguration> _settingsMock;
        private CommandRequestValidator _classUnderTest;

        [SetUp]
        public void Setup()
        {
            this._settingsMock = Substitute.For<IOptions<ConnectorConfiguration>>();
            this._settingsMock.Value.Returns(_ => new ConnectorConfiguration
            {
                ConnectorId = "myAppId"                
            });

            this._classUnderTest = new CommandRequestValidator(this._settingsMock);
        }

        [Test]
        public void When_AppId_Correct_Result_Is_Valid()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData
                {
                    DestinationIdentifiers = new List<Participant> { new Participant { Id = "myAppId" } }
                }
            };

            var result = this._classUnderTest.Validate(message);

            Assert.True(result.IsValid, $"Request with correct {IdFieldName} has to be valid");
        }

        [Test]
        public void When_No_MetaData_Result_Is_Invalid()
        {
            var message = new CommandRequest();

            var result = this._classUnderTest.Validate(message);

            Assert.False(result.IsValid, $"Request without {MetaDataFieldName} has to be invalid");
        }

        [Test]
        public void When_No_MetaData_Result_Has_ErrorMessage()
        {
            var message = new CommandRequest();

            var result = this._classUnderTest.Validate(message);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("MetaData is missing", errorMessages.Select(v => v.ToString()).ToArray());
        }

        [Test]
        public void When_No_DestinationIdentifiers_Result_Is_Invalid()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData()
            };

            var result = this._classUnderTest.Validate(message);

            Assert.False(result.IsValid, $"Request without {DestinationIdentifiersFieldName} has to be invalid");
        }

        [Test]
        public void When_No_DestinationIdentifiers_Result_Has_ErrorMessage()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData()
            };

            var result = this._classUnderTest.Validate(message);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("DestinationIdentifiers is missing", errorMessages.Select(v => v.ToString()).ToArray());
        }

        [Test]
        public void When_No_Participant_Result_Is_Invalid()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData
                {
                    DestinationIdentifiers = new List<Participant>()
                }
            };

            var result = this._classUnderTest.Validate(message);

            Assert.False(result.IsValid, $"Request with missing {ParticipantFieldName} has to be invalid");
        }

        [Test]
        public void When_No_Participant_Result_Has_ErrorMessage()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData
                {
                    DestinationIdentifiers = new List<Participant>()
                }
            };

            var result = this._classUnderTest.Validate(message);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("DestinationIdentifiers has to contain at least one Participant", errorMessages.Select(v => v.ToString()).ToArray());
        }

        [Test]
        public void When_No_AppId_Result_Is_Invalid()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData
                {
                    DestinationIdentifiers = new List<Participant> { new Participant() }
                }
            };

            var result = this._classUnderTest.Validate(message);

            Assert.False(result.IsValid, $"Request with missing {IdFieldName} has to be invalid");
        }

        [Test]
        public void When_No_AppId_Result_Has_ErrorMessage()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData()
            };

            var result = this._classUnderTest.Validate(message);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("DestinationIdentifiers is missing", errorMessages.Select(v => v.ToString()).ToArray());
        }

        [Test]
        public void When_AppId_Incorrect_Result_Is_Invalid()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData
                {
                    DestinationIdentifiers = new List<Participant> { new Participant { Id = "NotMyAppId" } }
                }
            };

            var result = this._classUnderTest.Validate(message);

            Assert.False(result.IsValid, $"Request with incorrect {IdFieldName} has to be invalid");
        }

        [Test]
        public void When_AppId_Incorrect_Result_Has_ErrorMessage()
        {
            var message = new CommandRequest
            {
                MetaData = new MessageMetaData
                {
                    DestinationIdentifiers = new List<Participant> { new Participant { Id = "NotMyAppId" } }
                }
            };

            var result = this._classUnderTest.Validate(message);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains($"{IdFieldName} is invalid in DestinationIdentifiers", errorMessages.Select(v => v.ToString()).ToArray());
        }
    }
}