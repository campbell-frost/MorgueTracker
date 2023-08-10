using System;
using System.IO;
using System.Web.UI;

namespace MorgueTracker
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = Path.GetFileName(Request.Url.AbsolutePath);
            if (currentPage.EndsWith("Intake"))
            {
                linkInsert.Attributes["class"] = "nav-link active";
            }
            else if(currentPage.EndsWith("Search"))
            {
                linkSearch.Attributes["class"] = "nav-link active";
            } 
            else if(currentPage.EndsWith("Records"))
            {
                linkList.Attributes["class"] = "nav-link active";
            }
        }


    }
}