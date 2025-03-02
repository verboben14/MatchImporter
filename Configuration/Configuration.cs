using MatchImporter.Constants;
using MatchImporter.Models;
using Microsoft.Extensions.Configuration;

namespace MatchImporter.Configuration
{
  public static class Configuration
  {
    public static string? BaseUrl { get; set; }
    public static string? LeagueTableEndpointExtension { get; set; }
    public static string LeagueTableEndpoint => $"{BaseUrl}{LeagueTableEndpointExtension!.Replace(LeagueConstants.SeasonIdUrlParameterName, SeasonId.ToString())}";
    public static int SeasonId { get; set; }
    public static List<Team>? Teams { get; set; }
    public static GoogleApiCredentials? GoogleApiCredentials { get; set; }

    public static void Initialize(IConfiguration configuration)
    {
      BaseUrl = configuration.GetRequiredSection(nameof(BaseUrl)).Value!;
      LeagueTableEndpointExtension = configuration.GetRequiredSection(nameof(LeagueTableEndpointExtension)).Value!;
      SeasonId = int.Parse(configuration.GetRequiredSection(nameof(SeasonId)).Value!);
      Teams = configuration.GetSection(nameof(Teams)).Get<List<Team>>()!;
      GoogleApiCredentials = configuration.GetSection(nameof(GoogleApiCredentials)).Get<GoogleApiCredentials>()!;
    }
  }
}