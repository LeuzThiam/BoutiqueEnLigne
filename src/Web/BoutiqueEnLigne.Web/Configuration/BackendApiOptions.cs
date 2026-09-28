namespace BoutiqueEnLigne.Web.Configuration
{
    public class BackendApiOptions
    {
        public const string SectionName = "BackendApi";

        public string BaseUrl { get; set; } = "http://localhost:5000/";
    }
}
