// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Call
{
    public class CallCommandCollection:List<CallCommand>
    {
        public string EndpointUrl { get; set; } = string.Empty;

        public CallCommandCollection()        {        }

        public CallCommandCollection(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }
    }
}
