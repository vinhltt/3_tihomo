# Production SSL Deployment Guide

## 📋 Overview

This guide outlines the production SSL deployment strategy for TiHoMo project, addressing the transition from development HTTP environment to production HTTPS environment.

## 🏗️ SSL Architecture

### Current Development Setup
```
Frontend (HTTP) → Ocelot Gateway (HTTP) → Microservices (HTTP)
```

### Production SSL Architecture (Recommended)
```
Internet → Nginx (SSL Termination) → Ocelot Gateway (HTTP) → Microservices (HTTP)
         ↳ Let's Encrypt Certificates
```

## 🔧 Approach 1: Reverse Proxy SSL Termination (Recommended)

### Why This Approach?
- **Centralized SSL Management**: Single point for certificate handling
- **Performance**: SSL termination at edge, HTTP internally
- **Cost-Effective**: Free Let's Encrypt certificates
- **Scalability**: Easy to scale backend services
- **Security**: Additional security layer with rate limiting

### Components
1. **Nginx**: SSL termination, reverse proxy, security headers
2. **Let's Encrypt**: Free SSL certificates with auto-renewal
3. **Certbot**: Automatic certificate management
4. **Docker**: Containerized deployment

### Implementation Files
- `docker-compose.yml`: Production container setup
- `config/nginx/nginx.production.conf`: Nginx SSL configuration
- `scripts/setup-production-ssl.sh`: SSL setup automation
- `src/be/Ocelot.Gateway/appsettings.Production.json`: Production gateway config

### Deployment Steps

#### 1. DNS Configuration
```bash
# Point your domains to your server
api.tihomo.com    → YOUR_SERVER_IP
app.tihomo.com    → YOUR_SERVER_IP
```

#### 2. SSL Certificate Setup
```bash
# Make script executable
chmod +x scripts/setup-production-ssl.sh

# Setup SSL certificates (staging first for testing)
STAGING=true ./scripts/setup-production-ssl.sh

# Setup production SSL certificates
./scripts/setup-production-ssl.sh
```

#### 3. Production Deployment
```bash
# Deploy production environment
docker-compose up -d

# Check services status
docker-compose ps

# Monitor logs
docker-compose logs -f
```

#### 4. Certificate Renewal Setup
```bash
# Add to crontab for automatic renewal
crontab -e

# Add this line:
0 12 * * * /path/to/tihomo/scripts/renew-certificates.sh
```

### Production URLs
- **API Gateway**: `https://api.tihomo.com`
- **Frontend**: `https://app.tihomo.com`
- **Health Check**: `https://api.tihomo.com/health`

## 🔧 Approach 2: Cloud Load Balancer SSL

### AWS Application Load Balancer
```yaml
# AWS ALB with ACM certificates
LoadBalancer:
  Type: AWS::ElasticLoadBalancingV2::LoadBalancer
  Properties:
    Scheme: internet-facing
    SecurityGroups: [!Ref ALBSecurityGroup]
    Subnets: [!Ref PublicSubnet1, !Ref PublicSubnet2]

Listener:
  Type: AWS::ElasticLoadBalancingV2::Listener
  Properties:
    Protocol: HTTPS
    Port: 443
    Certificates:
      - CertificateArn: !Ref SSLCertificate
    DefaultActions:
      - Type: forward
        TargetGroupArn: !Ref TargetGroup
```

### Benefits
- **Managed SSL**: AWS Certificate Manager handles certificates
- **Auto-Renewal**: Automatic certificate renewal
- **High Availability**: Built-in load balancing
- **Scaling**: Automatic scaling capabilities

### Considerations
- **Cost**: Monthly charges for load balancer
- **Vendor Lock-in**: AWS-specific solution
- **Complexity**: Requires VPC setup

## 🔧 Approach 3: Kubernetes Ingress

### Kubernetes Setup with cert-manager
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: tihomo-ingress
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - api.tihomo.com
    - app.tihomo.com
    secretName: tihomo-tls
  rules:
  - host: api.tihomo.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: ocelot-gateway
            port:
              number: 5000
```

### Benefits
- **Orchestration**: Full Kubernetes benefits
- **Auto-Scaling**: Horizontal pod autoscaling
- **Service Discovery**: Built-in service discovery
- **Rolling Updates**: Zero-downtime deployments

### Considerations
- **Complexity**: Kubernetes learning curve
- **Resource Overhead**: Additional infrastructure
- **Management**: Cluster management required

## 🔧 Approach 4: Direct Application SSL

### .NET Application SSL
```csharp
// Program.cs
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5000); // HTTP
    options.Listen(IPAddress.Any, 5001, listenOptions =>
    {
        listenOptions.UseHttps("certificate.pfx", "password");
    });
});
```

### Benefits
- **Direct Control**: Full control over SSL configuration
- **Simplicity**: No additional components
- **Performance**: No proxy overhead

### Considerations
- **Certificate Management**: Manual certificate handling
- **Renewal Complexity**: Manual renewal processes
- **Security**: Each service needs certificate management

## 📊 Comparison Matrix

| Approach | Complexity | Cost | Management | Performance | Security |
|----------|------------|------|------------|-------------|----------|
| Reverse Proxy | Medium | Low | Medium | High | High |
| Cloud LB | Low | Medium | Low | High | High |
| Kubernetes | High | Medium | High | High | High |
| Direct SSL | Low | Low | High | Medium | Medium |

## 🛡️ Security Considerations

### SSL/TLS Configuration
- **Minimum TLS 1.2**: Disable older protocols
- **Strong Ciphers**: Use modern cipher suites
- **HSTS**: HTTP Strict Transport Security
- **Certificate Pinning**: For mobile applications

### Security Headers
```nginx
add_header X-Frame-Options "SAMEORIGIN" always;
add_header X-Content-Type-Options "nosniff" always;
add_header X-XSS-Protection "1; mode=block" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
add_header Content-Security-Policy "default-src 'self';" always;
```

### Rate Limiting
```nginx
limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
limit_req_zone $binary_remote_addr zone=login:10m rate=5r/m;
```

## 🔍 Monitoring & Alerting

### SSL Certificate Monitoring
```bash
# Check certificate expiration
openssl x509 -enddate -noout -in certificate.pem

# Automated monitoring script
#!/bin/bash
CERT_FILE="/etc/letsencrypt/live/api.tihomo.com/fullchain.pem"
EXPIRY_DATE=$(openssl x509 -enddate -noout -in "$CERT_FILE" | cut -d= -f2)
EXPIRY_EPOCH=$(date -d "$EXPIRY_DATE" +%s)
CURRENT_EPOCH=$(date +%s)
DAYS_LEFT=$(( ($EXPIRY_EPOCH - $CURRENT_EPOCH) / 86400 ))

if [ $DAYS_LEFT -lt 30 ]; then
    echo "Certificate expires in $DAYS_LEFT days!"
    # Send alert
fi
```

### Health Checks
```nginx
location /health {
    access_log off;
    proxy_pass http://tihomo_gateway;
    proxy_set_header Host $host;
}
```

## 🚀 Deployment Checklist

### Pre-Deployment
- [ ] DNS records configured
- [ ] Firewall rules updated (ports 80, 443)
- [ ] SSL certificates obtained
- [ ] Environment variables configured
- [ ] Production configurations reviewed

### Deployment
- [ ] Production containers deployed
- [ ] Health checks passing
- [ ] SSL certificates valid
- [ ] HTTPS redirects working
- [ ] Security headers present

### Post-Deployment
- [ ] Certificate renewal scheduled
- [ ] Monitoring configured
- [ ] Performance testing completed
- [ ] Security scanning passed
- [ ] Backup procedures documented

## 🔄 Maintenance

### Regular Tasks
- **Certificate Monitoring**: Check expiration dates
- **Security Updates**: Keep Nginx and dependencies updated
- **Log Rotation**: Manage log file sizes
- **Performance Monitoring**: Monitor response times
- **Backup Verification**: Test backup procedures

### Troubleshooting

#### Common Issues
1. **Certificate Renewal Failures**
   ```bash
   # Check certificate status
   certbot certificates
   
   # Manual renewal
   certbot renew --dry-run
   ```

2. **SSL Configuration Errors**
   ```bash
   # Test Nginx configuration
   nginx -t
   
   # Check SSL configuration
   openssl s_client -connect api.tihomo.com:443
   ```

3. **Performance Issues**
   ```bash
   # Check proxy performance
   curl -w "@curl-format.txt" https://api.tihomo.com/health
   ```

## 📚 Additional Resources

- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
- [Nginx SSL Configuration](https://nginx.org/en/docs/http/configuring_https_servers.html)
- [Mozilla SSL Configuration Generator](https://ssl-config.mozilla.org/)
- [SSL Labs Server Test](https://www.ssllabs.com/ssltest/)

## 🎯 Recommendation

For TiHoMo project, **Approach 1 (Reverse Proxy SSL Termination)** is recommended because:

1. **Cost-Effective**: Free Let's Encrypt certificates
2. **Proven Solution**: Industry-standard approach
3. **Flexibility**: Easy to modify and scale
4. **Security**: Additional security layer with rate limiting
5. **Performance**: Optimized for web applications
6. **Maintenance**: Reasonable complexity for small teams

The complete implementation files are provided in this repository and can be deployed immediately after DNS configuration. 