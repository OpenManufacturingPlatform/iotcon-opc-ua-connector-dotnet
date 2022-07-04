﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Linq;
using FluentValidation;
using NUnit.Framework;
using OMP.Connector.Application.Validators;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Application.Tests.Validators
{
    [TestFixture]
    public class AlarmMonitoredItemValidatorTests
    {
        private const string IntervalNotANumber = " ";
        private const string IntervalLow = "10";
        private const string IntervalMedium = "20";
        private const string IntervalHigh = "30";

        private AlarmSubscriptionMonitoredItem _testItem;
        private AbstractValidator<AlarmSubscriptionMonitoredItem> _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            this._testItem = new AlarmSubscriptionMonitoredItem
            {
                PublishingInterval = IntervalMedium,
                HeartbeatInterval = IntervalHigh,
            };

            this._classUnderTest = new AlarmMonitoredItemValidator();
        }

        [Test]
        public void When_Increasing_Intervals_Correct_Result_Valid()
        {
            var result = this._classUnderTest.Validate(this._testItem);

            Assert.True(result.IsValid);
        }

        [Test]
        public void When_Equal_Intervals_Correct_Result_Valid()
        {
            this._testItem.PublishingInterval = IntervalLow;
            this._testItem.HeartbeatInterval = IntervalLow;

            var result = this._classUnderTest.Validate(this._testItem);

            Assert.True(result.IsValid);
        }

        [Test]
        public void When_PublishingInterval_Missing_Result_Invalid()
        {
            this._testItem.PublishingInterval = string.Empty;

            var result = this._classUnderTest.Validate(this._testItem);

            Assert.False(result.IsValid);
        }

        [Test]
        public void When_PublishingInterval_Missing_Result_Has_ErrorMessage()
        {
            this._testItem.PublishingInterval = string.Empty;

            var result = this._classUnderTest.Validate(this._testItem);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("HeartbeatInterval must be greater than PublishingInterval", errorMessages.Select(v => v.ToString()).ToArray());
        }

        [Test]
        public void When_PublishingInterval_No_Number_Result_Invalid()
        {
            this._testItem.PublishingInterval = IntervalNotANumber;

            var result = this._classUnderTest.Validate(this._testItem);

            Assert.False(result.IsValid);
        }

        [Test]
        public void When_PublishingInterval_No_Number_Result_Has_ErrorMessage()
        {
            this._testItem.PublishingInterval = IntervalNotANumber;

            var result = this._classUnderTest.Validate(this._testItem);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("HeartbeatInterval must be greater than PublishingInterval", errorMessages.Select(v => v.ToString()).ToArray());
        }

        [Test]
        public void When_HeartbeatInterval_Missing_Result_Invalid()
        {
            this._testItem.HeartbeatInterval = string.Empty;

            var result = this._classUnderTest.Validate(this._testItem);

            Assert.False(result.IsValid);
        }

        [Test]
        public void When_HeartbeatInterval_Missing_Result_Has_ErrorMessage()
        {
            this._testItem.HeartbeatInterval = string.Empty;

            var result = this._classUnderTest.Validate(this._testItem);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("HeartbeatInterval must be greater than PublishingInterval", errorMessages.Select(v => v.ToString()).ToArray());
        }

        [Test]
        public void When_HeartbeatInterval_No_Number_Result_Invalid()
        {
            this._testItem.HeartbeatInterval = IntervalNotANumber;

            var result = this._classUnderTest.Validate(this._testItem);

            Assert.False(result.IsValid);
        }

        [Test]
        public void When_HeartbeatInterval_No_Number_Result_Has_ErrorMessage()
        {
            this._testItem.HeartbeatInterval = IntervalNotANumber;

            var result = this._classUnderTest.Validate(this._testItem);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("HeartbeatInterval must be greater than PublishingInterval", errorMessages.Select(v => v.ToString()).ToArray());
        }

        [Test]
        public void When_PublishingI_Greater_HeartbeatI_Result_Invalid()
        {
            this._testItem.PublishingInterval = IntervalHigh;
            this._testItem.HeartbeatInterval = IntervalMedium;

            var result = this._classUnderTest.Validate(this._testItem);

            Assert.False(result.IsValid);
        }

        [Test]
        public void When_PublishingI_Greater_HeartbeatI_Result_Has_ErrorMessage()
        {
            this._testItem.PublishingInterval = IntervalHigh;
            this._testItem.HeartbeatInterval = IntervalMedium;

            var result = this._classUnderTest.Validate(this._testItem);

            var errorMessages = result.Errors;
            Assert.IsNotEmpty(errorMessages);
            Assert.Contains("HeartbeatInterval must be greater than PublishingInterval", errorMessages.Select(v => v.ToString()).ToArray());
        }
    }
}
