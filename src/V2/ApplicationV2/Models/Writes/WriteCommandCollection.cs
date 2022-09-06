// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Writes
{
    public class WriteCommandCollection : List<WriteCommand>
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public WriteCommandCollection(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }
    }
}
