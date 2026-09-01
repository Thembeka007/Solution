using SharedKernel.Base;

namespace SharedKernel.Events;
public class MatchCompletedEvent : DomainEvent
{
    public Guid MatchId { get; set; }

    public MatchCompletedEvent(Guid matchId)
    {
        MatchId = matchId;
    }
}