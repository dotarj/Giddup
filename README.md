# Giddup reference architecture

This repository contains a reference architecture using ports and adapters, domain-driven design (DDD), command query responsibility segregation (CQRS) and event sourcing (ES). It is written in C# using ASP.NET Core as web application framework and EventStore as data store.

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

 By running the Docker Compose file, an EventStore instance is started and the solution is build and started. When the services have started, you can make requests to localhost:5000.

 Sample HTTP requests of all available endpoints are provided in tools/pull-requests.http. The HTTP requests in this file can be executed in Visual Studio Code using the REST Client extension by Huachao Mao [[1]](https://marketplace.visualstudio.com/items?itemName=humao.rest-client "REST Client").

## Functional overview

Giddup is based on the Azure DevOps pull request user interface, which is a fully task-driven user interface. Task-driven user interfaces usually provide much better user experience and are the cornerstone of CQRS, a task in the user interface translates directly to a command on the server side.

![Azure DevOps pull request user interface](docs/images/pull-request-overview.png)

*Azure DevOps pull request user interface (courtesy of Microsoft).*

There are all kinds of tasks that can be performed in the Azure DevOps pull request user interface: adding and removing reviewers, linking and removing work items, submitting and resetting reviewer feedback, completing, abandoning and reactivating the pull request and so on. Most of the tasks provided in the Azure DevOps pull request user interface are implemented in Giddup.

TODO: expand on tasks with screenshots

## Technical overview

### Project structure

The structure of the solution conforms to the ports and adapters architecture (hexagonal architecture) introduced by Alistair Cockburn [[2]](https://alistair.cockburn.us/hexagonal-architecture/ "Hexagonal architecture"). In this architecture three main components are identified, each with their respective responsibility:

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

In a 'classic' three-tier architecture, presentation (UI) references application core (business logic) and application core references infrastructure (persistence). In a ports and adapters architecture all references are inwards, presentation and infrastructure reference application core and application core does not have any references.

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

*Difference in references between three-tier architecture (left) and ports and adapters architecture (right).*

This might seem like a subtle difference, but it is significant. In a three-tier architecture, the infrastructure layer dictates how data should be retrieved and persisted. In a ports and adapters architecture, the application core dictates how data should be retrieved and persisted. When another adapter is used, a Postgres adapter instead of Microsoft SQL Server adapter for example, this does not affect the application core in any way.

### Deciders

The implementation of the application core is inspired by the blog post Functional Event Sourcing Decider by Jérémie Chassaing [[3]](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider "Functional Event Sourcing Decider"). In his blog post Jérémie explains how commands, (initial) state, events and a decide and evolve function can be used to set up an event sourced domain. This simple drawing describes the flow of control:

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

## Acknowledgments

* [@microsoft](https://github.com/microsoft) for the Azure DevOps screenshots used in this document.
* [@rutgersc](https://github.com/rutgersc) for sharing the blog post Functional Event Sourcing Decider and having great discussions on software architecture.
* [@thinkbeforecoding](https://github.com/thinkbeforecoding) for his excellent blog post Functional Event Sourcing Decider.

## References
> - [1] Huachao Mao. [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).
> - [2] Alistair Cockburn. [Hexagonal architecture](https://alistair.cockburn.us/hexagonal-architecture/).
> - [3] Jérémie Chassaing. [Functional Event Sourcing Decider](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider).
> - [4] Microsoft. [Discriminated unions / enum class](https://github.com/dotnet/csharplang/blob/main/proposals/discriminated-unions.md).
