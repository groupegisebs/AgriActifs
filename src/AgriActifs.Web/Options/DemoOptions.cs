namespace AgriActifs.Web.Options;

public class DemoOptions
{
    public const string SectionName = "Demo";

    /// <summary>Affiche le bandeau rouge MODE DÉMO et le formulaire promoteur.</summary>
    public bool Enabled { get; set; } = true;

    public string PromoterName { get; set; } = "Équipe AgriActifs / GISE";
    /// <summary>Affiché sur le formulaire ; l'envoi réel cible toujours CEO@GISEBS.COM.</summary>
    public string PromoterEmail { get; set; } = "CEO@GISEBS.COM";
    public string? PromoterPhone { get; set; }
}
