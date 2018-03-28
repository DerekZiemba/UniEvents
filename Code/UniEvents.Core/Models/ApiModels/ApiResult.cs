using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZMBA;

namespace UniEvents.Models.ApiModels {

   public interface IApiResult {
      [JsonProperty(propertyName: "success")]
      bool bSuccess { get; set; }
      [JsonProperty(propertyName: "message")]
      string sMessage { get; set; }
   }

   public class ApiResult : IApiResult {
      private string _message = "";

      [JsonProperty(propertyName:"success")]
      public bool bSuccess { get; set; }

      [JsonProperty(propertyName: "message")]
      public string sMessage { get => _message; set => _message = value ?? ""; }

      public ApiResult AppendMessage(string message) {
         if (message.IsNotWhitespace()) {
            sMessage = String.IsNullOrWhiteSpace(sMessage) ? message : sMessage + " \r\n | " + message;
         }
         return this;
      }

      public ApiResult() { }
      public ApiResult(bool success, string message) {
         this.bSuccess = success;
         this.sMessage = message;
      }


      public ApiResult Failure(string message) { this.bSuccess = false; this.sMessage = message; return this; }
      public ApiResult Failure(Exception ex) { this.bSuccess = false; this.sMessage = ex.Message; AppendMessage(ex.InnerException?.Message); return this; }
      public ApiResult Failure(string message, Exception ex) { return this.Failure(message).AppendMessage(ex.Message).AppendMessage(ex.InnerException?.Message); }

      /// <summary>Because I already used "Success" as a property name and this the closes synonym I can think of. </summary>
      public ApiResult Success(string message) { this.bSuccess = true; this.sMessage = message; return this; }
   }

   public class ApiResult<T> : ApiResult {
      public T Result { get; set; }
      public ApiResult() { }
      public ApiResult(bool success, string message) : base(success, message) { }
      public ApiResult(bool success, string message, T result) : base(success, message) {
         this.Result = result;
      }
      public ApiResult(T result) {
         this.Result = result;
      }

      public new ApiResult<T> AppendMessage(string message) { base.AppendMessage(message); return this;  }

      public new ApiResult<T> Failure(string message) { this.bSuccess = false; this.sMessage = message; return this; }
      public new ApiResult<T> Failure(Exception ex) { this.bSuccess = false; this.sMessage = ex.Message; AppendMessage(ex.InnerException?.Message); return this;  }
      public new ApiResult<T> Failure(string message, Exception ex) { return this.Failure(message).AppendMessage(ex.Message).AppendMessage(ex.InnerException?.Message); }

      public new ApiResult<T> Success(string message) { this.bSuccess = true; this.sMessage = message; return this; }
      public ApiResult<T> Success(T value) { this.bSuccess = true; this.Result = value; return this; }
      public ApiResult<T> Success(string message, T value) { this.bSuccess = true; this.sMessage = message; this.Result = value; return this; }
     
      public ApiResult<T> NotNull(T value) { this.bSuccess = value != null; this.Result = value; return this; }
      public ApiResult<T> NotNull(string message, T value) { this.bSuccess = value != null; this.sMessage = message; this.Result = value; return this; }

   }

}
