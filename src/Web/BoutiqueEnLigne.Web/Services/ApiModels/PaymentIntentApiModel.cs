namespace BoutiqueEnLigne.Web.Services.ApiModels;

public class PaymentIntentApiModel
{
    public string ClientSecret { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
