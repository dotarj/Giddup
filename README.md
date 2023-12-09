# Giddup reference application

Giddup is a reference application using hexagonal architecture (ports and adapters architecture), domain-driven design (DDD), command query responsibility segregation (CQRS) and event sourcing (ES). It is written in C# using ASP.NET Core as web application framework and PostgreSQL as data store.

## Table of contents<!-- omit in toc -->

- [Getting started](#getting-started)
- [Functional overview](#functional-overview)
- [Technical overview](#technical-overview)
- [Acknowledgments](#acknowledgments)
- [References](#references)

## Getting started

Once you have cloned or downloaded the project you can run (and debug) the application using the provided Docker Compose file. The Docker Compose file can be started from Rider or Visual Studio, or you can use the Docker Compose CLI:

```Shell
docker-compose up --build
```

By running the Docker Compose file, an PostgreSQL instance is started and the solution is build and started. When the services have started, you can make HTTP requests using http://localhost:5000/swagger/ or GraphQL requests using http://localhost:5000/graphql/.

Sample HTTP requests of all available endpoints are provided in the file scripts/pull-requests.http. The HTTP requests in this file can be executed in Visual Studio Code using the REST Client extension by Huachao Mao [[1]](https://marketplace.visualstudio.com/items?itemName=humao.rest-client "REST Client").

## Functional overview

Giddup is based on the Azure DevOps pull request user interface, which is a fully task-driven user interface. Task-driven user interfaces usually provide much better user experience and are the cornerstone of CQRS, a task in the user interface translates directly to a command on the server side.

![Azure DevOps pull request user interface](docs/images/pull-request-overview.png)

*Azure DevOps pull request user interface (courtesy of Microsoft).*

There are all kinds of tasks that can be performed in the Azure DevOps pull request user interface: adding and removing reviewers, linking and removing work items, submitting and resetting reviewer feedback, completing, abandoning and reactivating the pull request and so on. Most of the tasks provided in the Azure DevOps pull request user interface are implemented in Giddup.

## Technical overview

### Hexagonal architecture (ports and adapters architecture)

The structure of the solution conforms to the hexagonal architecture (ports and adapters architecture) introduced by Alistair Cockburn [[2]](https://alistair.cockburn.us/hexagonal-architecture/ "Hexagonal architecture"). In this architecture three main components are identified, each with their respective responsibility:

* Application core
Application core is divided in the domain and application layer. Application core has no references to other projects in the solution and references to third party libraries should be avoided as much as possible.

  * Domain
Domain is the inner layer of the application core and is what the application is all about, the crown jewels. It solely consists of business logic encapsulated in aggregates, value objects and domain services.

  * Application
Application is the outer layer of the application core and acts as a wrapper around the domain layer, providing ports (interfaces) which describe how the application core must be used, or how the application core uses external systems.

* Presentation
Presentation translates what comes from the delivery mechanism to the application core. The delivery mechanism can be an HTTP request, a console window, a CRON job, a service bus consumer and so on. These delivery mechanisms are called primary adapters (or driving adapters) which use the ports defined in the application core to tell the application core what to do.

* Infrastructure
Infrastructure enables the application core to interact with external systems. The external system can be a data store, a service bus, an API, an SMTP server and so on. These implementations are called secondary adapters (or driven adapters) which implement ports defined in the application core.

#### Compared to three-tier architecture

In a 'classic' three-tier architecture, presentation (UI) references application core (business logic) and application core references infrastructure (persistence). In a hexagonal architecture all references are inwards, presentation and infrastructure reference application core and application core does not have any references.

```
┌──────────────────┐        ┌──────────────────┐
│                  │        │                  │
│   presentation   │        │   presentation   │
│                  │        │                  │
└──────────────────┘        └──────────────────┘
          │                           │
          ▼                           ▼
┌──────────────────┐        ┌──────────────────┐
│                  │        │                  │
│ application core │        │ application core │
│                  │        │                  │
└──────────────────┘        └──────────────────┘
          │                           ▲
          ▼                           │
┌──────────────────┐        ┌──────────────────┐
│                  │        │                  │
│  infrastructure  │        │  infrastructure  │
│                  │        │                  │
└──────────────────┘        └──────────────────┘
```

*Difference in references between three-tier architecture (left) and hexagonal architecture (right).*

This might seem like a subtle difference, but it is significant. In a three-tier architecture, the infrastructure layer dictates how data should be retrieved and persisted. In a hexagonal architecture, the application core dictates how data should be retrieved and persisted. When another adapter is used, a Postgres adapter instead of Microsoft SQL Server adapter for example, this does not affect the application core in any way.

### Presentation



### Application core

The implementation of the application core is inspired by the blog post Functional Event Sourcing Decider by Jérémie Chassaing [[3]](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider "Functional Event Sourcing Decider"). In his blog post, Jérémie explains how commands, (initial) state, events and a decide and evolve function can be used to set up an event sourced domain.

```
 command ┌──────────────┐
────────►│              │ events
  state  │    decide    │────────┐
┌───────►│              │        │
│        └──────────────┘        │
│                                │
│        ┌──────────────┐  event │
│ state  │              │◄───────┘
├───◄────│    evolve    │  state
│        │              │◄───────┐
│        └──────────────┘        │
└────────────────────────────────┘
```

*Decide and evolve flow of control (courtesy of Jérémie Chassaing).*

The samples used in the blog post are created using F#, which has support for language features that C# does not (yet) have. The challenge was to implement deciders using C#. One important language feature that C# does not support is sum types (also known as discriminated unions), which is currently in proposal for C# [[4]](https://github.com/dotnet/csharplang/blob/main/proposals/discriminated-unions.md "Discriminated unions / enum class"). Because sum types are not available in C#, Giddup uses marker interfaces and switch statements with pattern matching.

In contrast to the method names (decide and evolve) employed in Jérémie's blog post, Giddup utilizes different names. Commands are processed in a command processor, events are processed in an event processor and a separate interface is introduced for retrieving the current state of the aggregate, the state provider. The application service is coordinating the execution of commands. The command processor, event processor and application service are the 'ports' that will be used by the presentation layer and infrastructure layer.

When it comes to errors arising from the validation of business rules within the application core, a deliberate decision has been made to avoid using exceptions, as they can lead to less readable code due to the method signature not clearly indicating potential outcomes [[5]](https://wiki.c2.com/?DontUseExceptionsForFlowControl "Dont Use Exceptions For Flow Control").

### Data architecture diagram
Combining the approach of functional event sourcing deciders with hexagonal architecture and command query responsibility segregation (CQRS) results in the following data architecture diagram:

```
                     │ PRESENTATION            │ APPLICATION CORE                            │ INFRASTRUCTURE          │


                     │                         │                                             │                         │

                                    QUERY MODEL
                     │            ┌───────────────────────────────────────────────────────────────────────┐            │
                                  │                                                                       │
                                  │                                                                       │
                     │            │            │                                             │            │            │
                                  │                                                                       │                    _.───────._
                           ┌──────┴──────┐                                                         ┌──────┴──────┐           .-           -.
                     │     │ controller  │     │                                             │     │    query    │     │     ┤-_         _-│
                  ┌───(9)──┤ or graphql  │◄─────────────────────────(10)───────────────────────────┤  processor  │◄──(11)────│  ~───────~  │
                  │        │    query    │                                                         │             │           │    read     │
                  │  │     └──────┬──────┘     │                                             │     └──────┬──────┘     │     `._  model  _.'
                  │               │                                                                       │                     "───────"
                  │               │                                                                       │                         ▲
                  │  │            │            │                                             │            │            │            │
 ┌─────────────┐  │               │                                                                       │                  ┌──────┴──────┐
 │             │◄─┘               └───────────────────────────────────────────────────────────────────────┘                  │             │
 │   client    │     │              COMMAND MODEL                                            │                         │     │  projector  │
 │             ├──┐               ┌───────────────────────────────────────────────────────────────────────┐                  │             │
 └─────────────┘  │               │                         ┌──────────────────(3)──────────────┐  ┌──────┴──────┐           └──────┬──────┘
                  │  │            │            │            │            ┌─────────────┐     │  │  │    state    │     │            │
                  │               │                         │            │   command   │        └──┤  provider   │◄──(4)──┐         │
                  │               │                         ▼         ┌─►│  processor  │           │             │        │    _.───┴───._
                  │  │     ┌──────┴──────┐     │     ┌─────────────┐  │  │             │     │     └──────┬──────┘     │  │  .-           -.
                  │        │ controller  │           │ application │ (5) └─────────────┘                  │               └──┤-_         _-│
                  └──(1)──►│ or graphql  ├────(2)───►│   service   ├──┘         ▲                         │                  │  ~───────~  │
                     │     │  mutation   │     │     │             │     ┌──────┴──────┐     │            │            │  ┌─►│    write    │
                           └──────┬──────┘           └──────┬──────┘     │   domain    │           ┌──────┴──────┐        │  `._  model  _.'
                                  │                         │            │   service   │           │    event    │        │     "───────"
                     │            │            │            │            │             │     │  ┌─►│  processor  ├───(7)──┘
                                  │                         │            └─────────────┘        │  │             │
                                  │                         └──────────────────(6)──────────────┘  └──────┬──────┘
                     │            └───────────────────────────────────────────────────────────────────────┘            │


                     │                         │                                             │                         │
```

*Data architecture diagram describing the data flow of the Giddup application.*

1. Client sends mutation to controller or GraphQL mutation.
1. Controller or GraphQL mutation converts input into command and sends command to application service.
1. Application service retrieves aggregate state using state provider.
1. State provider retrieves aggregate events from write model and replays all events to rehydrate aggregate (current state).
1. Application service sends command and current state to command processor. Command processor evaluates business rules and returns event(s) if successful.
1. Application service sends events to events processor.
1. Event processor stores event(s) in write model.
1. Read model is updated asynchronously using event(s) from write model.
1. Client sends query to controller or GraphQL query.
1. Controller or GraphQL query sends query to query processor.
1. Query processor retrieves objects from read model.

## Acknowledgments

* [@microsoft](https://github.com/microsoft) for the Azure DevOps screenshot used in this document.
* [@rutgersc](https://github.com/rutgersc) for sharing the blog post Functional Event Sourcing Decider and having great discussions on software architecture.
* [@thinkbeforecoding](https://github.com/thinkbeforecoding) for his excellent blog post Functional Event Sourcing Decider.

## References
> - [1] Huachao Mao. [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).
> - [2] Alistair Cockburn. [Hexagonal architecture](https://alistair.cockburn.us/hexagonal-architecture/).
> - [3] Jérémie Chassaing. [Functional Event Sourcing Decider](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider).
> - [4] Microsoft. [Discriminated unions / enum class](https://github.com/dotnet/csharplang/blob/main/proposals/discriminated-unions.md).
> - [5] Ward Cunningham [Dont Use Exceptions For Flow Control](https://wiki.c2.com/?DontUseExceptionsForFlowControl)
