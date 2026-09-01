using SharedKernel.Enums;

namespace Ingestion.Normalizers;

public class MatchStatusMapper
{
    public static MatchStatus Map(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return MatchStatus.Scheduled;

        return status.Trim().ToLower() switch
        {
            "scheduled" => MatchStatus.Scheduled,
            "timed" => MatchStatus.Scheduled,
            "ns" => MatchStatus.Scheduled,

            "live" => MatchStatus.Live,
            "inplay" => MatchStatus.Live,
            "in_play" => MatchStatus.Live,
            "1h" => MatchStatus.Live,
            "2h" => MatchStatus.Live,
            "paused" => MatchStatus.Live,

            "finished" => MatchStatus.Completed,
            "ft" => MatchStatus.Completed,
            "full_time" => MatchStatus.Completed,
            "ended" => MatchStatus.Completed,

            "postponed" => MatchStatus.Postponed,

            "cancelled" => MatchStatus.Cancelled,

            _ => MatchStatus.Scheduled
        };
    }
}
