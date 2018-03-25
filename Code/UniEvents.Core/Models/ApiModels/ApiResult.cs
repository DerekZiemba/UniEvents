using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZMBA;

namespace UniEvents.Models.ApiModels {

   public interface IApiResult {
      bool Success { get; set; }
      string Message { get; set; }
   }

   public class ApiResult : IApiResult {
      private string _message = "";

      public bool Success { get; set; }
      public string Message { get => _message; set => _message = value ?? ""; }

      public ApiResult AppendMessage(string message) {
         if (!message.IsNullOrWhitespace()) {
            Message = Message.IsNullOrWhitespace() ? message : Message + " \r\n | " + message;
         }
         return this;
      }

      public ApiResult() { }
      public ApiResult(bool success, string message) {
         this.Success = success;
         this.Message = message;
      }


      public ApiResult Failure(string message) { this.Success = false; this.Message = message; return this; }
      public ApiResult Failure(Exception ex) { this.Success = false; this.Message = ex.Message; AppendMessage(ex.InnerException?.Message); return this; }
      public ApiResult Failure(string message, Exception ex) { return this.Failure(message).AppendMessage(ex.Message).AppendMessage(ex.InnerException?.Message); }

      /// <summary>Because I already used "Success" as a property name and this the closes synonym I can think of. </summary>
      public ApiResult Win(string message) { this.Success = true; this.Message = message; return this; }
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

      public new ApiResult<T> Failure(string message) { this.Success = false; this.Message = message; return this; }
      public new ApiResult<T> Failure(Exception ex) { this.Success = false; this.Message = ex.Message; AppendMessage(ex.InnerException?.Message); return this;  }
      public new ApiResult<T> Failure(string message, Exception ex) { return this.Failure(message).AppendMessage(ex.Message).AppendMessage(ex.InnerException?.Message); }

      /// <summary>Because I already used "Success" as a property name and this the closes synonym I can think of. </summary>
      public new ApiResult<T> Win(string message) { this.Success = true; this.Message = message; return this; }
      public ApiResult<T> Win(T value) { this.Success = true; this.Result = value; return this; }
      public ApiResult<T> Win(string message, T value) { this.Success = true; this.Message = message; this.Result = value; return this; }
     
      public ApiResult<T> NotNull(T value) { this.Success = value != null; this.Result = value; return this; }
      public ApiResult<T> NotNull(string message, T value) { this.Success = value != null; this.Message = message; this.Result = value; return this; }

   }

}
