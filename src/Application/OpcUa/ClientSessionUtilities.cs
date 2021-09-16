using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Extensions;
using OMP.Connector.Domain.OpcUa;
using Opc.Ua;
using Opc.Ua.Client;

namespace OMP.Connector.Application.OpcUa
{
    public static class ClientSessionUtilities
    {
        public static ReferenceDescriptionCollection Browse(Session session, BrowseDescription browseDescription, ILogger logger)
        {
            var browseDescriptionCollection = new BrowseDescriptionCollection { browseDescription };

            session.Browse(default,
                default,
                200u,
                browseDescriptionCollection,
                out var results,
                out var diagnosticInfo);

            ValidateResponseDiagnostics(browseDescriptionCollection, results, diagnosticInfo);

            var comparer = new ReferenceDescriptionEqualityComparer();
            var continuationPoint = results[0].ContinuationPoint;
            var references = results[0].References.Distinct(comparer).ToList();

            logger.Trace($"Browsed NodeId: {browseDescription.NodeId} and found [{references.Count}] references!");

            while (continuationPoint != null)
            {
                logger.Trace($"NodeId: {browseDescription.NodeId} has continuationPoint .....");
                var additionalReferences = BrowseNext(session, ref continuationPoint).Distinct(comparer).ToList();

                if (additionalReferences.Any())
                    references.AddRange(additionalReferences);

                logger.Trace($"Browsed continuationPoint,  NodeId: {browseDescription.NodeId} and found [{additionalReferences.Count}] references!");
            }
            return new ReferenceDescriptionCollection(references);
        }

        private static ReferenceDescriptionCollection BrowseNext(SessionClient session, ref byte[] continuationPoint)
        {
            var continuationPoints = new ByteStringCollection { continuationPoint };

            session.BrowseNext(
                null,
                false,
                continuationPoints,
                out var results,
                out var diagnosticInfo);

            ClientBase.ValidateResponse(results, continuationPoints);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfo, continuationPoints);

            continuationPoint = results[0].ContinuationPoint;
            return results[0].References;
        }

        public static void GetMethodArguments(Session session, NodeId methodNodeId, ILogger logger, out IEnumerable<Argument> inputArguments, out IEnumerable<Argument> outArguments)
        {
            inputArguments = new List<Argument>();
            outArguments = new List<Argument>();
            var browseDescription = new BrowseDescription
            {
                NodeId = methodNodeId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                IncludeSubtypes = true,
                NodeClassMask = (uint)NodeClass.Variable,
                ResultMask = (uint)BrowseResultMask.BrowseName
            };

            var methodReferences = Browse(session, browseDescription, logger);

            var readValuesIds = (from reference in methodReferences where !reference.NodeId.IsAbsolute where reference.BrowseName == BrowseNames.InputArguments || reference.BrowseName == BrowseNames.OutputArguments select new ReadValueId { NodeId = (NodeId)reference.NodeId, AttributeId = Attributes.Value, Handle = reference }).ToList();
            if (!readValuesIds.Any()) { return; }

            var readValueIdCollection = new ReadValueIdCollection(readValuesIds);
            session.Read(default,
                0,
                TimestampsToReturn.Neither,
                readValueIdCollection,
                out var results,
                out var diagnosticInfo);

            ValidateResponseDiagnostics(readValueIdCollection, results, diagnosticInfo);

            var combinedValueTuples = readValuesIds.Zip(results);

            foreach (var (readValue, dataValue) in combinedValueTuples)
            {
                if (!StatusCode.IsGood(dataValue.StatusCode)) continue;

                var reference = (ReferenceDescription)readValue.Handle;

                if (reference.BrowseName == BrowseNames.InputArguments)
                    inputArguments = (Argument[])ExtensionObject.ToArray(dataValue.GetValue<ExtensionObject[]>(null), typeof(Argument));


                if (reference.BrowseName == BrowseNames.OutputArguments)
                    outArguments = (Argument[])ExtensionObject.ToArray(dataValue.GetValue<ExtensionObject[]>(null), typeof(Argument));
            }

            foreach (var inputArgument in inputArguments)
                inputArgument.Value = TypeInfo.GetDefaultValue(inputArgument.DataType, inputArgument.ValueRank, session.TypeTree);
        }

        private static void ValidateResponseDiagnostics(IList request, IList response, DiagnosticInfoCollection diagnosticInfoCollection)
        {
            ClientBase.ValidateResponse(response, request);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfoCollection, request);
        }

        public static string GetNodeFriendlyDataType(Session session, NodeId dataTypeNodeId, int valueRank)
        {
            var dataType = session.NodeCache.Find(dataTypeNodeId);
            var dataTypeDisplayName = dataType?.DisplayName?.Text.ToLower();
            return valueRank >= ValueRanks.OneOrMoreDimensions ? $"{dataTypeDisplayName}[]" : dataTypeDisplayName;
        }
    }
}