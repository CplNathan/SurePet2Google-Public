using GoogleHelper.Context;
using GoogleHelper.Json;
using GoogleHelper.Models;
using System.Text.Json.Nodes;

namespace GoogleHelper.Services
{
    public interface IDeviceService
    {
        public Task<TResponse> QueryAsync<TResponse>(BaseContext session, BaseDeviceModel deviceModel, string deviceId, CancellationToken token)
            where TResponse : QueryDeviceData, new();
        public Task<TResponse> ExecuteAsync<TResponse>(BaseContext session, BaseDeviceModel deviceModel, string deviceId, string requestId, JsonObject data, CancellationToken token)
            where TResponse : ExecuteDeviceData, new();
        public Task<bool> FetchAsync(BaseContext session, BaseDeviceModel deviceModel, string deviceId, bool forceFetch = false);

        public List<string> ModelIdentifiers { get; }

        public Type ModelType { get; }
    }

    public abstract class BaseDeviceService<TContext, TModel> : IDeviceService
        where TContext : BaseContext
        where TModel : BaseDeviceModel, new()
    {
        public string Id { get; set; }

        public BaseDeviceService()
        {
            this.Id = Random.Shared.NextInt64().ToString();
        }

        public TModel DeviceModel => new()
        {
            Name = nameof(TModel)
        };

        public Type ModelType => this.DeviceModel.GetType();

        public List<string> ModelIdentifiers => this.DeviceModel.ModelIdentifiers;

        public abstract Task<TResponse> QueryAsyncImplementation<TResponse>(TContext session, TModel deviceModel, string deviceId, CancellationToken token) where TResponse : QueryDeviceData, new();

        public abstract Task<TResponse> ExecuteAsyncImplementation<TResponse>(TContext session, TModel deviceModel, string deviceId, string requestId, JsonObject data, CancellationToken token) where TResponse : ExecuteDeviceData, new();

        public abstract Task<bool> FetchAsyncImplementation(TContext session, TModel deviceModel, string deviceId, bool forceFetch = false);

        public async Task<TResponse> QueryAsync<TResponse>(BaseContext session, BaseDeviceModel deviceModel, string deviceId, CancellationToken token) where TResponse : QueryDeviceData, new()
        {
            return await this.QueryAsyncImplementation<TResponse>((TContext)session, (TModel)deviceModel, deviceId, token);
        }

        public async Task<TResponse> ExecuteAsync<TResponse>(BaseContext session, BaseDeviceModel deviceModel, string deviceId, string requestId, JsonObject data, CancellationToken token) where TResponse : ExecuteDeviceData, new()
        {
            return await this.ExecuteAsyncImplementation<TResponse>((TContext)session, (TModel)deviceModel, deviceId, requestId, data, token);
        }

        public async Task<bool> FetchAsync(BaseContext session, BaseDeviceModel deviceModel, string deviceId, bool forceFetch = false)
        {
            return await this.FetchAsyncImplementation((TContext)session, (TModel)deviceModel, deviceId, forceFetch);
        }
    }
}
