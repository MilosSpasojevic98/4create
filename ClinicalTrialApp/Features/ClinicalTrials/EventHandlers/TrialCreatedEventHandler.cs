using ClinicalTrialApp.Features.ClinicalTrials.Events;
using MediatR;

namespace ClinicalTrialApp.Features.ClinicalTrials.EventHandlers;

public class TrialCreatedEventHandler : INotificationHandler<TrialCreatedEvent>
{
    private readonly ILogger<TrialCreatedEventHandler> _logger;

    public TrialCreatedEventHandler(ILogger<TrialCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TrialCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Clinical trial created: {TrialId} - {Title} at {Timestamp}",
            notification.Trial.TrialId,
            notification.Trial.Title,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}