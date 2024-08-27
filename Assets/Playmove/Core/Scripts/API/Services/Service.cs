using Newtonsoft.Json;
using Playmove.Core.API.Models;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Playmove.Core.API.Services
{
    public abstract class Service<T, Vm>
        where T : VmItem<Vm>, new()
    {
        public static AsyncResult<LocalT> ParseJson<LocalT>(AsyncResult<DownloadHandler> result)
        {
            if (result.HasError)
                return new AsyncResult<LocalT>(default, result.Error);
            
            try
            {
                return new AsyncResult<LocalT>((LocalT)JsonConvert.DeserializeObject(result.Data.text), string.Empty);
            }
            catch (Exception e)
            {
                return new AsyncResult<LocalT>(default, e.ToString());
            }
        }

        public static AsyncResult<T> ParseVmJson(AsyncResult<DownloadHandler> result)
        {
            if (result.HasError)
                return new AsyncResult<T>(null, result.Error);

            var data = new T();
            try
            {
                data.SetDataFromVmJson(result.Data.text);
            }
            catch (Exception e)
            {
                return new AsyncResult<T>(null, e.ToString());
            }

            return new AsyncResult<T>(data, string.Empty);
        }

        public static AsyncResult<List<T>> ParseVmsJson(AsyncResult<DownloadHandler> result)
        {
            if (result.HasError)
                return new AsyncResult<List<T>>(null, result.Error);

            List<T> datas = new List<T>();
            try
            {
                var vmDatas = JsonConvert.DeserializeObject<List<Vm>>(result.Data.text);
                foreach (var vmData in vmDatas)
                {
                    T data = new T();
                    data.SetDataFromVm(vmData);
                    datas.Add(data);
                }

                return new AsyncResult<List<T>>(datas, string.Empty);
            }
            catch (Exception e)
            {
                return new AsyncResult<List<T>>(null, e.ToString());
            }
        }

        public static AsyncResult<bool> SimpleResult(AsyncResult<DownloadHandler> result)
        {
            if (result.HasError)
                return new AsyncResult<bool>(false, result.Error);
            else
                return new AsyncResult<bool>(true, string.Empty);
        }

        public static string GetVmsJson(List<T> data)
        {
            return JsonConvert.SerializeObject(data);
        } 
    }
}
