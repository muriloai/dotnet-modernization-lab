using System.Configuration;

namespace ConfigurationDemo.Models
{
    /// <summary>
    /// Demonstração de uma seção de configuração customizada no .NET Framework.
    /// Para criar configurações agrupadas e tipadas, era obrigatório herdar de ConfigurationSection
    /// e decorar cada propriedade com o atributo ConfigurationProperty, especificando nome do atributo XML,
    /// valor padrão e regras de obrigatoriedade.
    /// </summary>
    public class ConfiguracaoSmtpSection : ConfigurationSection
    {
        [ConfigurationProperty("servidor", IsRequired = true)]
        public string Servidor
        {
            get => (string)this["servidor"];
            set => this["servidor"] = value;
        }

        [ConfigurationProperty("porta", DefaultValue = 587, IsRequired = false)]
        public int Porta
        {
            get => (int)this["porta"];
            set => this["porta"] = value;
        }

        [ConfigurationProperty("habilitarSsl", DefaultValue = true, IsRequired = false)]
        public bool HabilitarSsl
        {
            get => (bool)this["habilitarSsl"];
            set => this["habilitarSsl"] = value;
        }

        [ConfigurationProperty("emailRemetente", IsRequired = true)]
        public string EmailRemetente
        {
            get => (string)this["emailRemetente"];
            set => this["emailRemetente"] = value;
        }
    }
}
