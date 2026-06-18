using NetTopologySuite.Geometries;
using System.Linq;

namespace Domain.Places;

public sealed class Report
{
    private readonly List<ReportVote> _votes = [];

    public int Id { get; private set; }
    public int CreatedByUserId { get; private set; }
    public string ReportType { get; private set; } = string.Empty;
    public string? SubType { get; private set; }
    public Point Location { get; private set; } = new(0, 0) { SRID = 4326 };
    public string? Description { get; private set; }
    public int Upvotes { get; private set; }
    public int Downvotes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<ReportVote> Votes => _votes.AsReadOnly();

    public Report(
        int createdByUserId,
        string reportType,
        string? subType,
        double latitude,
        double longitude,
        string? description,
        int durationMinutes)
    {
        CreatedByUserId = createdByUserId;
        ReportType = reportType.Trim().ToLowerInvariant();
        SubType = string.IsNullOrWhiteSpace(subType) ? null : subType.Trim().ToLowerInvariant();
        Location = new Point(longitude, latitude) { SRID = 4326 };
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Upvotes = 0;
        Downvotes = 0;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = DateTime.UtcNow.AddMinutes(durationMinutes);
        IsActive = true;
    }

    private Report() { }

    public void RecordVote(int userId, bool isUpvote)
    {
        var existingVote = _votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote is null)
        {
            _votes.Add(new ReportVote(Id, userId, isUpvote));
        }
        else
        {
            existingVote.UpdateVote(isUpvote);
        }

        Upvotes = _votes.Count(v => v.IsUpvote);
        Downvotes = _votes.Count(v => !v.IsUpvote);

        if (Downvotes >= 5 && Downvotes > Upvotes)
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
