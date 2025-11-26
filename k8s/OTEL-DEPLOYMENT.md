# OpenTelemetry Collector Deployment Guide

## Dosyalar

- `otel-collector-config.yaml` - OpenTelemetry Collector ConfigMap
- `otel-collector-deployment.yaml` - OpenTelemetry Collector Deployment
- `otel-collector-service.yaml` - OpenTelemetry Collector Service
- `jaeger-deployment.yaml` - Jaeger All-in-One (opsiyonel - trace görselle?tirme için)
- `dapr-config.yaml` - Dapr Configuration (OpenTelemetry entegrasyonu)

## Deployment S?ras?

### 1. OpenTelemetry Collector'? Deploy Edin

```bash
# ConfigMap'i olu?tur
kubectl apply -f k8s/otel-collector-config.yaml

# Deployment ve Service'i olu?tur
kubectl apply -f k8s/otel-collector-deployment.yaml
kubectl apply -f k8s/otel-collector-service.yaml

# Durumu kontrol et
kubectl get pods -l app=otel-collector
kubectl logs -l app=otel-collector -f
```

### 2. Jaeger'i Deploy Edin (Opsiyonel - Trace Görselle?tirme)

```bash
kubectl apply -f k8s/jaeger-deployment.yaml

# Durumu kontrol et
kubectl get pods -l app=jaeger
kubectl get svc jaeger-query

# Jaeger UI'a eri?im (NodePort üzerinden)
# http://localhost:30686
```

### 3. Dapr Configuration'? Güncelle

```bash
# Dapr config'i güncelle
kubectl apply -f k8s/dapr-config.yaml

# Dapr podlar?n? yeniden ba?lat (de?i?ikliklerin al?nmas? için)
kubectl rollout restart deployment daprone-service-api
kubectl rollout restart deployment daprtwo-service-api
```

## Do?rulama

### OpenTelemetry Collector Sa?l?k Kontrolü

```bash
# Health check endpoint
kubectl port-forward svc/otel-collector 13133:13133
curl http://localhost:13133

# ZPages (telemetri debug)
kubectl port-forward svc/otel-collector 55679:55679
# http://localhost:55679/debug/tracez
```

### Prometheus Metrikleri

```bash
# OpenTelemetry Collector kendi metriklerini expose eder
kubectl port-forward svc/otel-collector 8888:8888
curl http://localhost:8888/metrics

# Dapr metriklerini expose eder
kubectl port-forward svc/otel-collector 8889:8889
curl http://localhost:8889/metrics
```

### Jaeger UI

```bash
# NodePort üzerinden eri?im
http://<node-ip>:30686

# Veya port-forward ile
kubectl port-forward svc/jaeger-query 16686:16686
# http://localhost:16686
```

## Test Senaryosu

### 1. Publisher Servisinden Mesaj Gönder

```bash
# DaprOne Service'e istek at
kubectl port-forward svc/daprone-service-api 8080:80

curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "TestUser",
    "email": "test@example.com"
  }'
```

### 2. Trace'leri Jaeger'da Görüntüle

1. Jaeger UI'a git: http://localhost:16686
2. Service dropdown'dan `daprone-service-api` seç
3. "Find Traces" butonuna t?kla
4. Publish ve Subscribe trace'lerini görmelisiniz

### 3. Metrikleri Kontrol Et

```bash
# OpenTelemetry Collector metriklerini çek
kubectl port-forward svc/otel-collector 8889:8889
curl http://localhost:8889/metrics | grep dapr
```

## Beklenen Trace Ak???

```
???????????????????       ????????????????????       ???????????????????
? DaprOne Service ????????? RabbitMQ Pub/Sub ????????? DaprTwo Service ?
???????????????????       ????????????????????       ???????????????????
        ?                                                      ?
        ?                                                      ?
        ????????????????????????????????????????????????????????
                                ?
                                ?
                    ??????????????????????????
                    ? OpenTelemetry Collector?
                    ??????????????????????????
                                ?
                                ?
                         ???????????????
                         ?   Jaeger    ?
                         ???????????????
```

## Troubleshooting

### OpenTelemetry Collector Loglar?

```bash
kubectl logs -l app=otel-collector -f
```

### Dapr Sidecar Loglar?

```bash
# DaprOne sidecar
kubectl logs <daprone-pod-name> -c dappr

# DaprTwo sidecar
kubectl logs <daprtwo-pod-name> -c dappr
```

### Common Issues

**Problem:** Trace'ler görünmüyor  
**Çözüm:** 
- Dapr config'in do?ru uyguland???n? kontrol edin: `kubectl get configuration`
- Podlar? yeniden ba?lat?n: `kubectl rollout restart deployment`

**Problem:** OpenTelemetry Collector ba?lam?yor  
**Çözüm:**
- ConfigMap'in do?ru yüklendi?ini kontrol edin: `kubectl get configmap otel-collector-config -o yaml`
- Pod loglar?n? kontrol edin: `kubectl logs -l app=otel-collector`

**Problem:** Jaeger'a eri?ilemiyor  
**Çözüm:**
- Service'in do?ru expose edildi?ini kontrol edin: `kubectl get svc jaeger-query`
- Node IP'yi do?ru kulland???n?zdan emin olun: `kubectl get nodes -o wide`

## Ek Yap?land?rmalar

### Prometheus ile Entegrasyon

E?er ayr? bir Prometheus instance'?n?z varsa, otel-collector-config.yaml'da exporters bölümünü güncelleyin:

```yaml
exporters:
  prometheusremotewrite:
    endpoint: "http://prometheus-server:9090/api/v1/write"
```

### Production için Öneriler

1. **Resource Limits:** Production'da daha yüksek limit ayarlay?n
2. **Replicas:** HA için birden fazla replica kullan?n
3. **Persistent Storage:** Jaeger için persistent volume ekleyin
4. **TLS:** Güvenli ileti?im için TLS aktifle?tirin
5. **Sampling Rate:** `dapr-config.yaml`'da samplingRate'i dü?ürün (örn. "0.1" = %10)

## Referanslar

- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/)
- [Dapr Observability](https://docs.dapr.io/operations/observability/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
