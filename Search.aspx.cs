using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace MorgueTracker
{
    public partial class Search : Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        bool isFullDetails = false; // Flag to track if all details are available
        bool isMorgueLocation = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            HideLabels();
            Page.SetFocus(txtPatientID);
        }

        protected string removeSlashes(string str)
        {
            str = str.Trim('\\');
            return str;
        }

        public static string removeSlashesEmployee(string input)
        {
            // Remove slashes before and after the string
            input = input.Trim('\\');

            // Remove leading and trailing zeros
            input = Regex.Replace(input, @"^0+|0+$", "");

            // Remove numbers followed by exactly four zeros
            input = Regex.Replace(input, @"(\d)0000", "$1");

            return input;
        }


        protected void Search_Click(object sender, EventArgs e)
        {
            string patientID = removeSlashes(txtPatientID.Text.Trim());

            // if ID cannot be parsed to an int, print error
            if (!int.TryParse(patientID, out int parsedPatientID))
            {
                lblSuccessStatus.Text = "Invalid Patient ID";
                lblSuccessStatus.Style.Add("border-color", "red");
                lblSuccessStatus.Visible = true;
                clearFuneralFields();

                return;
            }

            // Gets and executes stored procedure
            SqlCommand cmdSearchPatientByID = new SqlCommand("SearchPatientByID", conn);
            cmdSearchPatientByID.CommandType = CommandType.StoredProcedure;
            cmdSearchPatientByID.Parameters.AddWithValue("@Patient_ID", patientID);

            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdSearchPatientByID);
            DataTable dataTable = new DataTable();

            conn.Open();
            dataAdapter.Fill(dataTable);
            conn.Close();

            if (dataTable.Rows.Count <= 0)
            {
                lblSuccessStatus.Text = "No results found.";
                lblSuccessStatus.Style.Add("border-color", "red");
                lblSuccessStatus.Visible = true;
            }
            else
            {
                DataRow row = dataTable.Rows[0];
                isFullDetails = !row.IsNull("Out_Employee_Name");
                isMorgueLocation = !row.IsNull("Location_In_Morgue");

                // Make all fields visible
                ShowLabels();

                // gets data from database and puts it into the text fields
                txtPatientName.Text = row["Patient_Name"].ToString();
                txtEmployeeName.Text = row["In_Employee_Name"].ToString();
                txtEmployeeID.Text = row["In_Employee_ID"].ToString();
                pCreatedDate.Text = row["Created_Date"].ToString();

                if (isMorgueLocation)
                {
                    ddlLocationInMorgue.Text = row["Location_In_Morgue"].ToString();
                }

                else
                {
                    // Patient has no location, show "Select Location" in the dropdown
                    ddlLocationInMorgue.SelectedIndex = -1;
                    btnRelease.Visible = false;
                    btnRelease.Attributes.Add("style", "width: 100%");
                    btnUpdate.Attributes.Add("style", "width: 100%");
                }

                if (isFullDetails)
                {
                    // Patient is already released
                    btnRelease.Visible = false;
                    btnUpdate.Attributes.Add("style", "width: 100%;");
                    // Make Funeral home fields visible
                    ShowReleaseLabels();

                    // gets data from database and puts it into the text fields
                    txtFuneralHome.Text = row["Funeral_Home"].ToString();
                    txtOutEmployeeName.Text = row["Out_Employee_Name"].ToString();
                    txtFuneralHomeEmployee.Text = row["Funeral_Home_Employee"].ToString();
                    txtOutEmployeeID.Text = row["Out_Employee_ID"].ToString();
                }
            }
        }

        // Clears release information on error
        protected void clearFuneralFields()
        {
            txtFuneralHome.Text = "";
            txtFuneralHomeEmployee.Text = "";
            txtOutEmployeeName.Text = "";
            txtOutEmployeeID.Text = "";
        }


        protected void Update_Click(object sender, EventArgs e)
        {
            string patientID = removeSlashes(txtPatientID.Text.Trim());
            string patientName = txtPatientName.Text.ToString();
            string inEmployeeID = removeSlashes(txtEmployeeID.Text.Trim());
            string inEmployeeName = txtEmployeeName.Text.ToString();
            string locationInMorgue = ddlLocationInMorgue.SelectedItem.Text;


            removeSlashesEmployee(inEmployeeID);

            // input validation for names
            // allows normal letters, numbers, accented letters, spaces, commas, apotrophes, and dashes
            // does not allow ×Þß÷þø
            Regex nameRegex = new Regex("(?i)^(?:(?![×Þß÷þø])[- .'0-9a-zÀ-ÿ])+$");


            if (!nameRegex.IsMatch(inEmployeeName) && !nameRegex.IsMatch(patientName))
            {
                lblSuccessStatus.Text = "Please input valid patient and employee name.";
                lblSuccessStatus.Visible = true;
                lblSuccessStatus.Attributes.Add("style", "border-color: red;");
            }

            else if (!nameRegex.IsMatch(inEmployeeName))
            {
                lblSuccessStatus.Text = "Please input a valid employee name.";
                lblSuccessStatus.Visible = true;
                lblSuccessStatus.Attributes.Add("style", "border-color: red;");
            }

            else if (!nameRegex.IsMatch(patientName))
            {
                lblSuccessStatus.Text = "Please input a valid patient name.";
                lblSuccessStatus.Visible = true;
                lblSuccessStatus.Attributes.Add("style", "border-color: red;");
            }

            // if ID cannot be parsed to an int, print error
            if (!int.TryParse(inEmployeeID, out int parsedInEmployeeID))
            {
                lblSuccessStatus.Text = "Invalid Employee ID";
                lblSuccessStatus.Style.Add("border-color", "red");
                lblSuccessStatus.Visible = true;
                clearFuneralFields();
                return;
            }


            // if there is no location in morgue, update information in database excluding location in morgue
            else if (locationInMorgue.Equals("Select Location") || ddlLocationInMorgue.SelectedValue.Equals("-1"))
            {

                SqlCommand cmdUpdatePatientInfoNoLocation = new SqlCommand("dbo.UpdatePatientInfoNoLocation", conn);
                cmdUpdatePatientInfoNoLocation.CommandType = CommandType.StoredProcedure;

                cmdUpdatePatientInfoNoLocation.Parameters.AddWithValue("@Patient_ID", patientID);
                cmdUpdatePatientInfoNoLocation.Parameters.AddWithValue("@Patient_Name", patientName);
                cmdUpdatePatientInfoNoLocation.Parameters.AddWithValue("@In_Employee_ID", parsedInEmployeeID);
                cmdUpdatePatientInfoNoLocation.Parameters.AddWithValue("@In_Employee_Name", inEmployeeName);

                try
                {
                    conn.Open();
                    cmdUpdatePatientInfoNoLocation.ExecuteScalar();
                    lblSuccessStatus.Style.Add("border-color", "lightseagreen");
                    lblSuccessStatus.Text = "Information Updated!";
                    lblSuccessStatus.Visible = true;
                }
                catch (SqlException ex)
                {
                    lblSuccessStatus.Style.Add("border-color", "red");
                    lblSuccessStatus.Text = "An error occurred: " + ex.Message;
                    lblSuccessStatus.Visible = true;
                }
                finally
                {
                    conn.Close();
                }
            }

            // if there is a location, update the info, including the location in morgue
            else
            {
                SqlCommand cmdUpdatePatientInfo = new SqlCommand("dbo.UpdatePatientInfo", conn);
                cmdUpdatePatientInfo.CommandType = CommandType.StoredProcedure;

                cmdUpdatePatientInfo.Parameters.AddWithValue("@Patient_ID", patientID);
                cmdUpdatePatientInfo.Parameters.AddWithValue("@Patient_Name", patientName);
                cmdUpdatePatientInfo.Parameters.AddWithValue("@In_Employee_ID", parsedInEmployeeID);
                cmdUpdatePatientInfo.Parameters.AddWithValue("@In_Employee_Name", inEmployeeName);
                cmdUpdatePatientInfo.Parameters.AddWithValue("@Location_In_Morgue", locationInMorgue);

                try
                {
                    conn.Open();
                    cmdUpdatePatientInfo.ExecuteScalar();
                    lblSuccessStatus.Style.Add("border-color", "lightseagreen");
                    lblSuccessStatus.Text = "Information Updated!";
                    lblSuccessStatus.Visible = true;
                }
                catch (SqlException ex)
                {
                    lblSuccessStatus.Style.Add("border-color", "red");
                    lblSuccessStatus.Text = "An error occurred: " + ex.Message;
                    lblSuccessStatus.Visible = true;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        protected void Release_Click(object sender, EventArgs e)
        {
            // if there is no location in morgue, print error message
            string locationInMorgue = ddlLocationInMorgue.SelectedItem.Text;
            if (locationInMorgue.Equals("Select Location") || ddlLocationInMorgue.SelectedValue.Equals("-1"))
            {
                lblSuccessStatus.Style.Add("border-color", "red");
                lblSuccessStatus.Text = "Location in Morgue Required";
                lblSuccessStatus.Visible = true;
                return;
            }
            // else show release text fields
            else
            {
                txtOutEmployeeID.Visible = true;
                txtOutEmployeeName.Visible = true;
                txtFuneralHome.Visible = true;
                txtFuneralHomeEmployee.Visible = true;

                lblOutEmployeeID.Visible = true;
                lblOutEmployeeName.Visible = true;
                lblFuneralHome.Visible = true;
                lblFuneralHomeEmployee.Visible = true;
                hrLine.Visible = true;

                btnSubmit.Visible = true;
                btnDelete.Visible = true;
                ShowLabels();
            }
        }

        // submits release information
        protected void Submit_Click(object sender, EventArgs e)
        {
            if (!isFullDetails)
            {
                string patientID = removeSlashes(txtPatientID.Text.Trim());
                string funeralHome = txtFuneralHome.Text.Trim().ToString();
                string funeralEmployee = txtFuneralHomeEmployee.Text.Trim().ToString();
                string morgueEmployee = txtOutEmployeeName.Text.Trim().ToString();
                string morgueEmployeeID = removeSlashesEmployee(txtOutEmployeeID.Text.ToString());


                // if funeral home is empty, print error
                if (string.IsNullOrWhiteSpace(funeralHome))
                {
                    lblSuccessStatus.Text = "Funeral Home is required.";
                    lblSuccessStatus.Style.Add("border-color", "red");
                    lblSuccessStatus.Visible = true;
                    clearFuneralFields();

                    return;
                }

                // if ID cannot be parsed to an int, print error
                // removes slashes before parsing
                if (!int.TryParse(morgueEmployeeID, out int parsedMorgueEmployeeID))
                {
                    lblSuccessStatus.Text = "Invalid Employee ID" + morgueEmployeeID;
                    lblSuccessStatus.Style.Add("border-color", "red");
                    lblSuccessStatus.Visible = true;
                    clearFuneralFields();

                    return;
                }

                // if ID cannot be parsed to an int, print error
                if (!int.TryParse(patientID, out int parsedPatientID))
                {
                    lblSuccessStatus.Text = "Invalid Patient ID";
                    lblSuccessStatus.Style.Add("border-color", "red");
                    lblSuccessStatus.Visible = true;
                    clearFuneralFields();
                    return;
                }

                // input validation for names
                // allows normal letters, numbers, accented letters, spaces, commas, apotrophes, and dashes
                // does not allow ×Þß÷þø

                Regex nameRegex = new Regex("(?i)^(?:(?![×Þß÷þø])[- .'0-9a-zÀ-ÿ])+$");

                if (!nameRegex.IsMatch(funeralEmployee) && !nameRegex.IsMatch(morgueEmployee))
                {
                    lblSuccessStatus.Text = "Please input valid employee names.";
                    lblSuccessStatus.Visible = true;
                    lblSuccessStatus.Attributes.Add("style", "border-color: red;");
                }

                else if (!nameRegex.IsMatch(funeralEmployee))
                {
                    lblSuccessStatus.Text = "Please input a valid funeral employee name.";
                    lblSuccessStatus.Visible = true;
                    lblSuccessStatus.Attributes.Add("style", "border-color: red;");
                }

                else if (!nameRegex.IsMatch(morgueEmployee))
                {
                    lblSuccessStatus.Text = "Please input a valid morgue employee name.";
                    lblSuccessStatus.Visible = true;
                    lblSuccessStatus.Attributes.Add("style", "border-color: red;");
                }

                // input validation for funeral home name
                // allows normal letters, numbers, accented letters, spaces, commas, apotrophes, dashes, and '&' 
                // does not allow ×Þß÷þø
                Regex regex = new Regex("(?i)^(?:(?![×Þß÷þø])[-& .'0-9a-zÀ-ÿ])+$");

                if (!regex.IsMatch(funeralHome))
                {
                    lblSuccessStatus.Text = "Please input a valid funeral home name.";
                    lblSuccessStatus.Visible = true;
                    lblSuccessStatus.Attributes.Add("style", "border-color: red;");
                }

                // saves release information into the database
                else
                {

                    SqlCommand cmdInsertPickedUpInfo = new SqlCommand("dbo.InsertPickedUpInfo", conn);
                    cmdInsertPickedUpInfo.CommandType = CommandType.StoredProcedure;

                    cmdInsertPickedUpInfo.Parameters.AddWithValue("@Patient_ID", parsedPatientID);
                    cmdInsertPickedUpInfo.Parameters.AddWithValue("@Funeral_Home", funeralHome);
                    cmdInsertPickedUpInfo.Parameters.AddWithValue("@Funeral_Home_Employee", funeralEmployee);
                    cmdInsertPickedUpInfo.Parameters.AddWithValue("@Out_Employee_Name", morgueEmployee);
                    cmdInsertPickedUpInfo.Parameters.AddWithValue("@Out_Employee_ID", parsedMorgueEmployeeID);
                    cmdInsertPickedUpInfo.Parameters.AddWithValue("@Picked_Up_Date", DateTime.Now.ToString());

                    try
                    {
                        conn.Open();
                        cmdInsertPickedUpInfo.ExecuteNonQuery();
                        lblSuccessStatus.Text = "Patient Released!";
                        lblSuccessStatus.Visible = true;
                        lblSuccessStatus.Attributes.Add("style", "border-color: lightseagreen;");
                    }
                    catch (SqlException ex)
                    {
                        lblSuccessStatus.Text = "An error occurred: " + ex.Message;
                        lblSuccessStatus.Visible = true;
                        lblSuccessStatus.Attributes.Add("style", "border-color: red;");
                        clearFuneralFields();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        // deletes patient from database
        protected void Delete_Click(object sender, EventArgs e)
        {
            string patientID = removeSlashes(txtPatientID.Text.Trim());

            SqlCommand cmdDeletePatient = new SqlCommand("dbo.DeletePatient", conn);

            cmdDeletePatient.CommandType = CommandType.StoredProcedure;

            cmdDeletePatient.Parameters.AddWithValue("@Patient_ID", patientID);

            try
            {
                conn.Open();
                cmdDeletePatient.ExecuteScalar();
                lblSuccessStatus.Style.Add("border-color", "lightseagreen");
                lblSuccessStatus.Text = "Patient Deleted!";
                lblSuccessStatus.Visible = true;
            }
            catch (SqlException ex)
            {
                lblSuccessStatus.Style.Add("border-color", "red");
                lblSuccessStatus.Text = "An error occurred: " + ex.Message;
                lblSuccessStatus.Visible = true;
            }
            finally
            {
                conn.Close();
            }
        }

        private void HideLabels()
        {
            hrLine.Visible = false;

            txtPatientName.Visible = false;
            txtEmployeeID.Visible = false;
            txtEmployeeName.Visible = false;
            pCreatedDate.Visible = false;

            lblPatientName.Visible = false;
            lblEmployeeID.Visible = false;
            lblEmployeeName.Visible = false;
            lblCreatedDate.Visible = false;

            ddlLocationInMorgue.Visible = false;
            lblLocationInMorgue.Visible = false;

            btnUpdate.Visible = false;
            btnRelease.Visible = false;

            hrLine2.Visible = false;

            txtFuneralHome.Visible = false;
            txtFuneralHomeEmployee.Visible = false;
            txtOutEmployeeName.Visible = false;
            txtOutEmployeeID.Visible = false;

            lblFuneralHome.Visible = false;
            lblFuneralHomeEmployee.Visible = false;
            lblOutEmployeeName.Visible = false;
            lblOutEmployeeID.Visible = false;

            btnSubmit.Visible = false;
            btnDelete.Visible = false;

            lblSuccessStatus.Visible = false;

        }

        private void ShowLabels()
        {
            hrLine2.Visible = true;

            txtPatientName.Visible = true;
            txtEmployeeID.Visible = true;
            txtEmployeeName.Visible = true;
            pCreatedDate.Visible = true;

            lblPatientName.Visible = true;
            lblEmployeeID.Visible = true;
            lblEmployeeName.Visible = true;
            lblCreatedDate.Visible = true;

            ddlLocationInMorgue.Visible = true;
            lblLocationInMorgue.Visible = true;

            btnUpdate.Visible = true;
            btnRelease.Visible = true;
        }

        private void ShowReleaseLabels()
        {
            hrLine.Visible = true;

            txtFuneralHome.Visible = true;
            txtFuneralHomeEmployee.Visible = true;
            txtOutEmployeeName.Visible = true;
            txtOutEmployeeID.Visible = true;

            lblFuneralHome.Visible = true;
            lblFuneralHomeEmployee.Visible = true;
            lblOutEmployeeName.Visible = true;
            lblOutEmployeeID.Visible = true;

            btnSubmit.Visible = true;
            btnDelete.Visible = true;
        }
    }
}