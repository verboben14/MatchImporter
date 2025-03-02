using MatchImporter.Constants;

namespace MatchImporter.Services
{
  public static class HtmlDownloaderService
  {
    public static async Task<string> DownloadHtmlOfLeague(string leagueName)
    {
      using (var client = new HttpClient())
      {
        var response = await client.GetAsync(Configuration.Configuration.LeagueTableEndpoint.Replace(LeagueConstants.LeagueNameUrlParameterName, leagueName));
        return await response.Content.ReadAsStringAsync();
      }
    }
  }
}
