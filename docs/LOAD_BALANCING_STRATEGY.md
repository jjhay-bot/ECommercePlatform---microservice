# Load Balancing Strategy for E-Commerce Microservices Platform

## Overview

This document outlines the load balancing strategies, implementation options, and configuration details for scaling the e-commerce microservices platform. Load balancing is essential for high availability, fault tolerance, and horizontal scaling.

## Current State vs Future State

### Current Architecture (Single Instance)
```
Client → ProductCatalogService:5241
       ↓
   MySQL Database
```

### Target Architecture (Load Balanced)
```
Client → Load Balancer → ProductCatalogService:5241
                      → ProductCatalogService:5242  
                      → ProductCatalogService:5243
                      ↓
                 Shared MySQL Database
```

## Load Balancing Options

### 1. API Gateway Pattern (Recommended)

**Advantages:**
- ✅ Single entry point for all microservices
- ✅ Cross-cutting concerns (auth, logging, rate limiting)
- ✅ Service discovery and routing
- ✅ Protocol translation and aggregation
- ✅ Built-in load balancing algorithms

**Implementation Options:**

#### Option A: Ocelot (.NET API Gateway)
```
ApiGateway (Ocelot) → ProductCatalogService instances
                   → OrderService instances
                   → UserService instances
                   → PaymentService instances
```

#### Option B: Kong/Nginx Plus
```
Kong API Gateway → Microservice instances
```

#### Option C: Cloud-Native (Azure API Management, AWS API Gateway)
```
Azure API Management → Microservice instances in AKS
```

### 2. Reverse Proxy Pattern

**Advantages:**
- ✅ Simple to implement
- ✅ High performance
- ✅ SSL termination
- ✅ Static file serving

**Implementation: Nginx**
```
Nginx Reverse Proxy → ProductCatalogService instances
```

### 3. Container Orchestration

**Advantages:**
- ✅ Auto-scaling based on metrics
- ✅ Health checks and self-healing
- ✅ Service mesh integration
- ✅ Rolling deployments

**Implementation Options:**
- Docker Swarm
- Kubernetes
- Azure Container Instances

### 4. Cloud Load Balancers

**Advantages:**
- ✅ Fully managed
- ✅ Global load balancing
- ✅ DDoS protection
- ✅ SSL certificates management

**Implementation Options:**
- Azure Application Gateway
- AWS Application Load Balancer
- Google Cloud Load Balancer

## Implementation Guide

### Option 1: Ocelot API Gateway (.NET)

#### Step 1: Create API Gateway Project
```bash
# In ECommercePlatform folder
dotnet new webapi -n ApiGateway
cd ApiGateway
dotnet add package Ocelot
dotnet add package Ocelot.Provider.Consul  # For service discovery
```

#### Step 2: Configure Ocelot
**ApiGateway/ocelot.json**
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/products/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5241
        },
        {
          "Host": "localhost",
          "Port": 5242
        },
        {
          "Host": "localhost",
          "Port": 5243
        }
      ],
      "UpstreamPathTemplate": "/api/v1/products/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "LoadBalancerOptions": {
        "Type": "RoundRobin"
      },
      "RateLimitOptions": {
        "ClientWhitelist": [],
        "EnableRateLimiting": true,
        "Period": "1m",
        "PeriodTimespan": 60,
        "Limit": 100
      }
    },
    {
      "DownstreamPathTemplate": "/api/v1/orders/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5251
        },
        {
          "Host": "localhost",
          "Port": 5252
        }
      ],
      "UpstreamPathTemplate": "/api/v1/orders/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "LoadBalancerOptions": {
        "Type": "LeastConnection"
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5000",
    "RateLimitOptions": {
      "DisableRateLimitHeaders": false,
      "QuotaExceededMessage": "API call quota exceeded!",
      "RateLimitCounterPrefix": "ocelot",
      "HttpStatusCode": 429
    }
  }
}
```

#### Step 3: Configure Program.cs
**ApiGateway/Program.cs**
```csharp
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Ocelot services
builder.Services.AddOcelot();

// Add CORS for web clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure pipeline
app.UseCors("AllowWebApp");
await app.UseOcelot();

app.Run();
```

### Option 2: Docker Compose with Nginx

#### Step 1: Create Docker Configuration
**docker-compose.yml**
```yaml
version: '3.8'

services:
  # MySQL Database
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword
      MYSQL_DATABASE: ProductCatalogDb
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  # ProductCatalogService instances
  productcatalog-1:
    build: ./ProductCatalogService
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "server=mysql;port=3306;database=ProductCatalogDb;user=root;password=rootpassword;"
    depends_on:
      - mysql
    expose:
      - "80"

  productcatalog-2:
    build: ./ProductCatalogService
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "server=mysql;port=3306;database=ProductCatalogDb;user=root;password=rootpassword;"
    depends_on:
      - mysql
    expose:
      - "80"

  productcatalog-3:
    build: ./ProductCatalogService
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "server=mysql;port=3306;database=ProductCatalogDb;user=root;password=rootpassword;"
    depends_on:
      - mysql
    expose:
      - "80"

  # Nginx Load Balancer
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - productcatalog-1
      - productcatalog-2
      - productcatalog-3

volumes:
  mysql_data:
```

#### Step 2: Configure Nginx
**nginx.conf**
```nginx
events {
    worker_connections 1024;
}

http {
    # Upstream definition for ProductCatalogService
    upstream productcatalog {
        least_conn;  # Load balancing method
        server productcatalog-1:80 max_fails=3 fail_timeout=30s;
        server productcatalog-2:80 max_fails=3 fail_timeout=30s;
        server productcatalog-3:80 max_fails=3 fail_timeout=30s;
    }

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;

    server {
        listen 80;
        server_name localhost;

        # Rate limiting
        limit_req zone=api burst=20 nodelay;

        # Health check endpoint
        location /health {
            access_log off;
            return 200 "healthy\\n";
            add_header Content-Type text/plain;
        }

        # ProductCatalogService routing
        location /api/v1/products {
            proxy_pass http://productcatalog;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            
            # Timeouts
            proxy_connect_timeout 30s;
            proxy_send_timeout 30s;
            proxy_read_timeout 30s;
            
            # Health checks
            proxy_next_upstream error timeout invalid_header http_500 http_502 http_503 http_504;
        }

        # Future services
        # location /api/v1/orders {
        #     proxy_pass http://orderservice;
        #     # ... similar configuration
        # }
    }

    # SSL Configuration (for production)
    server {
        listen 443 ssl http2;
        server_name yourdomain.com;

        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512;

        location / {
            proxy_pass http://productcatalog;
            # ... same proxy configuration as above
        }
    }
}
```

### Option 3: Kubernetes Deployment

#### Step 1: Service Deployment
**k8s/productcatalog-deployment.yaml**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: productcatalog-deployment
spec:
  replicas: 3
  selector:
    matchLabels:
      app: productcatalog
  template:
    metadata:
      labels:
        app: productcatalog
    spec:
      containers:
      - name: productcatalog
        image: productcatalog:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          value: "server=mysql-service;port=3306;database=ProductCatalogDb;user=root;password=rootpassword;"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: productcatalog-service
spec:
  selector:
    app: productcatalog
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
```

#### Step 2: Ingress Controller
**k8s/ingress.yaml**
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: api-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    nginx.ingress.kubernetes.io/rate-limit-connections: "10"
    nginx.ingress.kubernetes.io/rate-limit-window: "1m"
spec:
  ingressClassName: nginx
  rules:
  - host: api.yourdomain.com
    http:
      paths:
      - path: /api/v1/products
        pathType: Prefix
        backend:
          service:
            name: productcatalog-service
            port:
              number: 80
```

## Load Balancing Algorithms

### 1. Round Robin
- **How it works**: Requests distributed evenly across all instances
- **Best for**: Services with similar response times
- **Configuration**: `"Type": "RoundRobin"`

### 2. Least Connections
- **How it works**: Routes to instance with fewest active connections
- **Best for**: Services with varying response times
- **Configuration**: `"Type": "LeastConnection"`

### 3. Weighted Round Robin
- **How it works**: Distributes based on assigned weights
- **Best for**: Mixed instance sizes
- **Configuration**: Custom weights per instance

### 4. IP Hash
- **How it works**: Routes based on client IP hash
- **Best for**: Session affinity requirements
- **Configuration**: `"Type": "NoLoadBalancer"` with sticky sessions

## Health Checks and Monitoring

### Application Health Checks
**ProductCatalogService/Program.cs**
```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContext<ProductContext>()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddMySql(builder.Configuration.GetConnectionString("DefaultConnection"));

// Configure health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### Monitoring Metrics

#### Key Metrics to Track
1. **Request Rate**: Requests per second per instance
2. **Response Time**: Average and 95th percentile latency
3. **Error Rate**: 4xx and 5xx responses percentage
4. **Instance Health**: Up/down status of each instance
5. **Database Connections**: Active connection count
6. **Resource Usage**: CPU, memory, disk I/O

#### Implementation with Application Insights
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Custom metrics
services.AddSingleton<IMetrics, Metrics>();
```

## Performance Considerations

### Connection Pooling
```csharp
// ProductCatalogService/Program.cs
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseMySql(connectionString, serverVersion, mysqlOptions =>
    {
        mysqlOptions.CommandTimeout(30);
    }),
    ServiceLifetime.Scoped); // Important for load balanced scenarios
```

### Caching Strategy
```csharp
// Add distributed caching for load balanced scenarios
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// In controller
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "id" })]
public async Task<ActionResult<Product>> GetProduct(int id)
```

### Database Considerations
- **Connection Pooling**: Configure appropriate pool sizes
- **Read Replicas**: Separate read/write operations
- **Connection String**: Use connection pooling parameters
- **Transactions**: Handle distributed scenarios properly

## Security Considerations

### SSL/TLS Termination
- Terminate SSL at load balancer level
- Use HTTP between load balancer and services (internal network)
- Implement certificate rotation strategies

### Rate Limiting
```json
{
  "RateLimitOptions": {
    "EnableRateLimiting": true,
    "Period": "1m",
    "PeriodTimespan": 60,
    "Limit": 100,
    "QuotaExceededMessage": "Rate limit exceeded"
  }
}
```

### DDoS Protection
- Implement rate limiting per IP
- Use cloud-based DDoS protection
- Configure appropriate timeouts
- Implement circuit breaker patterns

## Deployment Strategies

### Blue-Green Deployment
```bash
# Deploy to green environment
docker-compose -f docker-compose.green.yml up -d

# Switch load balancer to green
# Update nginx configuration

# Remove blue environment
docker-compose -f docker-compose.blue.yml down
```

### Rolling Updates
```yaml
# Kubernetes rolling update
spec:
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1
      maxSurge: 1
```

### Canary Deployment
```nginx
# Nginx canary deployment
upstream productcatalog_stable {
    server productcatalog-1:80;
    server productcatalog-2:80;
}

upstream productcatalog_canary {
    server productcatalog-canary:80;
}

# Route 10% of traffic to canary
location /api/v1/products {
    if ($request_id ~ "^.{1}$") {
        proxy_pass http://productcatalog_canary;
        break;
    }
    proxy_pass http://productcatalog_stable;
}
```

## Testing Load Balancing

### Load Testing with Artillery
**loadtest.yml**
```yaml
config:
  target: 'http://localhost'
  phases:
    - duration: 60
      arrivalRate: 10
    - duration: 120
      arrivalRate: 50
  defaults:
    headers:
      Content-Type: 'application/json'

scenarios:
  - name: "Get all products"
    weight: 70
    flow:
      - get:
          url: "/api/v1/products"
  
  - name: "Create product"
    weight: 20
    flow:
      - post:
          url: "/api/v1/products"
          json:
            name: "Test Product"
            description: "Load test product"
            price: 19.99
            stock: 100
  
  - name: "Get specific product"
    weight: 10
    flow:
      - get:
          url: "/api/v1/products/1"
```

### Run Tests
```bash
# Install Artillery
npm install -g artillery

# Run load test
artillery run loadtest.yml

# Generate HTML report
artillery run --output report.json loadtest.yml
artillery report report.json
```

## Troubleshooting

### Common Issues

#### 1. Session Affinity Problems
- **Problem**: User session data not available on different instances
- **Solution**: Use distributed session storage (Redis)

#### 2. Database Connection Exhaustion
- **Problem**: Too many database connections from multiple instances
- **Solution**: Configure connection pooling properly

#### 3. Health Check Failures
- **Problem**: Load balancer removes healthy instances
- **Solution**: Configure appropriate health check timeouts

#### 4. Uneven Load Distribution
- **Problem**: Some instances receive more traffic
- **Solution**: Check load balancing algorithm and instance health

### Monitoring Commands
```bash
# Check Nginx status
nginx -t
systemctl status nginx

# View Nginx logs
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log

# Check Docker containers
docker ps
docker stats

# Kubernetes debugging
kubectl get pods
kubectl describe pod <pod-name>
kubectl logs <pod-name>
```

## Next Steps

1. **Start with Ocelot API Gateway** for .NET-native solution
2. **Implement health checks** in ProductCatalogService
3. **Add monitoring and logging** with Application Insights
4. **Create Docker configurations** for easy deployment
5. **Set up CI/CD pipeline** for automated deployments
6. **Implement distributed caching** with Redis
7. **Add comprehensive load testing** to validate performance

This load balancing strategy provides a solid foundation for scaling your microservices platform as it grows.
