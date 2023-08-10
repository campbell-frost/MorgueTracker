using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Text.RegularExpressions;

namespace MorgueTracker
{
    public partial class Intake : Page
    {
        private SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);

        // to check if patient ID already exists
        int count = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            lbStatus.Visible = false;
            txtPatientID.Focus();
        }

        public static string removeSlashesPatient(string str)
        {
            return str.Replace("\\", "");
        }

        public static string removeSlashesEmployee(string str)
        {
            // Remove slashes before and after the string
            str = str.Trim('\\');

            // Remove leading and trailing zeros
            str = Regex.Replace(str, @"^0+|0+$", "");

            // Remove numbers followed by exactly four zeros
            str = Regex.Replace(str, @"(\d)0000", "$1");

            return str;
        }

        protected void Submit_OnClick(object sender, EventArgs e)
        {
            string PatientID = removeSlashesPatient(txtPatientID.Text.ToString());
            string PatientName = txtPatientName.Text.ToString();
            string EmployeeID = removeSlashesEmployee(txtEmployeeID.Text.ToString());
            string EmployeeName = txtEmployeeName.Text.ToString();

            // Check that Patient ID that already exists
            SqlCommand cmdCheckPatientExists = new SqlCommand("SELECT COUNT(*) FROM MorgueTracker WHERE Patient_ID = @Patient_ID", conn);
            cmdCheckPatientExists.Parameters.AddWithValue("@Patient_ID", PatientID);

            try
            {
                conn.Open();
                count = (int)cmdCheckPatientExists.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                lbStatus.Style.Add("border-color", "red");
                lbStatus.Text = "An error occurred: " + ex.Message;
                lbStatus.Visible = true;
            }
            finally
            {
                conn.Close();
            }

            // input validation for empty patient ID
            if (string.IsNullOrEmpty(txtPatientID.Text))
            {
                lbStatus.Text = "Please Input a Patient ID";
                lbStatus.Visible = true;
                lbStatus.Attributes.Add("style", "border-color: red;");

                return;
            }

            // if ID cannot be parsed to an int, print error
            if (!int.TryParse(PatientID, out int parsedPatientID))
            {
                lbStatus.Text = "Invalid Patient ID";
                lbStatus.Style.Add("border-color", "red");
                lbStatus.Visible = true;

                return;
            }

            // if ID cannot be parsed to an int, print error
            if (!int.TryParse(EmployeeID, out int parsedEmployeeID))
            {
                lbStatus.Text = "Invalid Employee ID";
                lbStatus.Style.Add("border-color", "red");
                lbStatus.Visible = true;

                return;
            }

            // input validation for names
            // allows normal letters, numbers, accented letters, spaces, commas, apotrophes, and dashes
            // does not allow ×Þß÷þø
            Regex nameRegex = new Regex("(?i)^(?:(?![×Þß÷þø])[- .'0-9a-zÀ-ÿ])+$");

            if (!nameRegex.IsMatch(EmployeeName) && !nameRegex.IsMatch(PatientName))
            {
                lbStatus.Text = "Please input a valid patient and employee name.";
                lbStatus.Visible = true;
                lbStatus.Attributes.Add("style", "border-color: red;");
            }

            else if (!nameRegex.IsMatch(PatientName))
            {
                lbStatus.Text = "Please input a valid patient name.";
                lbStatus.Visible = true;
                lbStatus.Attributes.Add("style", "border-color: red;");
            }

            else if (!nameRegex.IsMatch(EmployeeName))
            {
                lbStatus.Text = "Please input a valid employee name.";
                lbStatus.Visible = true;
                lbStatus.Attributes.Add("style", "border-color: red;");
            }

            // insert patient into database
            else
            {
                SqlCommand cmdInsertMorguePatient = new SqlCommand("InsertMorguePatient", conn);
                cmdInsertMorguePatient.CommandType = System.Data.CommandType.StoredProcedure;

                cmdInsertMorguePatient.Parameters.AddWithValue("@Patient_ID", parsedPatientID);
                cmdInsertMorguePatient.Parameters.AddWithValue("@Patient_Name", PatientName);
                cmdInsertMorguePatient.Parameters.AddWithValue("@In_Employee_ID", parsedEmployeeID);
                cmdInsertMorguePatient.Parameters.AddWithValue("@In_Employee_Name", EmployeeName);
                cmdInsertMorguePatient.Parameters.AddWithValue("@Created_Date", DateTime.Now.ToString());

                try
                {
                    conn.Open();
                    cmdInsertMorguePatient.ExecuteNonQuery();

                    lbStatus.Text = "Patient Added Successfuly";
                    lbStatus.Visible = true;
                    lbStatus.Attributes.Add("style", "border-color: lightseagreen;");
                }
                catch
                {
                    lbStatus.Visible = true;
                    lbStatus.Attributes.Add("style", "border-color: red;");

                    // input validation for existing patient ID
                    if (count > 0)
                    {
                        lbStatus.Text = "Patient ID already exists";
                    }
                    else
                    {
                        lbStatus.Text = "Patient Upload Failed";
                    }
                }
                finally
                {
                    conn.Close();
                }

            }
        }
    }
}