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

using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUA.Services.Alarms
{
    internal sealed class AlarmFilterDefinition
    {
        public NodeId AreaId { get; set; }
        public EventSeverity Severity { get; set; }
        public IList<NodeId> EventTypes { get; set; }
        public bool IgnoreSuppressedOrShelved { get; set; }
        public SimpleAttributeOperandCollection SelectClauses { get; set; }

        #region [Public]
        public MonitoredItem CreateMonitoredItem()
        {
            // create the item with the filter.
            var monitoredItem = new MonitoredItem();

            SetMonitoredItemProperties(monitoredItem);

            return monitoredItem;
        }

        private void SetMonitoredItemProperties(MonitoredItem existingMonitoredItem)
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
            existingMonitoredItem.Filter = ConstructFilter();

            existingMonitoredItem.Handle = this;
        }

        /// <summary>
        /// Constructs the event filter for the subscription.
        /// </summary>
        /// <returns>The event filter.</returns>
        private EventFilter ConstructFilter()
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
    }
}
