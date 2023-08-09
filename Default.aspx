<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MorgueTracker3._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <div class="container">
            <div class="home-buttons">
                <a class="btn btn-primary" href="InsertPatient.aspx">
                    <i class="fas fa-user-plus"></i>  Insert 
                </a>
                <a class="btn btn-secondary" href="Search.aspx">
                    <i class="fas fa-search"></i>  Search
                </a>
            </div>
        </div>
    </main>
</asp:Content>
