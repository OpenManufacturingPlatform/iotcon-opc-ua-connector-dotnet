// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Reads
{
    public class ReadNodeCommandCollection : List<ReadNodeCommand>
    {
        public string EndpointUrl { get; set; }
        public ReadNodeCommandCollection(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }
    }
}
