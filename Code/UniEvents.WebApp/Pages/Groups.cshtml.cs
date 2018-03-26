using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages {
   public class GroupsModel : WebAppPageModel {

        public int[] UserGroups = { 1, 2, 3 };
        public int[] UserInvites = { 1, 2 };

        public GroupsModel(IHttpContextAccessor accessor) : base(accessor) { }

      public void OnGet() {

      }
   }
}