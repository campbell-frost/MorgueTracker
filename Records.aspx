<%@ Page Title="Records" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Records.aspx.cs" EnableEventValidation="false" Inherits="MorgueTracker.Records" %>


<asp:Content ID="body" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <section class="row justify-content-center" aria-labelledby="recordsTitle">
            <div class="col-12">

                <div class="row flex-wrap">
                    <div class="start-date col-md-3 d-flex flex-column align-items-start">
                        <asp:Label ID="lblStartDate" runat="server" Text="Start Date:" class="date-picker-label"></asp:Label>
                        <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" OnTextChanged="SearchByDate_Click" AutoPostBack="true" CssClass="date-picker" />
                    </div>
                    <div class="end-date col-md-7 d-flex flex-column align-items-start">
                        <asp:Label ID="lblEndDate" runat="server" Text="End Date:" class="date-picker-label"></asp:Label>
                        <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" OnTextChanged="SearchByDate_Click" AutoPostBack="true" CssClass="date-picker" />
                    </div>
                    <div class="picked-up col-md-2 d-flex flex-column justify-content-md-end justify-content-start">
                        <div class="form-check form-check-inline">
                            <asp:CheckBox ID="PickUpCheck" runat="server" OnCheckedChanged="SearchByDate_Click" AutoPostBack="true" CssClass="" />
                            <asp:Label ID="PickUpLabel" runat="server" Text="Picked Up" OnTextChanged="SearchByDate_Click" class="date-picker-label form-check-label no-wrap-label" AssociatedControlID="PickUpCheck" />
                        </div>
                    </div>
                </div>

                <asp:Label ID="lblStatus" runat="server" CssClass="form-control form-control-lg text-center my-5 col-md-3 p-5"></asp:Label>

                <div class="scrollable-gridview">
                    <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered table-light custom-table my-5" AllowPaging="true" PageSize="5" OnPageIndexChanging="gvList_PageIndexChanging">
                        <PagerSettings Position="Bottom" />
                        <Columns>
                            <asp:BoundField DataField="Patient_Name" HeaderText="Patient Name" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="Patient_ID" HeaderText="Patient ID" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="In_Employee_Name" HeaderText="Employee Name" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="In_Employee_ID" HeaderText="Employee ID" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="Created_Date" HeaderText="Created Date" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="Location_In_Morgue" HeaderText="Location In Morgue" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="Funeral_Home" HeaderText="Funeral Home" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="Funeral_Home_Employee" HeaderText="Funeral Home Employee" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="Out_Employee_Name" HeaderText="Release Employee Name" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="Out_Employee_ID" HeaderText="Release Employee ID" ItemStyle-CssClass="auto-width" />
                            <asp:BoundField DataField="Picked_Up_Date" HeaderText="Picked Up Date" ItemStyle-CssClass="auto-width" />
                        </Columns>
                    </asp:GridView>
                </div>


                <div class="row d-flex justify-content-end ">
                    <div class="col text-end">
                        <asp:Button ID="btnExport" runat="server" CssClass=" btn-media col-md-2 btn btn-primary btn-lg mb-5" OnClick="btnExport_Click" Text="Export" />
                    </div>
                </div>
            </div>
        </section>
    </main>
</asp:Content>
