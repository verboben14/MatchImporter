using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MatchImporter.Constants;
using MatchImporter.Models;

namespace MatchImporter.Services
{
  public static class GoogleCalendarService
  {
    public static async Task AddMatchesToCalendar(string calendarName, List<Match> matches)
    {
      var clientSecrets = new ClientSecrets
      {
        ClientId = Configuration.Configuration.GoogleApiCredentials!.ClientId,
        ClientSecret = Configuration.Configuration.GoogleApiCredentials.ClientSecret
      };
      UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                [CalendarService.Scope.Calendar],
                Configuration.Configuration.GoogleApiCredentials.Email,
                CancellationToken.None,
                new FileDataStore("token.json", true));
      var service = new CalendarService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = LeagueConstants.ApplicationName,
      });

      var batchRequest = new BatchRequest(service);
      foreach (var match in matches)
      {
        // Create a new event
        var matchEvent = new Event()
        {
          Summary = match.MatchString,
          Location = match.Field,
          Start = new EventDateTime()
          {
            DateTimeDateTimeOffset = match.Date,
            TimeZone = LeagueConstants.IanaTimeZoneName,
          },
          End = new EventDateTime()
          {
            DateTimeDateTimeOffset = match.Date.AddHours(1),
            TimeZone = LeagueConstants.IanaTimeZoneName,
          },
        };

        CalendarListResource.ListRequest request = service.CalendarList.List();
        CalendarList calendarList = request.Execute();
        string calendarId = calendarList.Items.Single(x => x.Summary == calendarName).Id;

        var insertRequest = service.Events.Insert(matchEvent, calendarId);

        batchRequest.Queue<Event>(
          insertRequest,
          (content, error, index, message) =>
          {
            if (error == null)
            {
              Console.WriteLine($"Event {index} created: {content.HtmlLink}");
            }
            else
            {
              Console.WriteLine($"Error creating event {index}: {error.Message}");
            }
          }
        );
      }
      await batchRequest.ExecuteAsync();
    }
  }
}
