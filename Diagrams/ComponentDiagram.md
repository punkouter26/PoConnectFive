graph TD
    subgraph Browser[Client Side - Browser]
        UI[Blazor WebAssembly UI]
        Services[Client-Side Services]
        UI --> Services
    end

    subgraph Server[Server Side - .NET API]
        API[ASP.NET Core Web API]
        Logic[Application Logic/Services]
        Data[Data Access Layer]
        API --> Logic
        Logic --> Data
    end

    subgraph Azure[Azure Cloud]
        Storage[Azure Table Storage]
        AppService[Azure App Service]
    end

    subgraph LocalDev[Local Development]
        Azurite[Azurite Emulator]
    end

    UI -- HTTPS/WSS --> API
    Services -- HTTPS --> API
    Data -- Azure SDK --> Storage
    Data -- Azure SDK --> Azurite

    API -- Hosted In --> AppService
    Storage -- Used By --> AppService

    style UI fill:#f9f,stroke:#333,stroke-width:2px
    style API fill:#bbf,stroke:#333,stroke-width:2px
    style Storage fill:#f96,stroke:#333,stroke-width:2px
    style Azurite fill:#ccf,stroke:#333,stroke-width:2px,stroke-dasharray: 5 5
