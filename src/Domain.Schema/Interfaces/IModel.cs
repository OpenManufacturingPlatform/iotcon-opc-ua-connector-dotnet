// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.Connector.Domain.Schema.Interfaces
{
    public interface IModel
    {
        string Id { get; set; }
        string Namespace { get; set; }
        string Schema { get; set; }
    }
}