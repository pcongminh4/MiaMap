namespace Domain.Places;

public sealed class ReportVote
{
    public int ReportId { get; private set; }
    public int UserId { get; private set; }
    public bool IsUpvote { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public ReportVote(int reportId, int userId, bool isUpvote)
    {
        ReportId = reportId;
        UserId = userId;
        IsUpvote = isUpvote;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private ReportVote() { }

    public void UpdateVote(bool isUpvote)
    {
        IsUpvote = isUpvote;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
