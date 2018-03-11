using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages
{
    public class HomePageModel : PageModel
    {
        public int[] EventFeed = { 1,2,3,4,5,6,7,8,9,10};  // we can use something else instead of array to store data

        public void OnGet()
        {
            // to get ten, and then another ten.... do we pull all at once and partiton, or keep track of where we are and pull individual chucks
            //TODO: pull all events only add ten
        }
    }
}