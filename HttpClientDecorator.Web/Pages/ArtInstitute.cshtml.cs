using WebSpark.HttpClientUtility.RequestResult;

namespace HttpClientDecorator.Web.Pages;

public class ArtInstituteModel : PageModel
{
    private readonly ILogger<ArtInstituteModel> _logger;
    private readonly IHttpRequestResultService _service;
    public HttpRequestResult<ArtWorksResponse> ArtResponse { get; set; } = default!;
    public ArtWorksResponse ArtWorksResponse { get; set; } = new ArtWorksResponse();
    public ArtList ArtList { get; set; } = new ArtList();
    public ArtInstituteModel(ILogger<ArtInstituteModel> logger, IHttpRequestResultService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }
    public async Task OnGet(CancellationToken ct = default)
    {
        ArtResponse = new HttpRequestResult<ArtWorksResponse>();

        if (ArtResponse == null)
        {
            _logger.LogError("artResponse is null");
            throw new Exception("artResponse is null");
        }
        ArtResponse.CacheDurationMinutes = 500;
        ArtResponse.RequestPath = "https://api.artic.edu/api/v1/artworks/search?query[term][is_public_domain]=true&limit=20&fields=id,title,image_id,artist_title,material_titles&q=impressionism+oil paint";
        ArtResponse = await _service.HttpSendRequestResultAsync(ArtResponse, ct: ct).ConfigureAwait(false);

        if (_service == null)
        {
            _logger.LogError("_service is null");
            throw new NullReferenceException(nameof(_service));
        }

        if (ArtResponse?.ResponseResults is null)
        {
            ArtWorksResponse = new ArtWorksResponse();
            _logger.LogError("jokeResult.ResponseResults is null");
        }
        else
        {
            ArtWorksResponse = ArtResponse.ResponseResults;
            if (ArtWorksResponse.data != null)
            {
                foreach (var item in ArtWorksResponse.data)
                {
                    if (item != null)
                    {
                        ArtWork artWork = new();
                        artWork.id = item.id.ToString();
                        artWork.title = item.title;
                        artWork.image_id = item.image_id;
                        artWork.artist_title = item.artist_title;
                        ArtList.Add(artWork);
                    }
                }
            }
        }
    }
}


public class ArtList
{
    public string Title { get; set; } = string.Empty;
    public List<ArtWork> list = [];
    public void Add(ArtWork artWork)
    {
        list.Add(artWork);
    }

}
public class ArtWork
{
    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string image_id { get; set; } = string.Empty;
    public string artist_title { get; set; } = string.Empty;
    public string[] material_titles { get; set; } = Array.Empty<string>();
    public string ImageUrl
    {
        get
        {
            return $"https://www.artic.edu/iiif/2/{image_id}/full/843,/0/default.jpg";
        }
    }
}




public class ArtWorksResponse
{
    public Pagination? pagination { get; set; }
    public Datum[]? data { get; set; }
    public Info? info { get; set; }
    public Config? config { get; set; }
}

public class Color
{
    public int h { get; set; }
    public int l { get; set; }
    public int s { get; set; }
    public double percentage { get; set; }
    public int population { get; set; }
}

public class Config
{
    public string iiif_url { get; set; } = string.Empty;
    public string website_url { get; set; } = string.Empty;
}

public class Contexts
{
    public List<string> groupings { get; set; } = new List<string>();
}

public class Datum
{
    public int id { get; set; }
    public string api_model { get; set; } = string.Empty;
    public string api_link { get; set; } = string.Empty;
    public bool is_boosted { get; set; }
    public string title { get; set; } = string.Empty;
    public List<object> alt_titles { get; set; } = new List<object>();
    public Thumbnail thumbnail { get; set; } = new Thumbnail();
    public string main_reference_number { get; set; } = string.Empty;
    public bool has_not_been_viewed_much { get; set; }
    public object boost_rank { get; set; } = new object();
    public int date_start { get; set; }
    public int date_end { get; set; }
    public string date_display { get; set; } = string.Empty;
    public string date_qualifier_title { get; set; } = string.Empty;
    public int? date_qualifier_id { get; set; }
    public string artist_display { get; set; } = string.Empty;
    public string place_of_origin { get; set; } = string.Empty;
    public string dimensions { get; set; } = string.Empty;
    public List<DimensionsDetail> dimensions_detail { get; set; } = new List<DimensionsDetail>();
    public string medium_display { get; set; } = string.Empty;
    public string inscriptions { get; set; } = string.Empty;
    public string credit_line { get; set; } = string.Empty;
    public string catalogue_display { get; set; } = string.Empty;
    public string publication_history { get; set; } = string.Empty;
    public object exhibition_history { get; set; } = new object();
    public string provenance_text { get; set; } = string.Empty;
    public object edition { get; set; } = new object();
    public string publishing_verification_level { get; set; } = string.Empty;
    public int internal_department_id { get; set; }
    public int? fiscal_year { get; set; }
    public object fiscal_year_deaccession { get; set; } = new object();
    public bool is_public_domain { get; set; }
    public bool is_zoomable { get; set; }
    public int max_zoom_window_size { get; set; }
    public object copyright_notice { get; set; } = new object();
    public bool has_multimedia_resources { get; set; }
    public bool has_educational_resources { get; set; }
    public bool has_advanced_imaging { get; set; }
    public double colorfulness { get; set; }
    public Color color { get; set; } = new Color();
    public object latitude { get; set; } = new object();
    public object longitude { get; set; } = new object();
    public object latlon { get; set; } = new object();
    public bool is_on_view { get; set; }
    public object on_loan_display { get; set; } = new object();
    public string gallery_title { get; set; } = string.Empty;
    public int? gallery_id { get; set; }
    public string artwork_type_title { get; set; } = string.Empty;
    public int artwork_type_id { get; set; }
    public string department_title { get; set; } = string.Empty;
    public string department_id { get; set; } = string.Empty;
    public int? artist_id { get; set; }
    public string artist_title { get; set; } = string.Empty;
    public List<object> alt_artist_ids { get; set; } = new List<object>();
    public List<int> artist_ids { get; set; } = new List<int>();
    public List<string> artist_titles { get; set; } = new List<string>();
    public List<string> category_ids { get; set; } = new List<string>();
    public List<string> category_titles { get; set; } = new List<string>();
    public List<string> term_titles { get; set; } = new List<string>();
    public string style_id { get; set; } = string.Empty;
    public string style_title { get; set; } = string.Empty;
    public List<string> alt_style_ids { get; set; } = new List<string>();
    public List<string> style_ids { get; set; } = new List<string>();
    public List<string> style_titles { get; set; } = new List<string>();
    public string classification_id { get; set; } = string.Empty;
    public string classification_title { get; set; } = string.Empty;
    public List<string> alt_classification_ids { get; set; } = new List<string>();
    public List<string> classification_ids { get; set; } = new List<string>();
    public List<string> classification_titles { get; set; } = new List<string>();
    public string subject_id { get; set; } = string.Empty;
    public List<string> alt_subject_ids { get; set; } = new List<string>();
    public List<string> subject_ids { get; set; } = new List<string>();
    public List<string> subject_titles { get; set; } = new List<string>();
    public string material_id { get; set; } = string.Empty;
    public List<string> alt_material_ids { get; set; } = new List<string>();
    public List<string> material_ids { get; set; } = new List<string>();
    public List<string> material_titles { get; set; } = new List<string>();
    public string technique_id { get; set; } = string.Empty;
    public List<string> alt_technique_ids { get; set; } = new List<string>();
    public List<string> technique_ids { get; set; } = new List<string>();
    public List<string> technique_titles { get; set; } = new List<string>();
    public List<object> theme_titles { get; set; } = new List<object>();
    public string image_id { get; set; } = string.Empty;
    public List<string> alt_image_ids { get; set; } = new List<string>();
    public List<object> document_ids { get; set; } = new List<object>();
    public List<object> sound_ids { get; set; } = new List<object>();
    public List<object> video_ids { get; set; } = new List<object>();
    public List<object> text_ids { get; set; } = new List<object>();
    public List<object> section_ids { get; set; } = new List<object>();
    public List<object> section_titles { get; set; } = new List<object>();
    public List<object> site_ids { get; set; } = new List<object>();
    public List<SuggestAutocompleteAll> suggest_autocomplete_all { get; set; } = new List<SuggestAutocompleteAll>();
    public DateTime source_updated_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime timestamp { get; set; }
}

public class DimensionsDetail
{
    public double depth_cm { get; set; }
    public double depth_in { get; set; }
    public double width_cm { get; set; }
    public double width_in { get; set; }
    public double height_cm { get; set; }
    public double height_in { get; set; }
    public int diameter_cm { get; set; }
    public int diameter_in { get; set; }
    public string clarification { get; set; } = string.Empty;
}

public class Info
{
    public string license_text { get; set; } = string.Empty;
    public List<string> license_links { get; set; } = new List<string>();
    public string version { get; set; } = string.Empty;
}

public class Pagination
{
    public int total { get; set; }
    public int limit { get; set; }
    public int offset { get; set; }
    public int total_pages { get; set; }
    public int current_page { get; set; }
    public string next_url { get; set; } = string.Empty;
}


public class SuggestAutocompleteAll
{
    public List<string> input { get; set; } = new List<string>();
    public Contexts contexts { get; set; } = new Contexts();
    public int? weight { get; set; }
}

public class Thumbnail
{
    public string lqip { get; set; } = string.Empty;
    public int width { get; set; }
    public int height { get; set; }
    public string alt_text { get; set; } = string.Empty;
}

