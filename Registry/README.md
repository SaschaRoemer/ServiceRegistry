A playground project inspired by Netflix Eureka.

ServiceRegistry is implmeneted in .NET 6 as an ASP.NET Web API. It provides a RESTful interface where a service can register itself with name and location and applications can request the service location.


## Service registration and discovery

A service once registered (POST) must renew (PUT) its registration every minute. In case it was not renewed for more than 2 minutes the service is considered to be offline and will not be available on GET.

### Service endpoint

* GET{key}: Get a service by ENVIRONMENT/NAME.
* POST{service}: Register a new service.
* PUT{service}: Renew the service registration.
* DELETE{location}: Un-registers a service from the registry.

### Load balancing

Different instances (locations) of a service can register (POST) under the same key. A simple load banlancing using round robin is in place to switch between the different locations when a location is requested (GET).


## Replication

ServiceRegistry supports a basic replication as a peer-to-peer approach. An instance is configured with its on location and the location(s) of other instances. The registry is synchronized between all instances each minute.

### Registry endpoint

* POST{service[]}: Receives a list of services and updates the internal list.


### Persistence

The internal list of services is only stored in memory. This is good for performance on one side but will lead to a loss of registered services if the web service goes offline. When the services goes online again it's data will be build-up as soon as services renew their registration.
Through replication the registered services are retained as long as at least one peer is online.


## Disclaimer

This project aims to show the basics of service discovery, .NET 6 and Web API. Being without any kind of security this implementation is far from production ready. All actions like registering a service or replication is open to everyone wich makes it vulnerable even to the simplest attacs. There is no protection against registering a malicious service by a third party.

Exception handling is another topic which is completely missing.