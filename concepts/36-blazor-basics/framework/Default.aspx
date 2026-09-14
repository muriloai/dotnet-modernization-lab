<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebFormsLegadoDemo.Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <div class="card-header">Contador com ViewState e Full Page PostBack</div>
        <p style="color: #64748b; font-size: 0.9rem;">
            Cada clique no botao abaixo submete o formulario inteiro via HTTP POST, serializa o dicionario ViewState e renderiza novamente todo o HTML no servidor IIS.
        </p>

        <div class="counter-box">
            <asp:Label ID="lblContador" runat="server" Text="0" />
        </div>

        <div>
            <asp:Button ID="btnIncrementar" runat="server" CssClass="btn" Text="+ Incrementar (PostBack)" OnClick="btnIncrementar_Click" />
            <asp:Button ID="btnDecrementar" runat="server" CssClass="btn" Text="- Decrementar (PostBack)" OnClick="btnDecrementar_Click" />
            <asp:Button ID="btnZerar" runat="server" CssClass="btn btn-danger" Text="Zerar" OnClick="btnZerar_Click" />
        </div>

        <div class="stat-box">
            <div><strong>Ciclos de Vida Executados:</strong> <asp:Label ID="lblCiclos" runat="server" Text="1" /></div>
            <div><strong>Momento do PostBack:</strong> <asp:Label ID="lblHora" runat="server" /></div>
            <div><strong>Estado do IsPostBack:</strong> <asp:Label ID="lblIsPostBack" runat="server" /></div>
            <div><strong>Sobrecarga do __VIEWSTATE:</strong> Presente no campo oculto do DOM gerando dezenas de KB a cada clique.</div>
        </div>
    </div>

    <div class="card">
        <div class="card-header">Inspecao do Campo Oculto __VIEWSTATE</div>
        <p style="color: #64748b; font-size: 0.9rem;">
            No Web Forms, o estado entre cliques e preservado em uma string Base64 enviada ao navegador. Abra as Ferramentas de Desenvolvedor (F12) e inspecione o elemento &lt;input type="hidden" name="__VIEWSTATE"&gt;.
        </p>
    </div>
</asp:Content>
