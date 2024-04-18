**Bamboo-card - Developer Coding Test => HackerNewsStories**


### HackerNewsStories 
Approaches:
Caching to reduce the number of requests to external APIs and speed up response to users (full list caching and pruning on request).
Using binary serialization to cache data (protobuf).
External API limitations. Using SemaphoreSlim to manage parallel queries.

The following approaches can be used:

Rate Limiting: To prevent both the native API and the external API from becoming overloaded, it is important to limit the rate of outgoing and incoming requests. This can be done using middleware, which checks the number of requests from one user or IP address per unit of time.
For rate limiting you can use the AspNetCoreRateLimit library.
It might be worth looking at the stability pattern (Polly).

### Project structure:
*Stories.API
*StoriesTests


#### HackerNewsStories which includes (just for fun...); 
* ASP.NET Core Minimal APIs and latest features of .NET8 
* Vertical Slice Architecture
* Using **MemoryCache**
* CQRS implementation using MediatR library
* CQRS Validation Pipeline Behaviors with MediatR and FluentValidation
* Use Carter for Minimal API endpoint definition
* Use Swagger
* Cross-cutting concerns Logging, Global Exception Handling and Health Checks

  
### Run The Project
The project can be launched in debug mode.

### Installing
Clone the repository.

```

## Authors

* **Sergiy Ponomarenko** 

