# Command model design guidelines

The command model consists of different components, each having its own distinct responsibility (SRP, single responsibility principle). This section provides guidelines for developing command model components. The goal is to help developers ensure consistency and ease of use by providing a unified programming model.

The guidelines are organized as simple recommendations prefixed with the terms *Do*, *Consider*, *Avoid*, and *Do not*. There might be situations where good design requires that you violate these design guidelines. Such cases should be rare, and it is important that you have a clear and compelling reason for your decision.

## Table of contents<!-- omit in toc -->

- [Data architecture diagram](#data-architecture-diagram)
- [Commands](#commands)
- [Events](#events)
- [Errors](#errors)
- [State](#state)
- [GraphQL mutations and mutation controllers](#graphql-mutations-and-mutation-controllers)
- [Application services](#application-services)
- [State providers](#state-providers)
- [Command processors](#command-processors)
- [Event processors](#event-processors)

## Data architecture diagram

An overview of the command model components and the related data flow results in the following data architecture diagram:

```
                  │ PRESENTATION        │ APPLICATION CORE                          │ INFRASTRUCTURE      │


                  │            COMMAND MODEL                                        │                     │
                             ┌─────────────────────────────────────────────────────────────────┐
                             │                     ┌────────────────────(3)───────────┐ ┌──────┴──────┐
                  │          │          │          │              ┌─────────────┐   │ │ │    state    │   │
                             │                     │              │   command   │     └─┤  provider   │◄(4)─┐
                             │                     ▼          ┌──►│  processor  │       │             │     │   _.───────._
┌─────────────┐   │   ┌──────┴──────┐   │   ┌─────────────┐   │   │             │   │   └──────┬──────┘   │ │ .-           -.
│             │       │ controller  │       │ application │  (5)  └─────────────┘              │            └─┤-_         _-│
│   client    ├──(1)─►│ or graphql  ├──(2)─►│   service   ├───┘          ▲                     │              │  ~───────~  │
│             │   │   │  mutation   │   │   │             │       ┌──────┴──────┐   │          │          │ ┌►│    data     │
└─────────────┘       └──────┬──────┘       └──────┬──────┘       │   domain    │       ┌──────┴──────┐     │ `._  store  _.'
                             │                     │              │   service   │       │    event    │     │    "───────"
                  │          │          │          │              │             │   │ ┌►│  processor  ├─(7)─┘
                             │                     │              └─────────────┘     │ │             │
                             │                     └────────────────────(6)───────────┘ └──────┬──────┘
                  │          └─────────────────────────────────────────────────────────────────┘          │


                  │                     │                                           │                     │
``` 

*Data architecture diagram describing the data flow of the Giddup application command model.*

1. Client sends mutation to GraphQL mutation or mutation controller.
1. GraphQL mutation or mutation controller converts input into command and sends command to application service.
1. Application service retrieves aggregate state using state provider.
1. State provider retrieves current aggregate state.
1. Application service sends command and current state to command processor. Command processor evaluates business rules and returns event(s) if successful.
1. Application service sends events to events processor.
1. Event processor stores event(s) and/or current aggregate state in data store.

## Commands

> [!IMPORTANT]
> A command describes the intention to change something in the system. It reflects a business process as close as possible.

A command is processed by a command processor and contains all information required for command processing and the resulting event processing. For example, `AddOptionalReviewerCommand` contains the ID of the optional reviewer, `LinkWorkItemCommand` contains the ID of the work item and `CompletePullRequestCommand` requires no extra information for command processing.

:heavy_check_mark: DO use immutable records.

:heavy_check_mark: DO implement a common (aggregate specific) interface.

:heavy_check_mark: DO place the interface and implementations in application core (domain).

:heavy_check_mark: DO use descriptive names using a verb, a noun and the suffix `Command`.

For example, `AddOptionalReviewerCommand`, `LinkWorkItemCommand` or `CompletePullRequestCommand`.

:heavy_check_mark: DO add delegates to a command when processing requires external information (information beyond the scope of the current aggregate).

For example, `AddOptionalReviewerCommand` contains a delegate which can validate whether the given reviewer exists.

:x: DO NOT add information beyond the scope of the current command.

See [IPullRequestCommand.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/IPullRequestCommand.cs) for an example implementation.

## Events

> [!IMPORTANT]
> An event describes the fact that something has happened in the system.

An event is processed by an event processor and contains all information required for event processing. For example, `OptionalReviewerAddedEvent` contains the ID of the optional reviewer, `WorkItemLinkedEvent` contains the ID of the work item and `PullRequestCompletedEvent` requires no extra information for event processing.

:heavy_check_mark: DO use immutable records.

:heavy_check_mark: DO implement a common (aggregate specific) interface.

:heavy_check_mark: DO place the interface and implementations in application core (domain).

:heavy_check_mark: DO use descriptive names using a noun, a past tense verb and the suffix `Event`.

For example, `OptionalReviewerAddedEvent`, `WorkItemLinkedEvent` and `PullRequestCompletedEvent`.

See [IPullRequestEvent.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/IPullRequestEvent.cs) for an example implementation.

## Errors

> [!IMPORTANT]
> An error describes the fact that the processing of a command failed due to the current state of the aggregate.

:heavy_check_mark: DO use immutable records.

:heavy_check_mark: DO implement a common (aggregate specific) interface.

:heavy_check_mark: DO place the interface and implementations in application core (domain).

:heavy_check_mark: DO use descriptive names and the suffix `Error`.

:x: DO NOT use exceptions for errors.

For example, `InvalidBranchNameError`, `ReviewerNotFoundError` and `TargetBranchEqualsSourceBranchError`.

See [IPullRequestError.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/IPullRequestError.cs) for an example implementation.

## State
> [!IMPORTANT]
> State describes the current state of an aggregate. State is immutable and named using the aggregate name and the suffix `State`. State is defined in application core (domain), grouped by aggregate and implements a common (aggregate specific) interface, for example `IPullRequestState`.

There are two types of state, state for an existing aggregate and initial state for a new aggregate. State for an existing aggregate contains the current values of the aggregate which are retrieved from the data store. Initial state for a new aggregate contains the default values for the aggregate.

:heavy_check_mark: DO use immutable records.

:heavy_check_mark: DO implement a common (aggregate specific) interface.

:heavy_check_mark: DO place the interface and implementations in application core (domain).

:heavy_check_mark: DO use descriptive names and the suffix `State`.

For example, `PullRequestState` and `InitialPullRequestState`.

:x: DO NOT add information which is not relevant for the command processor.

See [IPullRequestState.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/IPullRequestState.cs) for an example implementation.

## GraphQL mutations and mutation controllers

> [!IMPORTANT]
> An GraphQL mutation or mutation controller receives requests, translates the input to commands, processes the commands using an application service and returns a result to the client.

:heavy_check_mark: DO add authorization attributes to GraphQL mutations and mutation controllers.

:heavy_check_mark: DO place implementation in presentation.
 
:x: DO NOT add business logic to GraphQL mutations and mutation controllers.

See [PullRequestMutations.cs](https://github.com/dotarj/Giddup/blob/feature/documentation/src/Giddup.Presentation.Api/Mutations/PullRequestMutations.cs) for an example implementation.

## Application services

> [!IMPORTANT]
> An application service orchestrates the processing of commands in the system.

Processing of commands in an application service involves the following steps:

1. Retrieve the current aggregate state using the state provider.
1. Process the command using the command processor.
1. Depending on the outcome:
    1. When a concurrency conflict error is returned, retry the command processing.
    1. When one or more other errors are returned, return the errors.
    1. When events are returned, process the events using the event processor.

:heavy_check_mark: DO place the interface and implementation in application core (application).

:heavy_check_mark: DO use a name using the aggregate name and the suffix `Service`.

:heavy_check_mark: CONSIDER using optimistic concurrency to prevent data corruption.

:x: DO NOT rely on external services, apart from the state provider, command processor and event processor.

:x: AVOID the use of third-party libraries (for example, MediatR or AutoMapper).

The use of third-party libraries in application core is often unnecessary. Deeply integrating third-party libraries in application core can lead to problems in the future when a third-party library is no longer maintained or otherwise incompatible with future versions the system.

See [PullRequestService.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Application/PullRequests/PullRequestService.cs) for an example implementation.

## State providers

> [!IMPORTANT]
> A state providers sole purpose is to retrieve the current state of an aggregate from the data store.

Depending on the architecture, retrieving the state from the data store could be one of these variations:

- Read the aggregate events from the data store and replay all events to rehydrate aggregate (current state).
- Read the current aggregate state from the data store.

:heavy_check_mark: DO place the interface in application core (application) and the implementation in infrastructure.

:heavy_check_mark: DO use a name using the aggregate name and the suffix `StateProvider`.

:heavy_check_mark: DO add a single public method `Provide`.

See [PullRequestStateProvider.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.Infrastructure/PullRequests/CommandModel/PullRequestStateProvider.cs) for an example implementation.

## Command processors

> [!IMPORTANT]
> A command processors sole purpose is to process all business rules using the given command and state.

Processing of a command can either result in errors or events. One or more errors if business rules prevent the change due to the current state of the aggregate, or zero or more events if business rules were successfully processed. When the current state of the aggregate equals the desired state of the aggregate (described by the command), processing of the command should be considered successful and zero events are returned (robustness principle).

:heavy_check_mark: DO place the implementation in application core (domain).

:heavy_check_mark: DO use a name using the aggregate name and the suffix `CommandProcessor`.

:heavy_check_mark: DO add a single public method `Process`.

:x: DO NOT add an interface to a command processor.

Command processors only contain pure code which does not need to be mocked.

:x: DO NOT rely on external services, apart from the delegates provided in commands.

:x: DO NOT add authorization logic to command processors.

Adding authorization logic to command processors leads to increased complexity and decreased reusability, since authorization is very specific per use case. For example, the `RemoveReviewerCommand` can be used through the API where the caller is either a user or an OAuth 2.0 client credentials client. User authorization is significantly different compared to OAuth 2.0 client credentials client. The `RemoveReviewerCommand` can even be called though, for example, a service bus when a user is removed from the system. In this case, there is no authorization. Authorization should be implemented on the edge, in presentation.

See [PullRequestCommandProcessor.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.ApplicationCore/Domain/PullRequests/PullRequestCommandProcessor.cs) for an example implementation.

## Event processors

> [!IMPORTANT]
> An event processors sole purpose is to update the data store using the given events.

Depending on the architecture, updating the data store could be one of these variations:

- Write the events to the data store. Other systems monitor the data store for incoming events and update read models accordingly (projection).
- Write the new state to the data store.
- Write the events and the new state to the data store.

During updating of the data store, an event processor can prevent data corruption by using optimistic concurrency. The version of the aggregate is checked while saving the events and/or the state to the data store. Depending on the architecture, the actual optimistic concurrency implementation can differ.

:heavy_check_mark: DO place the interface in application core (application) the implementation in infrastructure.

:heavy_check_mark: DO use a name using the aggregate name and the suffix `EventProcessor`.

:heavy_check_mark: DO add a single public method `Process`.

:heavy_check_mark: CONSIDER using optimistic concurrency to prevent data corruption.

:x: DO NOT add business logic to event processors.

Since all business rules have been processed by an command processor, an event processor can assume all events to be valid and update the data store accordingly.

See [PullRequestEventProcessor.cs](https://github.com/dotarj/Giddup/blob/master/src/Giddup.Infrastructure/PullRequests/CommandModel/PullRequestEventProcessor.cs) for an example implementation.
