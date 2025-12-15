# LocalChat - Микросервисный чат-сервис

Прототип чат-сервиса на микросервисной архитектуре.

## Архитектура

- **Auth Service** (порт 5001) - регистрация и авторизация
- **Chat Service** (порт 5002) - отправка и хранение сообщений  
- **Realtime Hub** (порт 5003) - мгновенная доставка через SignalR
- **SQL Server** (порт 1433) - база данных
- **RabbitMQ** (порт 5672/15672) - асинхронные сообщения
- **Prometheus** (порт 9090) - сбор метрик
- **Grafana** (порт 3000) - визуализация метрик

## Запуск проекта

### Требования:
- Docker Desktop
- Docker Compose

### Запуск:
```bash 
cd deploy
docker-compose up --build
```

### Остановка:
```bash 
docker-compose down
```


## Доступ к сервисам

| Сервис | URL | Порт | Доступ |
|--------|-----|------|--------|
| Auth Service | http://localhost:5001/swagger | 5001 | Swagger UI |
| Chat Service | http://localhost:5002/swagger | 5002 | Swagger UI |
| Realtime Hub | http://localhost:5003/api/health | 5003 | Health Check |
| RabbitMQ Management | http://localhost:15672 | 15672 | guest/guest |
| Prometheus | http://localhost:9090 | 9090 | Metrics |
| Grafana | http://localhost:3000 | 3000 | admin/admin |
| SQL Server | localhost,1433 | 1433 | sa/Your_password123 |

## Использование API

### 1. Регистрация пользователя

```bash 
POST http://localhost:5001/api/auth/register
Content-Type: application/json

{
"username": "testuser",
"password": "password123"
}
```

### 2. Отправка сообщения
```bash 
POST http://localhost:5002/api/messages/send
Content-Type: application/json

{
"text": "Hello World",
"senderId": 1,
"senderName": "testuser"
}
```

### 3. Получение истории сообщений
```bash 
GET http://localhost:5002/api/messages/history?limit=50
```


### 4. Подключение к чату в реальном времени
Используйте SignalR клиент для подключения к `http://localhost:5003/chatHub`

## Технологии

- **Backend:** C#, ASP.NET Core (.NET 7)
- **База данных:** MS SQL Server
- **Очереди:** RabbitMQ
- **Реалтайм:** SignalR (WebSocket)
- **Контейнеризация:** Docker, Docker Compose
- **Мониторинг:** Prometheus + Grafana
- **CI/CD:** GitHub Actions

