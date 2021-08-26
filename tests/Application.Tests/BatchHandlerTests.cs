using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using OMP.Connector.Tests.Support;

namespace OMP.Connector.Application.Tests
{
    [TestFixture]
    public class BatchHandlerTests
    {
        [TestCase(10, 0)]
        [TestCase(10, 5)]
        [TestCase(10, 10)]
        [TestCase(10, 13)]
        [TestCase(50, 30000)]
        public void Should_Correctly_Return_Expected_Result(int batchSize, int numberOfItems)
        {
            // Arrange
            var expectedBatchTotal = BatchSupport.CalculateBatches(batchSize, numberOfItems);
            var inputCollection = GenerateDummyData(numberOfItems).ToList();
            var outputCollection = new List<int>();

            void batchAction(int[] items) { outputCollection.AddRange(items); }
            var batchHandler = new BatchHandler<int>(batchSize, batchAction);

            // Test
            batchHandler.RunBatches(inputCollection);

            // Verify
            Assert.AreEqual(inputCollection, outputCollection);
        }

        [TestCase(10, 0)]
        [TestCase(10, 5)]
        [TestCase(10, 10)]
        [TestCase(10, 13)]
        [TestCase(50, 30000)]
        public void Should_Correctly_Run_Expected_Number_Of_Batches(int batchSize, int numberOfItems)
        {
            // Arrange
            var expectedBatches = BatchSupport.CalculateBatches(batchSize, numberOfItems);
            var inputCollection = GenerateDummyData(numberOfItems).ToList();
            var outputCollection = Substitute.For<TestList<int>>();
            outputCollection.CustomAddRange(default).ReturnsForAnyArgs(CallCustomAddRange());

            var actualBatches = new List<int>();

            void batchAction(int[] items) { actualBatches.Add(outputCollection.CustomAddRange(items)); }
            var batchHandler = new BatchHandler<int>(batchSize, batchAction);

            // Test
            batchHandler.RunBatches(inputCollection);

            // Verify
            outputCollection.ReceivedWithAnyArgs(expectedBatches.Count).CustomAddRange(default);
            actualBatches.Should().BeEquivalentTo(expectedBatches);
        }

        private IEnumerable<int> GenerateDummyData(int size)
        {
            var random = new Random();
            for (int i = 1; i <= size; i++)
            {
                yield return random.Next();
            }
        }

        private static Func<CallInfo, int> CallCustomAddRange()
        {
            return args =>
            {
                var inputCollection = (IEnumerable<int>)args[0];
                return inputCollection.Count();
            };
        }
    }

    public class TestObject<T>
    {
        public virtual TestList<T> List { get; set; }
        public virtual int AddRangeToList(TestList<T> list)
        {
            return List.CustomAddRange(list);
        }
    }

    public class TestList<T> : List<T>
    {
        public virtual int CustomAddRange(IEnumerable<T> items)
        {
            AddRange(items);
            return items.Count();
        }
    }
}