// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Request.Control;
using OMP.Connector.Domain.Schema.Request.Subscription;
using OneOf.Types;
using Opc.Ua;
using ReadRequest = Opc.Ua.ReadRequest;

namespace OMP.Connector.Application.V2
{
    public interface IReadCommandService
    {
        Task<IEnumerable<ReadResponse>> ReadAsync(IEnumerable<ReadRequest> requests);
        Task<IEnumerable<ReadResponse>> RegisteredReadAsync(IEnumerable<ReadRequest> requests);
    }

    //internal interface IWritedCommandService
    //{
    //    Task<object> WriteAsync(WriteRequest request);//StatusCodeCollection
    //    Task<object> RegisteredWriteAsync(WriteRequest request);
    //}

    //internal interface IBrowsedCommandService
    //{
    //    Task<object> BrowseAsync(BrowseRequest request);
    //}

    //internal interface ICallCommandService
    //{
    //    Task<object> CallAsync(CallRequest request);
    //}

    //internal interface ISubscripptionCommandService
    //{
    //    Task<object> SubscribeAsync(CreateSubscriptionsRequest request);
    //    Task<object> UnsubscribeAsync(RemoveSubscriptionsRequest request);
    //    Task<object> UnsubscribeAsync(RemoveAllSubscriptionsRequest request);
    //}


    public delegate Task<IEnumerable<ReadResponse>> OnReadRequest(IEnumerable<ReadRequest> requests);
    //public delegate Task<object> OnWriteRequest(WriteRequest request);
    //public delegate Task<object> OnCallRequest(CallRequest request);
    //public delegate Task<object> OnBrowseRequest(BrowseRequest request);
    //public delegate Task<object> OnCreateSubscriptionsRequest(CreateSubscriptionsRequest request);
    //public delegate Task<object> OnRemoveSubscriptionsRequest(RemoveSubscriptionsRequest request);
    //public delegate Task<object> OnRemoveAllSubscriptionsRequest(RemoveAllSubscriptionsRequest request);

    //Hierdie is wat die client vir ons moet gee
        
    public interface IRequestSinkAndSource
    {
        Task OnTelemetryMessageReceived(object message); //so bear with me met die object

        event OnReadRequest OnReadRequest;

        Task StartRequestListner();
        //event OnWriteRequest OnWriteRequest;
        //event OnCallRequest OnCallRequest;
        //event OnBrowseRequest OnBrowseRequest;
        //event OnCreateSubscriptionsRequest OnCreateSubscriptionsRequest;
        //event OnRemoveSubscriptionsRequest OnRemoveSubscriptionsRequest;
        //event OnRemoveAllSubscriptionsRequest OnRemoveAllSubscriptionsRequest;
    }


    // OPTIONS 1 => FRAMEWORK LIKE
    public class OnsNuweRequestHandler : BackgroundService
    {
        private readonly IRequestSinkAndSource requestSinkAndSource;
        private readonly IReadCommandService readCommandService;

        public OnsNuweRequestHandler(
            IRequestSinkAndSource requestSinkAndSource, //PlantCon of EdgeStore gee dit
            IReadCommandService readCommandService // die is ons native code, wat jy ook kan override as jy wil (wat ons dalk in PC gaan doen vir ons custom requirements)            
            )
        {
            this.requestSinkAndSource = requestSinkAndSource;
            this.readCommandService = readCommandService;
            requestSinkAndSource.OnReadRequest += RequestSinkAndSource_OnReadRequest;

            // SO ONS CALL DIE IREQUEST SINK AND SOURCE
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return requestSinkAndSource.StartRequestListner();
        }

        private Task<IEnumerable<ReadResponse>> RequestSinkAndSource_OnReadRequest(IEnumerable<ReadRequest> requests)
        {
            return readCommandService.ReadAsync(requests);
        }        
    }

    public record struct ReadResponse(string NodeId, object? Value = null, ServiceResult? Errror = null);

    // In PlantCon
    class RaftsmanListnerFramework : IRequestSinkAndSource
    {
        public event OnReadRequest OnReadRequest;

        public Task OnTelemetryMessageReceived(object message)
        {
            throw new System.NotImplementedException();
        }

        public Task StartRequestListner()
        {
            throw new System.NotImplementedException();
        }

        private Task WheneverWeReceiveAnPAJFromRaftsman()
        {
            //Conver PAJ to ReadRequest
            //...
            var responses = OnReadRequest?.Invoke(null);
            // Map to PlantCon models and do serialization
            // send responses here to Raftsman
            return Task.CompletedTask;
        }

        // and then also remember to register on DI
    }

    // OPTION 2 => LIBRARY LIKE

    public interface IOmpRequestHandler
    {
        Task<IEnumerable<ReadResponse>> ReadNodeValues(IEnumerable<ReadRequest> requests);
    }

    public class OmpRequestHandler
    {
        private readonly IReadCommandService readCommandService;

        public OmpRequestHandler(IReadCommandService readCommandService)
        {
            this.readCommandService = readCommandService;
        }

        public Task<IEnumerable<ReadResponse>> ReadNodeValues(IEnumerable<ReadRequest> requests)
        {
            try
            {
                return readCommandService.ReadAsync(requests);
            }
            catch (System.Exception ex)
            {
                //....
                throw;
            }
        }

        // WE WILL THEN INJECT SingleTon<IOmpRequestHandler, OmpRequestHandler> into users DI
    }

    // BV IN PlantCon

    class RaftsmanListnerLibraryExample
    {
        private readonly IOmpRequestHandler ompRequestHandler;

        public RaftsmanListnerLibraryExample(IOmpRequestHandler ompRequestHandler)
        {
            this.ompRequestHandler = ompRequestHandler;
        }

        private Task WheneverWeReceiveAnPAJFromRaftsman()
        {
            //Conver PAJ to ReadRequest
            //...
            var responses = ompRequestHandler.ReadNodeValues(null);
            // Map to PlantCon models and do serialization
            // send responses here to Raftsman
            return Task.CompletedTask;
        }
    }

}
