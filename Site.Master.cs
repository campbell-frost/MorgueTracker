using System;
using System.IO;

using System.Web.UI;

namespace MorgueTracker3
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = Path.GetFileName(Request.Url.AbsolutePath);
            if (currentPage.EndsWith("InsertPatient"))
            {
                linkInsert.Attributes["class"] = "nav-link active";
            }
            else if(currentPage.EndsWith("Search"))
            {
                linkSearch.Attributes["class"] = "nav-link active";

            } 
            else if(currentPage.EndsWith("List"))
            {
                linkList.Attributes["class"] = "nav-link active";
            }
        }


    }
}