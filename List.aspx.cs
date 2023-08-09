
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MorgueTracker3
{
    public partial class List : Page
    {
        private string selectAll = "SELECT Patient_Name, Patient_ID, In_Employee_Name, In_Employee_ID, Created_Date, Out_Employee_Name, Out_Employee_ID, Location_In_Morgue, Funeral_Home, Funeral_Home_Employee, Picked_Up_Date ";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGridView();
            }
        }

        // Method for picking the date button(s) which then populates the table.
        protected void SearchByDate_Click(object sender, EventArgs e)
        {
            BindGridView();
        }

        protected void gvList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvList.PageIndex = e.NewPageIndex;
            BindGridView();
        }

        // Method for getting data from the database and binding a grid view


        private void BindGridView()
        {
            bool isPickedUp = PickUpCheck.Checked;
            string dtStartDate = txtStartDate.Text;
            string dtEndDate = txtEndDate.Text;

            string query = BuildQuery(isPickedUp, dtStartDate, dtEndDate);
            string connectionString = ConfigurationManager.ConnectionStrings["Conn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(dtStartDate))
                    {
                        cmd.Parameters.AddWithValue("@startDate", dtStartDate);
                    }
                    if (!string.IsNullOrEmpty(dtEndDate))
                    {
                        cmd.Parameters.AddWithValue("@endDate", dtEndDate);
                    }

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }

                    gvList.DataSource = dt;
                    gvList.DataBind();

                    lblStatus.Visible = dt.Rows.Count == 0;
                    if (dt.Rows.Count == 0)
                    {
                        lblStatus.Text = "No Data Found";
                        lblStatus.Attributes.Add("style", "border-color: red;");
                    }
                }
            }
        }

        // Method for building a query depending on the date picked by the user and whether picked up box is checked or not.

        private string BuildQuery(bool isPickedUp, string startDate, string endDate)
        {
            string query = selectAll + "FROM MorgueTracker ";

            if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate))
            {
                if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
                {
                    query += "WHERE CAST(Created_Date AS DATE) >= @startDate ";
                }
                else if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    query += "WHERE CAST(Created_Date AS DATE) <= @endDate ";
                }
                else
                {
                    query += "WHERE CAST(Created_Date AS DATE) BETWEEN @startDate AND @endDate ";
                }

                query += isPickedUp ? "AND Funeral_Home IS NOT NULL " : "AND Funeral_Home IS NULL ";
            }
            else
            {
                query += isPickedUp ? "WHERE Funeral_Home IS NOT NULL " : "WHERE Funeral_Home IS NULL ";
            }

            query += "ORDER BY Created_Date DESC";

            return query;
        }

        // Method for downloading the data when export button is clicked on screen.

        protected void btnExport_Click(object sender, EventArgs e)
        {
            int pageSize = gvList.PageSize;
            int pageIndex = gvList.PageIndex;

            gvList.AllowPaging = false;
            BindGridView();

            string dateTimeWithHyphens = DateTime.Now.ToString("MM-dd-yyyy");

            if (gvList.Rows.Count == 0)
            {
                lblStatus.Text = "Cannot export empty dataset.";
                lblStatus.Visible = true;
            }
            else
            {
                ExportGridToExcel(dateTimeWithHyphens);
            }

            gvList.AllowPaging = true;
            gvList.PageSize = pageSize;
            gvList.PageIndex = pageIndex;
            BindGridView();
        }


        // Method for exporting the data into an excel file.

        private void ExportGridToExcel(string dateTimeWithHyphens)
        {
            string pickedUp = PickUpCheck.Checked ? "Picked_Up" : "Not_Picked_Up";
            string fileName = GetExportFileName(pickedUp, dateTimeWithHyphens);

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName); //telling the browser that the content should be treated
                                                                                          //as an attachment that will be downloaded and saved locally.
                                                                                          //The filename of this download is set to the fileName string.
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel"; // signifies Excel data.

            using (StringWriter sw = new StringWriter())
            {
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                StyleGridForExport(hw);
                Response.Output.Write(sw.ToString()); //the data that the client will receive.
                Response.Flush();
                Response.End();
            }
        }

        // Method for getting the formatted file name (.xls).

        private string GetExportFileName(string pickedUp, string dateTimeWithHyphens)
        {
            string startDateText = txtStartDate.Text;
            string endDateText = txtEndDate.Text;

            if (string.IsNullOrEmpty(startDateText) && string.IsNullOrEmpty(endDateText))
            {
                return $"{pickedUp}_Morgue_Patients_as_of({dateTimeWithHyphens}).xls";
            }
            else if (string.IsNullOrEmpty(endDateText))
            {
                DateTime startDate = DateTime.ParseExact(startDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                string startFormattedDateString = startDate.ToString("MM-dd-yyyy");

                return $"{pickedUp}_Morgue_Patients({startFormattedDateString}_to_{dateTimeWithHyphens}).xls";
            }
            else if (string.IsNullOrEmpty(startDateText))
            {
                DateTime endDate = DateTime.ParseExact(endDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                string endFormattedDateString = endDate.ToString("MM-dd-yyyy");

                return $"{pickedUp}_Morgue_Patients_until({endFormattedDateString}).xls";
            }
            else
            {
                DateTime startDate = DateTime.ParseExact(startDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                string startFormattedDateString = startDate.ToString("MM-dd-yyyy");
                DateTime endDate = DateTime.ParseExact(endDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                string endFormattedDateString = endDate.ToString("MM-dd-yyyy");

                return $"{pickedUp}_Morgue_Patients({startFormattedDateString}_to_{endFormattedDateString}).xls";
            }
        }


        // Method for styling the downloaded excel spreadsheet.

        private void StyleGridForExport(HtmlTextWriter hw)
        {
            gvList.HeaderRow.Style.Add("background-color", "#edfbfb");

            foreach (GridViewRow row in gvList.Rows)
            {
                row.BackColor = System.Drawing.Color.White;

                foreach (TableCell cell in row.Cells)
                {
                    cell.CssClass = "textmode";
                    cell.Attributes.Add("style", "mso-number-format:\\@"); //Microsoft office specific css format
                }
            }

            gvList.RenderControl(hw);
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            /*
             * NEED THIS METHOD IN ORDER TO MAKE GetExportFileName() METHOD WORK!!
             * 
             * Confirms that an HtmlForm control is rendered for the specified ASP.NET
               server control at run time. */
        }
    }
}
