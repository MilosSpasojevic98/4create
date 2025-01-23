using ClinicalTrialApp.Common.Domain;
using ClinicalTrialApp.Models;

namespace ClinicalTrialApp.Features.ClinicalTrials.Events;

public record TrialCreatedEvent : IDomainEvent
{
    public TrialCreatedEvent(ClinicalTrialMetadata trial)
    {
        Trial = trial;
        OccurredOn = DateTime.UtcNow;
    }

    public ClinicalTrialMetadata Trial { get; }
    public DateTime OccurredOn { get; }
} 