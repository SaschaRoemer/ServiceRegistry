#$location = "https://localhost:7089"
$location = "http://host.docker.internal:7089"
#$location = "https://wapp-6141f1e3-802c-4aa1-b11d-c0c7dd74f0f0.azurewebsites.net/"
$environment = "dev"
$service = "Echo"

$request = @{
    Uri = "${location}/Service"
    Headers = @{"Accept" = "*/*"}
    Body = "{ `"key`": { `"environment`": `"${environment}`", `"name`": `"${service}`" }, `"location`": { `"scheme`": `"http`", `"host`": `"localhost:80`", `"path`": `"/${service}`" } }"
    ContentType = "application/json"
}

Invoke-WebRequest -Method "POST" @request | Select-Object -Property StatusCode, Content

Invoke-WebRequest -Method "GET" -Uri "${location}/Service/${environment}/${service}" | Select-Object -Property StatusCode, Content

Invoke-WebRequest -Method "PUT" @request | Select-Object -Property StatusCode, Content

Invoke-WebRequest -Method "DELETE" -Uri "${location}/Service/http%3A%2F%2Flocalhost%2FEcho" | Select-Object -Property StatusCode, Content

Invoke-WebRequest -Method "GET" -Uri "${location}/Service/${environment}/${service}" | Select-Object -Property StatusCode, Content