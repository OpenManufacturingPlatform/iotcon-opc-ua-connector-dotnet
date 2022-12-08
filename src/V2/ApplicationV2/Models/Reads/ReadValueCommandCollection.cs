// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Reads
{
    public class ReadValueCommandCollection : List<ReadValueCommand>
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public ReadValueCommandCollection(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }
    }
}
