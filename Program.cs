using HtmlAgilityPack;
using MatchImporter.Configuration;
using MatchImporter.Constants;
using MatchImporter.Models;
using MatchImporter.Services;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
  .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false, reloadOnChange: true)
  .AddUserSecrets<Program>();

var configuration = builder.Build();

Configuration.Initialize(configuration);

if (Configuration.Teams is null || Configuration.Teams.Count == 0)
{
  Console.WriteLine("No teams found in configuration.");
  return;
}

foreach (var team in Configuration.Teams)
{
  var htmlString = await HtmlDownloaderService.DownloadHtmlOfLeague(team.LeagueName);
  var htmlDocument = new HtmlDocument();
  htmlDocument.LoadHtml(htmlString);
  var htmlNodesWithTeamName = htmlDocument.DocumentNode.Descendants().Where(x => x.Name == "div" && x.ChildNodes.Count == 1 && x.InnerText.Contains(team.Name) && x.InnerText.Contains('-')).ToList();
  List<string> matches = [];
  List<HtmlNode> matchNodes = [];
  foreach (var htmlNode in htmlNodesWithTeamName)
  {
    var match = htmlNode.InnerText;
    if (matches.Contains(match))
    {
      continue;
    }
    matches.Add(match);
    matchNodes.Add(htmlNode);
  }
  List<Match> matchesToCalendar = [];
  foreach (var matchNode in matchNodes)
  {
    var parentNode = matchNode.ParentNode;
    var childrenNodes = parentNode.ChildNodes.Where(x => x.Name == "div");

    // Get the home and away teams
    var matchDiv = childrenNodes.Single(x => x.Attributes.Any(x => x.Name == "class" && x.Value.Contains(LeagueConstants.MatchHtmlClass)));
    var matchString = matchDiv.InnerText;
    var homeTeam = matchString.Split(LeagueConstants.MatchSeparator)[0].Trim();
    var awayTeam = matchString.Split(LeagueConstants.MatchSeparator)[1].Trim();

    // Get the match field
    var fieldDiv = childrenNodes.Single(x => x.Attributes.Any(x => x.Name == "class" && x.Value.Contains(LeagueConstants.FieldHtmlClass)));
    var field = fieldDiv.InnerText;

    // Get the match date
    var dateDiv = childrenNodes.Single(x => x.Attributes.Any(x => x.Name == "class" && x.Value.Contains(LeagueConstants.DateHtmlClass)));
    var date = DateTime.Parse(dateDiv.InnerText);

    var match = new Match
    {
      MatchString = matchString,
      HomeTeam = homeTeam,
      AwayTeam = awayTeam,
      Field = field,
      Date = date
    };
    matchesToCalendar.Add(match);
  }

  await GoogleCalendarService.AddMatchesToCalendar(team.CalendarName, matchesToCalendar);
}