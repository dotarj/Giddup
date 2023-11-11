// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure;
using Giddup.Infrastructure.PullRequests;
using Giddup.Infrastructure.PullRequests.QueryModel.Models;
using Microsoft.EntityFrameworkCore;

namespace Giddup.Presentation.Projections;

public class PullRequestProjectionWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PullRequestProjectionWorker> _logger;

    public PullRequestProjectionWorker(IConfiguration configuration, ILogger<PullRequestProjectionWorker> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var dbContext = CreateDbContext(_configuration);

        var offset = await GetOffset(dbContext);

        while (!stoppingToken.IsCancellationRequested)
        {
            var events = await GetEvents(offset.Value, dbContext, stoppingToken);

            foreach (var @event in events)
            {
                _logger.LogInformation($"{DateTimeOffset.Now} processing event '{@event.Event.GetType().Name}' for aggregate ID {@event.AggregateId}.");

                var pullRequest = await GetPullRequest(dbContext, @event.AggregateId, stoppingToken);

                _ = @event.Event switch
                {
                    AbandonedEvent => Abandoned(pullRequest),
                    ApprovedEvent abandonedEvent => Approved(abandonedEvent, pullRequest),
                    ApprovedWithSuggestionsEvent approvedWithSuggestionsEvent => ApprovedWithSuggestions(approvedWithSuggestionsEvent, pullRequest),
                    AutoCompleteSetEvent => AutoCompleteSet(pullRequest),
                    AutoCompleteCancelledEvent => AutoCompleteCancelled(pullRequest),
                    CompletedEvent => Completed(pullRequest),
                    CreatedEvent createdEvent => Created(createdEvent, pullRequest),
                    DescriptionChangedEvent descriptionChangedEvent => DescriptionChanged(descriptionChangedEvent, pullRequest),
                    FeedbackResetEvent feedbackResetEvent => FeedbackReset(feedbackResetEvent, pullRequest),
                    OptionalReviewerAddedEvent optionalReviewerAddedEvent => OptionalReviewerAdded(optionalReviewerAddedEvent, pullRequest),
                    ReactivatedEvent reactivatedEvent => Reactivated(reactivatedEvent, pullRequest),
                    RejectedEvent rejectedEvent => Rejected(rejectedEvent, pullRequest),
                    RequiredReviewerAddedEvent requiredReviewerAddedEvent => RequiredReviewerAdded(requiredReviewerAddedEvent, pullRequest),
                    ReviewerMadeOptionalEvent reviewerMadeOptionalEvent => ReviewerMadeOptional(reviewerMadeOptionalEvent, pullRequest),
                    ReviewerMadeRequiredEvent reviewerMadeRequiredEvent => ReviewerMadeRequired(reviewerMadeRequiredEvent, pullRequest),
                    ReviewerRemovedEvent reviewerRemovedEvent => ReviewerRemoved(reviewerRemovedEvent, pullRequest),
                    TargetBranchChangedEvent targetBranchChangedEvent => TargetBranchChanged(targetBranchChangedEvent, pullRequest),
                    TitleChangedEvent titleChangedEvent => TitleChanged(titleChangedEvent, pullRequest),
                    WaitingForAuthorEvent waitingForAuthorEvent => WaitingForAuthor(waitingForAuthorEvent, pullRequest),
                    WorkItemLinkedEvent workItemLinkedEvent => await WorkItemLinked(workItemLinkedEvent, pullRequest, dbContext),
                    WorkItemRemovedEvent workItemRemovedEvent => WorkItemRemoved(workItemRemovedEvent, pullRequest),

                    _ => throw new InvalidOperationException($"Event '{@event.Event.GetType().FullName}' not supported.")
                };

                offset.Value = @event.Offset;

                _ = await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation($"{DateTimeOffset.Now} processed event '{@event.Event.GetType().Name}' for aggregate ID {@event.AggregateId}.");

            }

            await Task.Delay(100, stoppingToken);
        }
    }

    private static PullRequest Abandoned(PullRequest pullRequest)
        => SetStatus(PullRequestStatus.Abandoned, pullRequest);

    private static PullRequest Approved(ApprovedEvent @event, PullRequest pullRequest)
        => SetReviewerFeedback(@event.ReviewerId, ReviewerFeedback.Approved, pullRequest);

    private static PullRequest ApprovedWithSuggestions(ApprovedWithSuggestionsEvent @event, PullRequest pullRequest)
        => SetReviewerFeedback(@event.ReviewerId, ReviewerFeedback.Approved, pullRequest);

    private static PullRequest AutoCompleteSet(PullRequest pullRequest)
        => SetAutoCompleteMode(AutoCompleteMode.Enabled, pullRequest);

    private static PullRequest AutoCompleteCancelled(PullRequest pullRequest)
        => SetAutoCompleteMode(AutoCompleteMode.Enabled, pullRequest);

    private static PullRequest Completed(PullRequest pullRequest)
        => SetStatus(PullRequestStatus.Completed, pullRequest);

    private static PullRequest Created(CreatedEvent @event, PullRequest pullRequest)
    {
        pullRequest.OwnerId = @event.OwnerId;
        pullRequest.SourceBranch = @event.SourceBranch.ToString();
        pullRequest.TargetBranch = @event.TargetBranch.ToString();
        pullRequest.Title = @event.Title.ToString();

        return pullRequest;
    }

    private static PullRequest DescriptionChanged(DescriptionChangedEvent @event, PullRequest pullRequest)
    {
        pullRequest.Description = @event.Description;

        return pullRequest;
    }

    private static PullRequest FeedbackReset(FeedbackResetEvent @event, PullRequest pullRequest)
        => SetReviewerFeedback(@event.ReviewerId, ReviewerFeedback.None, pullRequest);

    private static PullRequest OptionalReviewerAdded(OptionalReviewerAddedEvent @event, PullRequest pullRequest)
    {
        pullRequest.OptionalReviewers.Add(new OptionalReviewer
        {
            UserId = @event.ReviewerId
        });

        return pullRequest;
    }

    private static PullRequest Reactivated(ReactivatedEvent @event, PullRequest pullRequest)
        => SetStatus(PullRequestStatus.Active, pullRequest);

    private static PullRequest Rejected(RejectedEvent @event, PullRequest pullRequest)
        => SetReviewerFeedback(@event.ReviewerId, ReviewerFeedback.Rejected, pullRequest);

    private static PullRequest RequiredReviewerAdded(RequiredReviewerAddedEvent @event, PullRequest pullRequest)
    {
        pullRequest.RequiredReviewers.Add(new RequiredReviewer
        {
            UserId = @event.ReviewerId
        });

        return pullRequest;
    }

    private static PullRequest ReviewerMadeOptional(ReviewerMadeOptionalEvent @event, PullRequest pullRequest)
    {
        var requiredReviewer = pullRequest.RequiredReviewers.FirstOrDefault(requiredReviewer => requiredReviewer.UserId == @event.ReviewerId);

        if (requiredReviewer == null)
        {
            throw new InvalidOperationException();
        }

        _ = pullRequest.RequiredReviewers.Remove(requiredReviewer);

        pullRequest.OptionalReviewers.Add(new OptionalReviewer
        {
            UserId = requiredReviewer.UserId,
            Feedback = requiredReviewer.Feedback
        });

        return pullRequest;
    }

    private static PullRequest ReviewerMadeRequired(ReviewerMadeRequiredEvent @event, PullRequest pullRequest)
    {
        var optionalReviewer = pullRequest.OptionalReviewers.FirstOrDefault(optionalReviewer => optionalReviewer.UserId == @event.ReviewerId);

        if (optionalReviewer == null)
        {
            throw new InvalidOperationException();
        }

        _ = pullRequest.OptionalReviewers.Remove(optionalReviewer);

        pullRequest.RequiredReviewers.Add(new RequiredReviewer
        {
            UserId = optionalReviewer.UserId,
            Feedback = optionalReviewer.Feedback
        });

        return pullRequest;
    }

    private static PullRequest ReviewerRemoved(ReviewerRemovedEvent @event, PullRequest pullRequest)
    {
        var optionalReviewer = pullRequest.OptionalReviewers.FirstOrDefault(optionalReviewer => optionalReviewer.UserId == @event.ReviewerId);

        if (optionalReviewer != null)
        {
            _ = pullRequest.OptionalReviewers.Remove(optionalReviewer);
        }

        var requiredReviewer = pullRequest.RequiredReviewers.FirstOrDefault(requiredReviewer => requiredReviewer.UserId == @event.ReviewerId);

        if (requiredReviewer != null)
        {
            _ = pullRequest.RequiredReviewers.Remove(requiredReviewer);
        }

        return pullRequest;
    }

    private static PullRequest TargetBranchChanged(TargetBranchChangedEvent @event, PullRequest pullRequest)
    {
        pullRequest.TargetBranch = @event.TargetBranch.ToString();

        return pullRequest;
    }

    private static PullRequest TitleChanged(TitleChangedEvent @event, PullRequest pullRequest)
    {
        pullRequest.Title = @event.Title.ToString();

        return pullRequest;
    }

    private static PullRequest WaitingForAuthor(WaitingForAuthorEvent @event, PullRequest pullRequest)
        => SetReviewerFeedback(@event.ReviewerId, ReviewerFeedback.WaitingForAuthor, pullRequest);

    private static async Task<PullRequest> WorkItemLinked(WorkItemLinkedEvent @event, PullRequest pullRequest, GiddupDbContext dbContext)
    {
        var workItem = await dbContext.WorkItems.FirstOrDefaultAsync(workItem => workItem.Id == @event.WorkItemId);

        if (workItem == null)
        {
            throw new InvalidOperationException();
        }

        pullRequest.WorkItems.Add(workItem);

        return pullRequest;
    }

    private static PullRequest WorkItemRemoved(WorkItemRemovedEvent @event, PullRequest pullRequest)
    {
        var workItem = pullRequest.WorkItems.FirstOrDefault(workItem => workItem.Id == @event.WorkItemId);

        if (workItem != null)
        {
            _ = pullRequest.WorkItems.Remove(workItem);
        }

        return pullRequest;
    }

    private static PullRequest SetStatus(PullRequestStatus status, PullRequest pullRequest)
    {
        pullRequest.Status = status;

        return pullRequest;
    }

    private static PullRequest SetReviewerFeedback(Guid userId, ReviewerFeedback feedback, PullRequest pullRequest)
    {
        var optionalReviewer = pullRequest.OptionalReviewers
            .FirstOrDefault(reviewer => reviewer.UserId == userId);

        if (optionalReviewer != null)
        {
            optionalReviewer.Feedback = feedback;
        }

        var requiredReviewer = pullRequest.RequiredReviewers
            .FirstOrDefault(reviewer => reviewer.UserId == userId);

        if (requiredReviewer != null)
        {
            requiredReviewer.Feedback = feedback;
        }

        return pullRequest;
    }

    private static PullRequest SetAutoCompleteMode(AutoCompleteMode autoCompleteMode, PullRequest pullRequest)
    {
        pullRequest.AutoCompleteMode = autoCompleteMode;

        return pullRequest;
    }

    private static async Task<EventProjectionOffset> GetOffset(GiddupDbContext dbContext)
    {
        var offset = await dbContext.EventProjectionOffsets
            .Where(offset => offset.AggregateType == nameof(PullRequest))
            .FirstOrDefaultAsync();

        if (offset == null)
        {
            offset = new EventProjectionOffset { AggregateType = nameof(PullRequest), Value = 0 };

            _ = dbContext.EventProjectionOffsets.Add(offset);
        }

        return offset;
    }

    private static async Task<List<(long Offset, Guid AggregateId, IPullRequestEvent Event)>> GetEvents(long offset, GiddupDbContext dbContext, CancellationToken cancellationToken)
    {
        var events = await dbContext.Events
            .Where(@event => @event.AggregateType == nameof(PullRequest))
            .Where(@event => @event.Offset > offset)
            .Select(@event => new { Offset = @event.Offset, @event.AggregateId, Event = PullRequestEventSerializer.Deserialize(@event) })
            .ToListAsync(cancellationToken: cancellationToken);

        return events
            .Select(@event => (@event.Offset, @event.AggregateId, @event.Event))
            .ToList();
    }

    private static async Task<PullRequest> GetPullRequest(GiddupDbContext dbContext, Guid pullRequestId, CancellationToken cancellationToken)
    {
        var pullRequest = await dbContext.PullRequests
            .Include(pullRequest => pullRequest.OptionalReviewers)
            .Include(pullRequest => pullRequest.RequiredReviewers)
            .FirstOrDefaultAsync(pullRequest => pullRequest.Id == pullRequestId, cancellationToken);

        if (pullRequest == null)
        {
            pullRequest = new PullRequest { Id = pullRequestId };

            _ = dbContext.PullRequests.Add(pullRequest);
        }

        return pullRequest;
    }

    private static GiddupDbContext CreateDbContext(IConfiguration configuration)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GiddupDbContext>();

        _ = optionsBuilder.UseNpgsql(configuration.GetConnectionString("Postgres"));

        return new GiddupDbContext(optionsBuilder.Options);
    }
}
