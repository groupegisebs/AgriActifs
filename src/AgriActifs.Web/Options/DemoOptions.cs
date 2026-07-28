namespace AgriActifs.Web.Options;

public class DemoOptions
{
    public const string SectionName = "Demo";

    /// <summary>Affiche le bandeau rouge MODE DÉMO et le formulaire promoteur.</summary>
    public bool Enabled { get; set; } = true;

    public string PromoterName { get; set; } = "Équipe AgriActifs / GISE";
    /// <summary>Affiché sur le formulaire ; l'envoi réel cible toujours ceo@gisebs.com.</summary>
    public string PromoterEmail { get; set; } = "ceo@gisebs.com";
    public string? PromoterPhone { get; set; }
}
