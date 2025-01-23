using MediatR;

namespace ClinicalTrialApp.Common.Domain;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
} 