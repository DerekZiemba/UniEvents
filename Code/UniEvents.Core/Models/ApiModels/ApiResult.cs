using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZMBA;

namespace UniEvents.Models.ApiModels {

   public abstract class ApiResultBase<T, TDerived> where TDerived : ApiResultBase<T, TDerived> {
      private string _message = "";

      [JsonProperty(propertyName: "success")]
      public bool bSuccess { get; set; }

      [JsonProperty(propertyName: "message")]
      public string sMessage { get => _message; set => _message = value ?? ""; }

      [JsonProperty(propertyName: "result")]
      public T Result { get; set; }

      public TDerived AppendMessage(string message) {
         if (message.IsNotWhitespace()) {
            sMessage = String.IsNullOrWhiteSpace(sMessage) ? message : sMessage + " \r\n | " + message;
         }
         return (TDerived)this; ;
      }

      public TDerived Failure(string message) { this.bSuccess = false; this.sMessage = message; return (TDerived)this; }
      public TDerived Failure(Exception ex) { this.bSuccess = false; this.sMessage = ex.Message; AppendMessage(ex.InnerException?.Message); return (TDerived)this; }
      public TDerived Failure(string message, Exception ex) { return this.Failure(message).AppendMessage(ex.Message).AppendMessage(ex.InnerException?.Message); }
      public TDerived Success(string message) { this.bSuccess = true; this.sMessage = message; return (TDerived)this; }
      public TDerived Success(T value) { this.bSuccess = true; this.Result = value; return (TDerived)this; }
      public TDerived Success(string message, T value) { this.bSuccess = true; this.sMessage = message; this.Result = value; return (TDerived)this; }
      public TDerived NotNull(T value) { this.bSuccess = value != null; this.Result = value; return (TDerived)this; }
      public TDerived NotNull(string message, T value) { this.bSuccess = value != null; this.sMessage = message; this.Result = value; return (TDerived)this; }

   }

   public class ApiResult : ApiResultBase<object, ApiResult> {
      public ApiResult() { }
      public ApiResult(bool success, string message) {
         this.bSuccess = success;
         this.sMessage = message;
      }
      public ApiResult(bool success, string message, object result) : this(success, message) {
         this.Result = result;
      }
      public ApiResult(object result) {
         this.Result = result;
      }
   }



   public class ApiResult<T> : ApiResultBase<T, ApiResult<T>> {
      public ApiResult() { }
      public ApiResult(bool success, string message) {
         this.bSuccess = success;
         this.sMessage = message;
      }
      public ApiResult(bool success, string message, T result) : this(success, message) {
         this.Result = result;
      }
      public ApiResult(T result) {
         this.Result = result;
      }

   }

}
