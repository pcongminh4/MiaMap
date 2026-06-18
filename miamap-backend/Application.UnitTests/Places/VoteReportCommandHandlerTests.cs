using Application.Common.Abstractions.Data;
using Application.Places.VoteReport;
using Domain.Places;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class VoteReportCommandHandlerTests
{
    /// <summary>
    /// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
    /// Tên test: Handle_Should_CreateNewVote_When_NoPreviousVoteExists
    /// Mô tả: Đảm bảo khi người dùng vote lần đầu tiên trên một báo cáo hoạt động, thực thể ReportAggregate tự thêm lượt vote mới,
    ///        tính toán lại điểm số Upvotes/Downvotes nội bộ và gọi SaveChangesAsync trên UnitOfWork.
    /// Đầu vào: VoteReportCommand với ReportId=1, UserId=2, IsUpvote=true. Thực thể Report ban đầu chưa có lượt vote nào.
    /// Kết quả kỳ vọng: Danh sách Votes của Report chứa 1 vote của UserId=2. Upvotes=1, Downvotes=0.
    ///                  UnitOfWork.SaveChangesAsync được gọi đúng 1 lần.
    /// </summary>
    [Fact]
    public async Task Handle_Should_CreateNewVote_When_NoPreviousVoteExists()
    {
        // Arrange
        var command = new VoteReportCommand(1, 2, true);
        var reportRepository = Substitute.For<IReportRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var report = new Report(1, "accident", null, 10.77, 106.69, "Tai nạn", 120);
        reportRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);

        var handler = new VoteReportCommandHandler(reportRepository, unitOfWork);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        report.Votes.Should().ContainSingle(v => v.UserId == 2 && v.IsUpvote);
        report.Upvotes.Should().Be(1);
        report.Downvotes.Should().Be(0);
        report.IsActive.Should().BeTrue();

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
    /// Tên test: Handle_Should_UpdateExistingVote_When_PreviousVoteExists
    /// Mô tả: Đảm bảo khi người dùng đã bình chọn trước đó và thực hiện bình chọn lại (đổi ý), thực thể ReportAggregate
    ///        sẽ cập nhật trực tiếp lượt bình chọn cũ thay vì tạo mới, đồng thời cập nhật lại tổng số vote tương ứng.
    /// Đầu vào: VoteReportCommand với ReportId=1, UserId=2, IsUpvote=false. Thực thể Report đã được seed sẵn 1 vote hợp lệ (IsUpvote=true).
    /// Kết quả kỳ vọng: Danh sách Votes vẫn chỉ có 1 phần tử của UserId=2 nhưng IsUpvote được đổi thành false. Upvotes=0, Downvotes=1.
    ///                  UnitOfWork.SaveChangesAsync được gọi đúng 1 lần.
    /// </summary>
    [Fact]
    public async Task Handle_Should_UpdateExistingVote_When_PreviousVoteExists()
    {
        // Arrange
        var command = new VoteReportCommand(1, 2, false);
        var reportRepository = Substitute.For<IReportRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var report = new Report(1, "accident", null, 10.77, 106.69, "Tai nạn", 120);
        report.RecordVote(2, true); // Seed trước 1 lượt upvote của UserId 2

        reportRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);

        var handler = new VoteReportCommandHandler(reportRepository, unitOfWork);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        report.Votes.Should().ContainSingle(v => v.UserId == 2 && !v.IsUpvote);
        report.Upvotes.Should().Be(0);
        report.Downvotes.Should().Be(1);
        report.IsActive.Should().BeTrue();

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
    /// Tên test: Handle_Should_DeactivateReport_When_DownvotesReach5AndExceedUpvotes
    /// Mô tả: Đảm bảo nếu số lượng downvote đạt từ 5 lượt trở lên và vượt quá số lượng upvote sau lượt vote mới,
    ///        thực thể ReportAggregate tự động chuyển trạng thái hoạt động (IsActive) thành false.
    /// Đầu vào: VoteReportCommand với ReportId=1, UserId=16, IsUpvote=false (gửi lượt downvote thứ 5).
    ///          Thực thể Report đã được seed sẵn 2 upvotes và 4 downvotes.
    /// Kết quả kỳ vọng: Report.IsActive trở thành false. Upvotes=2, Downvotes=5.
    ///                  UnitOfWork.SaveChangesAsync được gọi đúng 1 lần.
    /// </summary>
    [Fact]
    public async Task Handle_Should_DeactivateReport_When_DownvotesReach5AndExceedUpvotes()
    {
        // Arrange
        var command = new VoteReportCommand(1, 16, false);
        var reportRepository = Substitute.For<IReportRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var report = new Report(1, "accident", null, 10.77, 106.69, "Tai nạn", 120);
        
        // Seed 2 upvotes
        report.RecordVote(10, true);
        report.RecordVote(11, true);

        // Seed 4 downvotes
        report.RecordVote(12, false);
        report.RecordVote(13, false);
        report.RecordVote(14, false);
        report.RecordVote(15, false);

        reportRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);

        var handler = new VoteReportCommandHandler(reportRepository, unitOfWork);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        report.IsActive.Should().BeFalse();
        report.Upvotes.Should().Be(2);
        report.Downvotes.Should().Be(5);

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
