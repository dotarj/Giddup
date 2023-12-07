# Components

## State
> [!IMPORTANT]
> State describes the current state of an aggregate. State is immutable and named using the aggregate name and the suffix `State`. State is defined in application core (domain), grouped by aggregate and implements a common (aggregate specific) interface, for example `IPullRequestState`.

There are two types of state, state for an existing aggregate and initial state for a new aggregate. State for an existing aggregate contains the current values of the aggregate which are retrieved from the data store. Initial state for a new aggregate contains the default values for the aggregate.

See [IPullRequestState.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/IPullRequestState.cs) for an example implementation.

## Commands

> [!IMPORTANT]
> A command describes the intention to change something in the system. It reflects a business process as close as possible. A command is immutable and descriptively named using a verb, a noun and the suffix `Command`. Commands are defined in application core (domain), grouped by aggregate and implement a common (aggregate specific) interface, for example `IPullRequestCommand`.

A command contains all information required for command processing and the resulting event processing. For example, `AddOptionalReviewerCommand` contains the ID of the optional reviewer, `LinkWorkItemCommand` contains the ID of the work item and `CompletePullRequestCommand` requires no extra information for command processing.

When processing of a command requires external information (information beyond the scope of the current aggregate), delegates (`Func<>`) can be added to a command which can be used by the command processor. For example, `AddOptionalReviewerCommand` contains a delegate which can validate whether the given reviewer exists.

See [IPullRequestCommand.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/IPullRequestCommand.cs) for an example implementation.

## Events

> [!IMPORTANT]
> An event describes the fact that something has happened in the system. An event is immutable and descriptively named using a noun, a past tense verb and the suffix `Event`. Events are defined in application core (domain), grouped by aggregate and implement a common (aggregate specific) interface, for example `IPullRequestEvent`.

An event contains all information required for event processing. For example, `OptionalReviewerAddedEvent` contains the ID of the optional reviewer, `WorkItemLinkedEvent` contains the ID of the work item and `PullRequestCompletedEvent` requires no extra information for event processing.

See [IPullRequestEvent.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/IPullRequestEvent.cs) for an example implementation.

## Errors

> [!IMPORTANT]
> An error describes the fact that the processing of a command failed due to the current state of the aggregate. An error is immutable and descriptively named using the suffix `Error`. Errors are defined in application core (domain), grouped by aggregate and implement a common (aggregate specific) interface, for example `IPullRequestError`.

See [IPullRequestError.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/IPullRequestError.cs) for an example implementation.

## GraphQL mutations and mutation controllers

## Application services

> [!IMPORTANT]
> An application service orchestrates the processing of commands.

Optimistic concurrency!

1. Retrieve the current aggregate state using the state provider.
1. Process the command using the command processor.
1. Depending on the outcome:
   1. When a concurrency conflict error is returned, retry the command processing.
   1. When another error is returned, return the error.
   1. When events are returned, process the events using the event processor.

## State providers

> [!IMPORTANT]
> A state providers sole purpose is to retrieves the current state from the data store.

## Command processors

> [!IMPORTANT]
> A command processors sole purpose is to process all business rules using the given command and state. A command processor is static, has one public method (`Process`), does not rely on external services (apart from the delegates provided in commands) and is named using the aggregate name and the suffix `CommandProcessor`. Command processors are defined in application core (domain).

Processing of a command can either result in errors or events. One or more errors if business rules prevent the change due to the current state of the aggregate, or zero or more events if business rules were successfully processed. When the current state of the aggregate equals the desired state of the aggregate (described by the command), processing of the command should be considered successful and zero events are returned (robustness principle).

## Domain services

## Event processors

> [!IMPORTANT]
> An event processors sole purpose is to update the data store using the given events. An event processor has one public method (`Process`) and is named using the aggregate name and the suffix `EventProcessor`. Event processor interfaces are defined in application core (application) and implementations in infrastructure.

Since all business rules have been processed by an command processor, an event processor can assume all events to be valid and update the data store accordingly.

Depending on the architecture, updating the data store could be:

- Write the events to the data store. Other systems monitor the data store for incoming events and update read models accordingly (projection).
- Write the new state to the data store.
- Write the events and the new state to the data store.

During updating of the data store, an event processor prevents data corruption by using optimistic concurrency. The version of the aggregate is checked while saving the events and/or the state to the data store. Depending on the architecture, the actual optimistic concurrency implementation can differ.
