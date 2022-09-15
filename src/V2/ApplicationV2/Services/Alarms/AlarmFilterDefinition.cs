/* ========================================================================
 * Copyright (c) 2005-2019 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUA.Services.Alarms
{
    public class AlarmFilterDefinition
    {
        public NodeId AreaId { get; set; }
        public EventSeverity Severity { get; set; }
        public IList<NodeId> EventTypes { get; set; }
        public bool IgnoreSuppressedOrShelved { get; set; }
        public SimpleAttributeOperandCollection SelectClauses { get; set; }

        #region [Public]
        public MonitoredItem CreateMonitoredItem(Session session)
        {
            // create the item with the filter.
            var monitoredItem = new MonitoredItem();

            SetMonitoredItemProperties(monitoredItem, session);

            return monitoredItem;
        }

        public void UpdateMonitoredItem(MonitoredItem existingMonitoredItem, Session session)
        {
            SetMonitoredItemProperties(existingMonitoredItem, session);
        }

        private void SetMonitoredItemProperties(MonitoredItem existingMonitoredItem, Session session)
        {
            if (AreaId == null)
            {
                // default area if none specified
                AreaId = ObjectIds.Server;
            }

            existingMonitoredItem.DisplayName = null;
            existingMonitoredItem.StartNodeId = AreaId;
            existingMonitoredItem.RelativePath = null;
            existingMonitoredItem.NodeClass = NodeClass.Object;
            existingMonitoredItem.AttributeId = Attributes.EventNotifier;
            existingMonitoredItem.IndexRange = null;
            existingMonitoredItem.Encoding = null;
            existingMonitoredItem.MonitoringMode = MonitoringMode.Reporting;
            existingMonitoredItem.SamplingInterval = 0;
            existingMonitoredItem.QueueSize = uint.MaxValue;
            existingMonitoredItem.DiscardOldest = true;
            existingMonitoredItem.Filter = ConstructFilter(session);

            existingMonitoredItem.Handle = this;
        }

        /// <summary>
        /// Constructs the select clauses for a set of event types.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="eventTypeIds">The event type ids.</param>
        /// <returns>The select clauses for all fields discovered.</returns>
        /// <remarks>
        /// Each event type is an ObjectType in the address space. The fields supported by the
        /// server are defined as children of the ObjectType. Many of the fields are manadatory
        /// and are defined by the UA information model, however, indiviudual servers many not 
        /// support all of the optional fields.
        /// 
        /// This method browses the type model and 
        /// </remarks>
        public SimpleAttributeOperandCollection ConstructSelectClauses(
            Session session,
            params NodeId[] eventTypeIds)
        {
            // browse the type model in the server address space to find the fields available for the event type.
            var selectClauses = new SimpleAttributeOperandCollection();

            // must always request the NodeId for the condition instances.
            // this can be done by specifying an operand with an empty browse path.
            var operand = new SimpleAttributeOperand();

            operand.TypeDefinitionId = ObjectTypeIds.BaseEventType;
            operand.AttributeId = Attributes.NodeId;
            operand.BrowsePath = new QualifiedNameCollection();

            selectClauses.Add(operand);

            // add the fields for the selected EventTypes.
            if (eventTypeIds != null)
            {
                for (var ii = 0; ii < eventTypeIds.Length; ii++)
                {
                    CollectFields(session, eventTypeIds[ii], selectClauses);
                }
            }

            // use BaseEventType as the default if no EventTypes specified.
            else
            {
                CollectFields(session, ObjectTypeIds.BaseEventType, selectClauses);
            }

            return selectClauses;
        }

        /// <summary>
        /// Constructs the event filter for the subscription.
        /// </summary>
        /// <returns>The event filter.</returns>
        public EventFilter ConstructFilter(Session session)
        {
            var filter = new EventFilter();

            // the select clauses specify the values returned with each event notification.
            filter.SelectClauses = SelectClauses;

            // the where clause restricts the events returned by the server.
            // it works a lot like the WHERE clause in a SQL statement and supports
            // arbitrary expession trees where the operands are literals or event fields.
            var whereClause = new ContentFilter();

            // the code below constructs a filter that looks like this:
            // (Severity >= X OR LastSeverity >= X) AND (SuppressedOrShelved == False) AND (OfType(A) OR OfType(B))

            // add the severity.
            ContentFilterElement element1 = null;
            ContentFilterElement element2;

            if (Severity > EventSeverity.Min)
            {
                // select the Severity property of the event.
                var operand1 = new SimpleAttributeOperand();
                operand1.TypeDefinitionId = ObjectTypeIds.BaseEventType;
                operand1.BrowsePath.Add(BrowseNames.Severity);
                operand1.AttributeId = Attributes.Value;

                // specify the value to compare the Severity property with.
                var operand2 = new LiteralOperand();
                operand2.Value = new Variant((ushort)Severity);

                // specify that the Severity property must be GreaterThanOrEqual the value specified.
                element1 = whereClause.Push(FilterOperator.GreaterThanOrEqual, operand1, operand2);
            }

            // add the suppressed or shelved.
            if (!IgnoreSuppressedOrShelved)
            {
                // select the SuppressedOrShelved property of the event.
                var operand1 = new SimpleAttributeOperand();
                operand1.TypeDefinitionId = ObjectTypeIds.BaseEventType;
                operand1.BrowsePath.Add(BrowseNames.SuppressedOrShelved);
                operand1.AttributeId = Attributes.Value;

                // specify the value to compare the Severity property with.
                var operand2 = new LiteralOperand();
                operand2.Value = new Variant(false);

                // specify that the Severity property must Equal the value specified.
                element2 = whereClause.Push(FilterOperator.Equals, operand1, operand2);

                // chain multiple elements together with an AND clause.
                if (element1 != null)
                {
                    element1 = whereClause.Push(FilterOperator.And, element1, element2);
                }
                else
                {
                    element1 = element2;
                }
            }

            // add the event types.
            if (EventTypes != null && EventTypes.Count > 0)
            {
                element2 = null;

                // save the last element.
                for (var ii = 0; ii < EventTypes.Count; ii++)
                {
                    // for this example uses the 'OfType' operator to limit events to thoses with specified event type. 
                    var operand1 = new LiteralOperand();
                    operand1.Value = new Variant(EventTypes[ii]);
                    var element3 = whereClause.Push(FilterOperator.OfType, operand1);

                    // need to chain multiple types together with an OR clause.
                    if (element2 != null)
                    {
                        element2 = whereClause.Push(FilterOperator.Or, element2, element3);
                    }
                    else
                    {
                        element2 = element3;
                    }
                }

                // need to link the set of event types with the previous filters.
                if (element1 != null)
                {
                    whereClause.Push(FilterOperator.And, element1, element2);
                }
            }

            filter.WhereClause = whereClause;

            // return filter.
            return filter;
        }
        #endregion

        #region [Private]
        /// <summary>
        /// Collects the fields for the event type.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="eventFields">The event fields.</param>
        private void CollectFields(Session session, NodeId eventTypeId, SimpleAttributeOperandCollection eventFields)
        {
            // get the supertypes.
            var supertypes = Utilities.BrowseSuperTypes(session, eventTypeId, false);

            if (supertypes == null)
            {
                return;
            }

            // process the types starting from the top of the tree.
            var foundNodes = new Dictionary<NodeId, QualifiedNameCollection>();
            var parentPath = new QualifiedNameCollection();

            for (var ii = supertypes.Count - 1; ii >= 0; ii--)
            {
                CollectFields(session, (NodeId)supertypes[ii].NodeId, parentPath, eventFields, foundNodes);
            }

            // collect the fields for the selected type.
            CollectFields(session, eventTypeId, parentPath, eventFields, foundNodes);
        }

        /// <summary>
        /// Collects the fields for the instance node.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="nodeId">The node id.</param>
        /// <param name="parentPath">The parent path.</param>
        /// <param name="eventFields">The event fields.</param>
        /// <param name="foundNodes">The table of found nodes.</param>
        private void CollectFields(
            Session session,
            NodeId nodeId,
            QualifiedNameCollection parentPath,
            SimpleAttributeOperandCollection eventFields,
            Dictionary<NodeId, QualifiedNameCollection> foundNodes)
        {
            // find all of the children of the field.
            var nodeToBrowse = new BrowseDescription();

            nodeToBrowse.NodeId = nodeId;
            nodeToBrowse.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse.ReferenceTypeId = ReferenceTypeIds.Aggregates;
            nodeToBrowse.IncludeSubtypes = true;
            nodeToBrowse.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable);
            nodeToBrowse.ResultMask = (uint)BrowseResultMask.All;

            var children = Utilities.Browse(session, nodeToBrowse, false);

            if (children == null)
            {
                return;
            }

            // process the children.
            for (var ii = 0; ii < children.Count; ii++)
            {
                var child = children[ii];

                if (child.NodeId.IsAbsolute)
                {
                    continue;
                }

                // construct browse path.
                var browsePath = new QualifiedNameCollection(parentPath);
                browsePath.Add(child.BrowseName);

                // check if the browse path is already in the list.
                if (!ContainsPath(eventFields, browsePath))
                {
                    var field = new SimpleAttributeOperand();

                    field.TypeDefinitionId = ObjectTypeIds.BaseEventType;
                    field.BrowsePath = browsePath;
                    field.AttributeId = child.NodeClass == NodeClass.Variable ? Attributes.Value : Attributes.NodeId;

                    eventFields.Add(field);
                }

                // recusively find all of the children.
                var targetId = (NodeId)child.NodeId;

                // need to guard against loops.
                if (!foundNodes.ContainsKey(targetId))
                {
                    foundNodes.Add(targetId, browsePath);
                    CollectFields(session, (NodeId)child.NodeId, browsePath, eventFields, foundNodes);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified select clause contains the browse path.
        /// </summary>
        /// <param name="selectClause">The select clause.</param>
        /// <param name="browsePath">The browse path.</param>
        /// <returns>
        /// 	<c>true</c> if the specified select clause contains path; otherwise, <c>false</c>.
        /// </returns>
        private bool ContainsPath(SimpleAttributeOperandCollection selectClause, QualifiedNameCollection browsePath)
        {
            for (var ii = 0; ii < selectClause.Count; ii++)
            {
                var field = selectClause[ii];

                if (field.BrowsePath.Count != browsePath.Count)
                {
                    continue;
                }

                var match = true;

                for (var jj = 0; jj < field.BrowsePath.Count; jj++)
                {
                    if (field.BrowsePath[jj] != browsePath[jj])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
