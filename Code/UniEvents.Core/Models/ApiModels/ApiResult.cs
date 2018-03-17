using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniEvents.Models.ApiModels {

   public interface IApiResult {
      bool Success { get; set; }
      string Message { get; set; }
   }

   public class ApiResult : IApiResult {
      public bool Success { get; set; }
      public string Message { get; set; } = "";

      public ApiResult() { }
      public ApiResult(bool success, string message) {
         this.Success = success;
         this.Message = message ?? "";
      }
   }

   public class ApiResult<T> : ApiResult {
      public T Result { get; set; }
      public ApiResult() { }
      public ApiResult(bool success, string message) : base(success, message) {  }

      public ApiResult(bool success, string message, T result) : base(success, message) {
         this.Result = result;
      }
      public ApiResult(T result) {
         this.Result = result;
      }
   }

}
