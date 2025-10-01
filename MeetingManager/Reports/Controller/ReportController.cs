using MeetingManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeetingManager.Reports.Controller
{
    [Route("api/reports")]
    [ApiController]
    [Authorize(Roles = "Admin")]  // reports usually are admin-only
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReportController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/reports/meetings-summary?range=month
        [HttpGet("meetings-summary")]
        public async Task<IActionResult> GetMeetingsSummary([FromQuery] string? range)
        {
            range = string.IsNullOrWhiteSpace(range) ? "month" : range.ToLower();

            var query = _db.Meetings.Where(m => m.Status != "Canceled");

            if (range == "week")
            {
                var weekly = await query
                    .Select(m => new
                    {
                        m.StartTime,
                        Year = m.StartTime.Year,
                        Week = EF.Functions.DateDiffWeek(
                                    new DateTime(1900, 1, 1), // FIXED baseline
                                    m.StartTime
                               ) + 1
                    })
                    .GroupBy(x => new { x.Year, x.Week })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Week,
                        Count = g.Count(),
                        StartDate = g.Min(x => x.StartTime),
                        EndDate = g.Max(x => x.StartTime)
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Week)
                    .ToListAsync();

                return Ok(weekly.Select(d => new
                {
                    Period = $"{d.StartDate:yyyy-MM-dd} to {d.EndDate:yyyy-MM-dd}",
                    d.Count
                }));
            }
            else // default: month
            {
                var monthly = await query
                    .GroupBy(m => new { m.StartTime.Year, m.StartTime.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToListAsync();

                return Ok(monthly.Select(g => new
                {
                    Period = $"{g.Year}-{g.Month:D2}",
                    g.Count
                }));
            }
        }


        // GET: api/reports/most-used-rooms
        [HttpGet("most-used-rooms")]
        public async Task<IActionResult> GetMostUsedRooms()
        {
            var data = await _db.Meetings
                .Where(m => m.Status != "Canceled")
                .GroupBy(m => m.Room.RoomName)
                .Select(g => new
                {
                    RoomName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(r => r.Count)
                .Take(5)
                .ToListAsync();

            return Ok(data);
        }
    }
}
