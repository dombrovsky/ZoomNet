using Pathoschild.Http.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZoomNet.Models;

namespace ZoomNet.Resources
{
	/// <summary>
	/// Allows you to view various metrics.
	/// </summary>
	/// <remarks>
	/// See <a href="https://marketplace.zoom.us/docs/api-reference/zoom-api/reports">Zoom documentation</a> for more information.
	/// </remarks>
	public class Reports : IReports
	{
		private readonly IClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="Reports" /> class.
		/// </summary>
		/// <param name="client">The HTTP client.</param>
		internal Reports(IClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Get a list of participants from past meetings with two or more participants. To see a list of participants for meetings with one participant use Dashboards.GetMeetingParticipantsAsync.
		/// </summary>
		/// <param name="meetingId">The meeting ID or meeting UUID. If given the meeting ID it will take the last meeting instance.</param>
		/// <param name="pageSize">The number of records returned within a single API call.</param>
		/// <param name="pageToken">
		/// The next page token is used to paginate through large result sets.
		/// A next page token will be returned whenever the set of available results exceeds the current page size.
		/// The expiration period for this token is 15 minutes.
		/// </param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An array of <see cref="ReportParticipant">participants</see>.
		/// </returns>
		public Task<PaginatedResponseWithToken<ReportMeetingParticipant>> GetMeetingParticipantsAsync(string meetingId, int pageSize = 30, string pageToken = null, CancellationToken cancellationToken = default)
		{
			VerifyPageSize(pageSize);

			return _client
				.GetAsync($"report/meetings/{meetingId}/participants")
				.WithArgument("include_fields", "registrant_id")
				.WithArgument("page_size", pageSize)
				.WithArgument("next_page_token", pageToken)
				.WithCancellationToken(cancellationToken)
				.AsPaginatedResponseWithToken<ReportMeetingParticipant>("participants");
		}

		/// <inheritdoc/>
		public Task<PaginatedResponseWithToken<PastMeeting>> GetMeetingsAsync(string userId, DateTime from, DateTime to, ReportMeetingType type, int pageSize = 30, string pageToken = null, CancellationToken cancellationToken = default)
		{
			VerifyReportDatesRange(from, to);
			VerifyPageSize(pageSize);

			return _client
				.GetAsync($"report/users/{userId}/meetings")
				.WithArgument("from", from.ToZoomFormat(dateOnly: true))
				.WithArgument("to", to.ToZoomFormat(dateOnly: true))
				.WithArgument("type", type.ToEnumString())
				.WithArgument("page_size", pageSize)
				.WithArgument("next_page_token", pageToken)
				.WithCancellationToken(cancellationToken)
				.AsPaginatedResponseWithToken<PastMeeting>("meetings");
		}

		/// <inheritdoc/>
		public Task<PaginatedResponseWithToken<ReportHost>> GetHostsAsync(DateTime from, DateTime to, ReportHostType type = ReportHostType.Active, int pageSize = 30, string pageToken = null, CancellationToken cancellationToken = default)
		{
			VerifyReportDatesRange(from, to);
			VerifyPageSize(pageSize);

			return _client
				.GetAsync("report/users")
				.WithArgument("from", from.ToZoomFormat(dateOnly: true))
				.WithArgument("to", to.ToZoomFormat(dateOnly: true))
				.WithArgument("type", type.ToEnumString())
				.WithArgument("page_size", pageSize)
				.WithArgument("next_page_token", pageToken)
				.WithCancellationToken(cancellationToken)
				.AsPaginatedResponseWithToken<ReportHost>("users");
		}

		private static void VerifyPageSize(int pageSize)
		{
			if (pageSize < 1 || pageSize > 300)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 300");
			}
		}

		private static void VerifyReportDatesRange(DateTime from, DateTime to)
		{
			if (to < from)
			{
				throw new ArgumentOutOfRangeException(nameof(to), $"Should be greater then or equal to {nameof(from)}.");
			}

			if (to - from > TimeSpan.FromDays(30))
			{
				throw new ArgumentOutOfRangeException(nameof(to), "The date range should not exceed one month.");
			}
		}
	}
}
