# Project Summary

This solution implements a secure file upload workflow using two microservices:

* **AppService** — exposes public APIs for sellers to upload product images and create products.
* **StorageService** — securely handles image uploads using pre-signed URLs and validates uploaded images.

The system uses HMAC signatures to ensure metadata integrity when generating pre-signed URLs, enforcing secure inter-service communication.

---



## Quick Setup

```bash
# 1. Build and run
docker-compose up --build

# 2. Test
chmod +x test-workflow.sh
./test-workflow.sh
```

---



# HMAC Security Model

HMAC is used only when requesting a pre-signed upload URL.

## Why?

To ensure that:

* Metadata cannot be tampered with
* Only AppService can generate upload URLs
* StorageService trusts the upload contract
* Upload sessions cannot be forged or replayed

## What is signed?

AppService signs the full `FileMetadata`:

```json
{
  "fileName": "...",
  "fileSize": 12345,
  "contentType": "image/png",
  "timestamp": 1700000000,
  "expiresIn": 3600
}
```

StorageService recomputes the HMAC using the shared secret and rejects any mismatches.

This makes the URL truly "pre-signed".

---


# API Endpoints Summary

## AppService

| Method | Endpoint              | Purpose                              |
| ------ | --------------------- | ------------------------------------ |
| POST   | `/api/upload/request` | Request pre-signed URL (HMAC-signed) |
| POST   | `/api/products`       | Create product using imageId         |
| GET    | `/api/products`       | List all products                    |
| GET    | `/api/products/{id}`  | Get product by ID                    |

## StorageService

| Method | Endpoint                              | Purpose                             |
| ------ | ------------------------------------- | ----------------------------------- |
| POST   | `/internal/presigned-url`             | Verify HMAC + create upload session |
| PUT    | `/upload/{uploadId}`                  | Upload image using pre-signed URL   |
| GET    | `/internal/images/{imageId}/validate` | Validate image exists               |
| GET    | `/images/{imageId}`                   | Download image                      |

---


## API Authentication

AppService endpoints require API key authentication. Include the following header in your requests:

```
X-API-Key: seller-secret-key-12345
```

This key should be used when testing with Swagger or any HTTP client (Postman, curl, etc.).

---


# Architecture Overview

## 1. System Level – Microservices Architecture

### End-to-End Workflow Overview

The full upload + product creation workflow works as follows:

1. **Seller requests a pre-signed upload URL** from AppService. AppService signs the `FileMetadata` with HMAC and forwards it to StorageService.
2. **StorageService validates the signature** and creates an `UploadSession`. It returns a time-limited pre-signed URL.
3. **Seller uploads the image** using the pre-signed URL. StorageService validates:
   * Upload session exists
   * Not expired
   * File size matches metadata
   
   It then stores the image and returns an `imageId`.
4. **Seller creates a product** in AppService using the `imageId`. AppService calls StorageService to validate the image existence. If valid, AppService saves the product.
5. **Seller can list or retrieve the product.**

---



The solution follows a microservices architecture with two independent services:

###  App Service

* Owns Products
* Exposes authenticated public APIs for the seller
* Handles:
   * Requesting pre-signed upload URLs
   * Creating products linked to images
   * Listing and retrieving products
* Communicates with Storage Service via HTTP + HMAC-signed requests

###  Storage Service

* Owns Images and Upload Sessions
* Issues pre-signed upload URLs
* Verifies HMAC signatures to ensure integrity
* Stores uploaded image data (in-memory)
* Validates `ImageId` when AppService creates a product

### Service-to-Service Communication

* **Protocol:** HTTP (REST)
* **Format:** JSON
* **Security:** HMAC SHA-256 signatures using a shared secret
* **Purpose:** Prevents tampering and unauthorized upload requests

## 2. Service Level – Clean / Layered Architecture

Each microservice follows a Clean Architecture layout with 4 isolated layers:

```
ServiceName/
  ├── ServiceName.Domain
  ├── ServiceName.Application
  ├── ServiceName.Infrastructure
  └── ServiceName.Api
```

### 2.1 Domain Layer (`*.Domain`)

The Domain layer contains the core business logic.

**Contains:**

* **Entities**
   * AppService → `Product`
   * StorageService → `UploadSession`, `StoredImage`
* **Value Objects**
   * `FileMetadata`
* **Repository Interfaces**
   * `IProductRepository`
   * `IUploadSessionRepository`
   * `IImageRepository`

**Characteristics:**

* Pure POCOs
* No external dependencies
* Contains business rules and invariants

### 2.2 Application Layer (`*.Application`)

The Application layer contains the use cases and workflows.

**Contains:**

* **Commands & Queries (MediatR)**
   * `CreateProductCommand`
   * `RequestUploadUrlCommand`
   * `CreateUploadSessionCommand`
   * `ProcessUploadCommand`
   * `ValidateImageQuery`
* **DTOs**
* **Interfaces for external dependencies**
   * `IStorageServiceClient`
   * `ISignatureService`
   * `IFileStorageService`

**Responsibilities:**

* Executes business use cases
* Calls domain repository interfaces
* No infrastructure or framework details

### 2.3 Infrastructure Layer (`*.Infrastructure`)

The Infrastructure layer provides implementations of external concerns.

**Contains:**

* **Repository Implementations** (in-memory)
* **HTTP Clients**
   * `StorageServiceClient` (AppService → StorageService)
* **Security Components**
   * `HmacSignatureService`
* **File Storage Implementations**
   * `InMemoryFileStorageService`

**Responsibilities:**

* Implements Application layer interfaces
* Communicates with external systems
* Depends on Application and Domain layers

### 2.4 API Layer (`*.Api`)

The API layer exposes HTTP endpoints and hosts the service.

**Contains:**

* **Endpoints / Controllers**
   * Product endpoints
   * Upload endpoints
   * Image endpoints
* **Program.cs & DI configuration**
* **Swagger/OpenAPI**
* **Middleware**
   * API Key authentication (AppService)
   * Validation pipeline

**Responsibilities:**

* Handles incoming HTTP requests
* Maps them to MediatR commands and queries
* Performs validation and returns API responses

## System Architecture

```mermaid
flowchart LR
    Client["Seller / Client"]
    subgraph AppService
        direction TB
        AppApi["API Layer (AppService.Api)"]
        AppApp["Application Layer (AppService.Application)"]
        AppDomain["Domain Layer (AppService.Domain)"]
        AppInfra["Infrastructure Layer (AppService.Infrastructure)"]
    end
    subgraph StorageService
        direction TB
        StorApi["API Layer (StorageService.Api)"]
        StorApp["Application Layer (StorageService.Application)"]
        StorDomain["Domain Layer (StorageService.Domain)"]
        StorInfra["Infrastructure Layer (StorageService.Infrastructure)"]
    end
    Client --> AppApi
    AppApi --> AppApp
    AppApp --> AppDomain
    AppApp --> AppInfra
    AppInfra -->|"HTTP + HMAC"| StorApi
    StorApi --> StorApp
    StorApp --> StorDomain
    StorApp --> StorInfra
```

## Flow 1 – Request Pre-Signed Upload URL

```mermaid
sequenceDiagram
    actor Client as "Seller"
    participant AppApi as "AppService API"
    participant AppHandler as "RequestUploadUrlHandler"
    participant SigApp as "HMAC Service (App)"
    participant HttpClient as "StorageServiceClient"
    participant StorApi as "StorageService API"
    participant StorHandler as "CreateUploadSessionHandler"
    participant SessRepo as "UploadSessionRepository"
    Client->>AppApi: POST /api/upload/request\n{ fileName, fileSize, contentType }
    AppApi->>AppHandler: RequestUploadUrlCommand
    AppHandler->>AppHandler: Build FileMetadata\n(timestamp, expiresIn)
    AppHandler->>SigApp: Sign(metadata)
    SigApp-->>AppHandler: signature
    AppHandler->>HttpClient: RequestPresignedUrl(metadata, signature)
    HttpClient->>StorApi: POST /internal/presigned-url\n{ metadata, signature }
    StorApi->>StorHandler: CreateUploadSessionCommand
    StorHandler->>StorHandler: Validate timestamp
    StorHandler->>SigApp: Verify(metadata, signature)
    SigApp-->>StorHandler: valid / invalid
    alt Invalid signature or expired metadata
        StorHandler-->>StorApi: error (401 / 400)
        StorApi-->>HttpClient: error response
        HttpClient-->>AppHandler: failure
        AppHandler-->>AppApi: throws InvalidOperation
        AppApi-->>Client: 4xx / 5xx error
    else Valid request
        StorHandler->>SessRepo: Add(uploadSession with uploadId, expiresAt)
        StorHandler-->>StorApi: { uploadUrl, uploadId, expiresAt }
        StorApi-->>HttpClient: 200 OK
        HttpClient-->>AppHandler: PresignedUrlResponse
        AppHandler-->>AppApi: RequestUploadUrlResult
        AppApi-->>Client: 200 OK\n{ uploadUrl, uploadId, expiresAt }
    end
```

## Flow 2 – Upload Image Using Pre-Signed URL

```mermaid
sequenceDiagram
    actor Client as "Seller"
    participant StorApi as "StorageService API"
    participant StorHandler as "ProcessUploadHandler"
    participant SessRepo as "UploadSessionRepository"
    participant ImgRepo as "ImageRepository"
    Client->>StorApi: PUT /upload/{uploadId}\n{ base64Data }
    StorApi->>StorHandler: ProcessUploadCommand\n{ uploadId, fileData }
    StorHandler->>SessRepo: GetByIdAsync(uploadId)
    SessRepo-->>StorHandler: UploadSession or null
    alt Session not found
        StorHandler-->>StorApi: error "Invalid or expired upload ID"
        StorApi-->>Client: 400 Bad Request
    else Session found
        StorHandler->>StorHandler: Check session.IsExpired()
        alt Session expired
            StorHandler->>SessRepo: Remove(uploadId)
            StorHandler-->>StorApi: error "Upload URL has expired"
            StorApi-->>Client: 400 Bad Request
        else Session valid
            StorHandler->>StorHandler: Validate file size\n(actual == metadata.FileSize)
            alt Size mismatch
                StorHandler-->>StorApi: error "File size mismatch"
                StorApi-->>Client: 400 Bad Request
            else Size OK
                StorHandler->>ImgRepo: AddAsync(StoredImage\nwith new ImageId)
                StorHandler->>SessRepo: Remove(uploadId)
                StorHandler-->>StorApi: { imageId, fileName, fileSize, uploadedAt }
                StorApi-->>Client: 200 OK\n{ imageId, fileName, fileSize, uploadedAt }
            end
        end
    end
```

## Flow 3 – Create Product with ImageId

```mermaid
sequenceDiagram
    actor Client as "Seller"
    participant AppApi as "AppService API"
    participant AppHandler as "CreateProductHandler"
    participant HttpClient as "StorageServiceClient"
    participant StorApi as "StorageService API"
    participant StorHandler as "ValidateImageHandler"
    participant ImgRepo as "ImageRepository"
    participant ProdRepo as "ProductRepository"
    Client->>AppApi: POST /api/Products\n{ name, description, price, imageId }
    AppApi->>AppHandler: CreateProductCommand
    AppHandler->>HttpClient: ValidateImageAsync(imageId)
    HttpClient->>StorApi: GET /internal/images/{imageId}/validate
    StorApi->>StorHandler: ValidateImageQuery
    StorHandler->>ImgRepo: GetByIdAsync(imageId)
    ImgRepo-->>StorHandler: StoredImage or null
    alt Image not found
        StorHandler-->>StorApi: null
        StorApi-->>HttpClient: 404 Not Found
        HttpClient-->>AppHandler: false
        AppHandler-->>AppApi: throws InvalidOperation\n("Invalid image ID")
        AppApi-->>Client: 500 error\n("Invalid image ID. Please upload first.")
    else Image found
        StorHandler-->>StorApi: ValidateImageResult
        StorApi-->>HttpClient: 200 OK
        HttpClient-->>AppHandler: true
        AppHandler->>ProdRepo: AddAsync(new Product\nwith ImageId)
        AppHandler-->>AppApi: CreateProductResult\n{ productId, name, price, imageId }
        AppApi-->>Client: 201 Created\n{ productId, name, price, imageId, createdAt }
    end
```

## Flow 4 – Get Products / Get Product by Id

### 4.1 Get all products

```mermaid
sequenceDiagram
    actor Client as "Seller"
    participant AppApi as "AppService API"
    participant QueryHandler as "GetAllProductsHandler"
    participant ProdRepo as "ProductRepository"
    Client->>AppApi: GET /api/Products
    AppApi->>QueryHandler: GetAllProductsQuery
    QueryHandler->>ProdRepo: GetAllAsync()
    ProdRepo-->>QueryHandler: List<Product>
    QueryHandler-->>AppApi: List<ProductDto>
    AppApi-->>Client: 200 OK\n[ { productId, name, price, imageId, createdAt }, ... ]
```

### 4.2 Get product by id

```mermaid
sequenceDiagram
    actor Client as "Seller"
    participant AppApi as "AppService API"
    participant QueryHandler as "GetProductByIdHandler"
    participant ProdRepo as "ProductRepository"
    Client->>AppApi: GET /api/Products/{id}
    AppApi->>QueryHandler: GetProductByIdQuery(id)
    QueryHandler->>ProdRepo: GetByIdAsync(id)
    ProdRepo-->>QueryHandler: Product or null
    alt Product found
        QueryHandler-->>AppApi: ProductDto
        AppApi-->>Client: 200 OK\n{ productId, name, price, imageId, createdAt }
    else Product not found
        QueryHandler-->>AppApi: null
        AppApi-->>Client: 404 Not Found
    end
```

-------

# Enhancements (Not Required for Task)

If extended, the system could support:

* Storing images in S3, Azure Blob, or local filesystem
* Adding JWT authentication instead of API key
* Adding rate limiting to prevent abuse
* Adding background workers for image processing
* Storing products and images in persistent databases
* Upload session reuse prevention using Redis
* Client-side direct uploads to StorageService
* Change the internal communication from Http Request to Grpc
