using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OMP.Connector.Application.Repositories;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Application.Tests.Repositories
{
    [TestFixture]
    public class LocalSubscriptionRepositoryTests
    {
        private const string EndpointUrl = "url";
        private const string EndpointUrlDifferent = "url/DifferentEndpoint";
        private const string LowValue = "1";
        private const string MediumValue = "2";
        private const string HighValue = "3";
        private const string NodeIdNodeA = "nodeA";
        private const string NodeIdNodeB = "nodeB";

        private LocalSubscriptionRepository _repository;

        [SetUp]
        public void SetUp()
        {
            this._repository = new LocalSubscriptionRepository();
        }

        [Test]
        public void Should_Update_Items_When_ItemSettings_Change()
        {
            // Arrange
            var monitoredItems = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, new SubscriptionMonitoredItem
                    {
                        NodeId = NodeIdNodeA,
                        SamplingInterval = LowValue,
                        PublishingInterval = MediumValue,
                        HeartbeatInterval = HighValue
                    }
                }
            };
            var monitoredItemsDifferent = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, new SubscriptionMonitoredItem
                    {
                        NodeId = NodeIdNodeA,
                        SamplingInterval = MediumValue,
                        PublishingInterval = MediumValue,
                        HeartbeatInterval = HighValue
                    }
                }
            };

            // Test
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = MediumValue,
                MonitoredItems = monitoredItems
            });
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = MediumValue,
                MonitoredItems = monitoredItemsDifferent
            });

            // Verify
            var actualSubscriptionDto = this._repository.GetAllByEndpointUrl(EndpointUrl);

            var actualSubscriptionList = actualSubscriptionDto.ToList();
            Assert.That(actualSubscriptionList.Count(), Is.EqualTo(1));

            var singleSubscription = actualSubscriptionList.First();
            Assert.That(singleSubscription.MonitoredItems.Keys.Count(), Is.EqualTo(1));

            var singleItem = singleSubscription.MonitoredItems.First();
            Assert.AreEqual(MediumValue, singleItem.Value.SamplingInterval);
        }

        [Test]
        public void Should_Update_Item_When_Same_NodeId_Is_Used()
        {
            // Arrange
            var monitoredItems = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, new SubscriptionMonitoredItem
                    {
                        NodeId = NodeIdNodeA,
                        SamplingInterval = LowValue,
                        PublishingInterval = LowValue,
                        HeartbeatInterval = LowValue
                    }
                }
            };
            var monitoredItemsDifferent = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, new SubscriptionMonitoredItem
                    {
                        NodeId = NodeIdNodeA,
                        SamplingInterval = MediumValue,
                        PublishingInterval = MediumValue,
                        HeartbeatInterval = MediumValue
                    }
                }
            };

            // Test
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItems
            });
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = MediumValue,
                MonitoredItems = monitoredItemsDifferent
            });

            // Verify
            var actualSubscriptionDto = this._repository.GetAllByEndpointUrl(EndpointUrl);

            var actualSubscriptionList = actualSubscriptionDto.ToList();
            Assert.That(actualSubscriptionList.Count(), Is.EqualTo(1));

            var singleSubscription = actualSubscriptionList.First();
            Assert.AreEqual(MediumValue, singleSubscription.PublishingInterval);
        }

        [Test]
        public void Should_Split_Items_When_One_PublishInterval_Changes()
        {
            // Arrange
            var monitoredItems = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, new SubscriptionMonitoredItem
                    {
                        NodeId = NodeIdNodeA,
                        SamplingInterval = LowValue,
                        PublishingInterval = LowValue,
                        HeartbeatInterval = LowValue
                    }
                },
                {
                    NodeIdNodeB, new SubscriptionMonitoredItem
                    {
                        NodeId = NodeIdNodeB,
                        SamplingInterval = LowValue,
                        PublishingInterval = LowValue,
                        HeartbeatInterval = LowValue
                    }
                }
            };
            var monitoredItemsDifferent = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeB, new SubscriptionMonitoredItem
                    {
                        NodeId = NodeIdNodeB,
                        SamplingInterval = LowValue,
                        PublishingInterval = MediumValue,
                        HeartbeatInterval = LowValue
                    }
                }
            };

            // Test
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItems
            });
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = MediumValue,
                MonitoredItems = monitoredItemsDifferent
            });

            // Verify
            var actualSubscriptionDto = this._repository.GetAllByEndpointUrl(EndpointUrl);

            var subscriptionDtos = actualSubscriptionDto.ToList();
            var actualSubscriptionList = subscriptionDtos.ToList();
            Assert.AreEqual(2, actualSubscriptionList.Count);
            Assert.That(actualSubscriptionList.Count(), Is.EqualTo(2));
            Assert.That(subscriptionDtos.Count(dto => dto.PublishingInterval == LowValue), Is.EqualTo(1));
            Assert.That(subscriptionDtos.Count(dto => dto.PublishingInterval == MediumValue), Is.EqualTo(1));
        }

        [Test]
        public void Should_Not_Mix_Subscription_Of_Different_Servers()
        {
            // Arrange
            var itemsNodeA = new SubscriptionMonitoredItem
            {
                NodeId = NodeIdNodeA,
                SamplingInterval = LowValue,
                PublishingInterval = LowValue,
                HeartbeatInterval = LowValue
            };

            var itemsNodeB = new SubscriptionMonitoredItem
            {
                NodeId = NodeIdNodeB,
                SamplingInterval = LowValue,
                PublishingInterval = LowValue,
                HeartbeatInterval = LowValue
            };

            var monitoredItems = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, itemsNodeA
                },
                {
                    NodeIdNodeB, itemsNodeB
                }
            };

            var monitoredItemsDifferent = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, itemsNodeA
                }
            };

            // Test
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItems
            });
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrlDifferent,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItemsDifferent
            });

            // Verify
            var actualSubscriptionDto = this._repository.GetAllSubscriptions();

            var subscriptionDtos = actualSubscriptionDto.ToList();
            var actualSubscriptionList = subscriptionDtos.ToList();
            Assert.AreEqual(2, actualSubscriptionList.Count);

            var subscriptionServerA = subscriptionDtos.Where(dto => dto.EndpointUrl == EndpointUrl);
            var serverASubscriptionList = subscriptionServerA.ToList();
            Assert.That(serverASubscriptionList.Count(), Is.EqualTo(1));
            Assert.AreEqual(2, serverASubscriptionList.First().MonitoredItems.Count);

            var subscriptionServerB = subscriptionDtos.Where(dto => dto.EndpointUrl == EndpointUrlDifferent);
            var serverBSubscriptionList = subscriptionServerB.ToList();
            Assert.That(serverBSubscriptionList.Count(), Is.EqualTo(1));
            Assert.AreEqual(1, serverBSubscriptionList.First().MonitoredItems.Count);
        }

        [Test]
        public void Should_Delete_Single_Item()
        {
            // Arrange
            var itemsNodeA = new SubscriptionMonitoredItem
            {
                NodeId = NodeIdNodeA,
                SamplingInterval = LowValue,
                PublishingInterval = LowValue,
                HeartbeatInterval = LowValue
            };

            var itemsNodeB = new SubscriptionMonitoredItem
            {
                NodeId = NodeIdNodeB,
                SamplingInterval = LowValue,
                PublishingInterval = LowValue,
                HeartbeatInterval = LowValue
            };

            var monitoredItems = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, itemsNodeA
                },
                {
                    NodeIdNodeB, itemsNodeB
                }
            };

            var monitoredItemsDifferent = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, itemsNodeA
                }
            };

            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItems
            });
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrlDifferent,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItemsDifferent
            });

            // Test
            var deleteItems = new List<OpcUaMonitoredItem> { itemsNodeA };
            var actualDeletionSuccessful = this._repository.DeleteMonitoredItems(EndpointUrl, deleteItems);

            // Verify
            Assert.True(actualDeletionSuccessful);
            var actualSubscriptionDto = this._repository.GetAllSubscriptions();

            var subscriptionDtos = actualSubscriptionDto.ToList();
            var actualSubscriptionList = subscriptionDtos.ToList();
            Assert.That(actualSubscriptionList.Count(), Is.EqualTo(2));

            var subscriptionServerA = subscriptionDtos.Where(dto => dto.EndpointUrl == EndpointUrl);
            var serverASubscriptionList = subscriptionServerA.ToList();
            Assert.That(serverASubscriptionList.Count(), Is.EqualTo(1));
            Assert.AreEqual(1, serverASubscriptionList.First().MonitoredItems.Count);
            Assert.AreEqual(itemsNodeB, serverASubscriptionList.First().MonitoredItems.First().Value);

            var subscriptionServerB = subscriptionDtos.Where(dto => dto.EndpointUrl == EndpointUrlDifferent);
            var serverBSubscriptionList = subscriptionServerB.ToList();
            Assert.That(serverBSubscriptionList.Count(), Is.EqualTo(1));
            Assert.AreEqual(1, serverBSubscriptionList.First().MonitoredItems.Count);
        }

        [Test]
        public void Should_Delete_Whole_Subscription_If_No_Item_Left()
        {
            // Arrange
            var itemsNodeA = new SubscriptionMonitoredItem
            {
                NodeId = NodeIdNodeA,
                SamplingInterval = LowValue,
                PublishingInterval = LowValue,
                HeartbeatInterval = LowValue
            };

            var itemsNodeB = new SubscriptionMonitoredItem
            {
                NodeId = NodeIdNodeB,
                SamplingInterval = LowValue,
                PublishingInterval = LowValue,
                HeartbeatInterval = LowValue
            };

            var monitoredItems = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, itemsNodeA
                },
                {
                    NodeIdNodeB, itemsNodeB
                }
            };

            var monitoredItemsDifferent = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, itemsNodeA
                }
            };

            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItems
            });
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrlDifferent,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItemsDifferent
            });

            // Test
            var deleteItems = new List<OpcUaMonitoredItem> { itemsNodeA };
            var actualDeletionSuccessful = this._repository.DeleteMonitoredItems(EndpointUrlDifferent, deleteItems);

            // Verify
            Assert.True(actualDeletionSuccessful);
            var actualSubscriptionDto = this._repository.GetAllSubscriptions();

            var subscriptionDtos = actualSubscriptionDto.ToList();
            var actualSubscriptionList = subscriptionDtos.ToList();
            Assert.That(actualSubscriptionList.Count(), Is.EqualTo(1));

            var subscriptionServerA = subscriptionDtos.Where(dto => dto.EndpointUrl == EndpointUrl);
            var serverASubscriptionList = subscriptionServerA.ToList();
            Assert.That(serverASubscriptionList.Count(), Is.EqualTo(1));
            Assert.AreEqual(2, serverASubscriptionList.First().MonitoredItems.Count);

            var subscriptionServerB = subscriptionDtos.Where(dto => dto.EndpointUrl == EndpointUrlDifferent);
            Assert.IsEmpty(subscriptionServerB);
        }

        [Test]
        public void Should_Delete_Subscription_Identified_By_Endpoint_And_PublishingInterval()
        {
            // Arrange
            var itemsNodeA = new SubscriptionMonitoredItem
            {
                NodeId = NodeIdNodeA,
                SamplingInterval = LowValue,
                PublishingInterval = LowValue,
                HeartbeatInterval = LowValue
            };

            var itemsNodeB = new SubscriptionMonitoredItem
            {
                NodeId = NodeIdNodeB,
                SamplingInterval = LowValue,
                PublishingInterval = LowValue,
                HeartbeatInterval = LowValue
            };

            var monitoredItems = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, itemsNodeA
                },
                {
                    NodeIdNodeB, itemsNodeB
                }
            };

            var monitoredItemsDifferent = new Dictionary<string, SubscriptionMonitoredItem>
            {
                {
                    NodeIdNodeA, itemsNodeA
                }
            };

            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItems
            });
            this._repository.Add(new SubscriptionDto
            {
                EndpointUrl = EndpointUrlDifferent,
                PublishingInterval = LowValue,
                MonitoredItems = monitoredItemsDifferent
            });

            // Test
            var deleteSubscription = new SubscriptionDto
            {
                EndpointUrl = EndpointUrl,
                PublishingInterval = LowValue
            };
            var actualDeletionSuccessful = this._repository.Remove(deleteSubscription);

            // Verify
            Assert.True(actualDeletionSuccessful);
            var actualSubscriptionDto = this._repository.GetAllSubscriptions();

            var subscriptionDtos = actualSubscriptionDto.ToList();
            var actualSubscriptionList = subscriptionDtos.ToList();
            Assert.That(actualSubscriptionList.Count(), Is.EqualTo(1));

            var subscriptionServerA = subscriptionDtos.Where(dto => dto.EndpointUrl == EndpointUrl);
            Assert.IsEmpty(subscriptionServerA);

            var subscriptionServerB = subscriptionDtos.Where(dto => dto.EndpointUrl == EndpointUrlDifferent);
            Assert.That(subscriptionServerB.Count(), Is.EqualTo(1));
        }
    }
}