using System;
using System.Web.UI;

namespace WebFormsLegadoDemo
{
    public partial class Default : Page
    {
        private int Contador
        {
            get
            {
                if (ViewState["Contador"] == null)
                {
                    ViewState["Contador"] = 0;
                }
                return (int)ViewState["Contador"];
            }
            set
            {
                ViewState["Contador"] = value;
            }
        }

        private int Ciclos
        {
            get
            {
                if (ViewState["Ciclos"] == null)
                {
                    ViewState["Ciclos"] = 1;
                }
                return (int)ViewState["Ciclos"];
            }
            set
            {
                ViewState["Ciclos"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                Ciclos++;
            }

            lblContador.Text = Contador.ToString();
            lblCiclos.Text = Ciclos.ToString();
            lblHora.Text = DateTime.UtcNow.ToString("HH:mm:ss.fff") + " UTC";
            lblIsPostBack.Text = IsPostBack ? "True (Requisicao HTTP POST)" : "False (Primeira Carga HTTP GET)";
        }

        protected void btnIncrementar_Click(object sender, EventArgs e)
        {
            Contador++;
            lblContador.Text = Contador.ToString();
        }

        protected void btnDecrementar_Click(object sender, EventArgs e)
        {
            Contador--;
            lblContador.Text = Contador.ToString();
        }

        protected void btnZerar_Click(object sender, EventArgs e)
        {
            Contador = 0;
            lblContador.Text = Contador.ToString();
        }
    }
}
