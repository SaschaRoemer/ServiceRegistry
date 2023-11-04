A playground project inspired by Netflix Eureka.

ServiceRegistry is implemented as an ASP.NET Core 6 Web API. It provides a RESTful interface to register services with name and location (URL). Applications that require a service request the service location.


## Service registration and discovery

A service once registered (POST) must renew (PUT) its registration every minute. In case it was not renewed for more than 2 minutes the service is considered to be offline.

A service consists of a key and a location:

```
Service:
    properties:
    key:
        $ref: '#/components/schemas/ServiceKey'
    location:
        $ref: '#/components/schemas/Location'

Location:
    properties:
    scheme:
        type: string
    host:
        type: string
    path:
        type: string
        
ServiceKey:
    properties:
    environment:
        type: string
    name:
        type: string
```

Applications GET the service location by its key.


### Service endpoint

* GET{key}: Request a service by ENVIRONMENT/NAME.
* POST{service}: Rlegister a new service.
* PUT{service}: Renew the service registration.
* DELETE{location}: Un-registers a service from the registry.

### Load balancing

Different instances (locations) of a service can register under the same key. A simple load banlancing using round-robin is in place to switch between the different locations when a location is requested.