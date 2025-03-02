namespace MatchImporter.Models
{
  public class Match
  {
    public required string MatchString { get; set; }
    public required string HomeTeam { get; set; }
    public required string AwayTeam { get; set; }
    public required string Field { get; set; }
    public DateTime Date { get; set; }
  }
}
