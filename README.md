# Лабораторна робота №3 — CI/CD
 
Проєкт — веб-застосунок **Task Tracker** 
 
---
### Лебедєва Софія ІМ-44
---
 
## Варіант індивідуального завдання
 
**Номер варіанту:** N = 13
 
| Параметр | Формула | Значення | Опис |
|----------|---------|----------|------|
| V2 | (13 % 2) + 1 | **2** | Файл конфігурації `/etc/mywebapp/config.json`, БД — PostgreSQL |
| V3 | (13 % 3) + 1 | **2** | Тип застосунку — Task Tracker |
| V5 | (13 % 5) + 1 | **4** | Порт застосунку — `8000` |
 
---
 
## Документація по веб-застосунку
 
## Стек
 
C# (.NET 10), ASP.NET Core, EF Core, PostgreSQL, nginx, systemd, Vagrant, Docker.
 
### API Endpoints
 
| Метод | Шлях | Опис |
|-------|------|------|
| `GET` | `/` | HTML-сторінка зі списком ендпоінтів |
| `GET` | `/tasks` | Отримати список усіх задач |
| `POST` | `/tasks` | Створити нову задачу. Body: `{"title": "..."}` |
| `POST` | `/tasks/{id}/done` | Позначити задачу як виконану |
| `GET` | `/health/alive` | Health check (тільки внутрішньо) |
| `GET` | `/health/ready` | Health check з перевіркою БД (тільки внутрішньо) |
 
Ендпоінти `/health/*` доступні тільки локально — nginx закриває їх ззовні.
 
#### Приклад використання
 
Створити задачу:
 
Linux / macOS / WSL:
```bash
curl -X POST http://localhost:8080/tasks \
     -H "Content-Type: application/json" \
     -d '{"title": "Створити задачу"}'
```
 
PowerShell:
```powershell
curl.exe -X POST http://localhost:8080/tasks `
     -H "Content-Type: application/json" `
     -d '{\"title\": \"Створити задачу\"}'
```
 
Відповідь:
```json
{
  "id": 1,
  "title": "Створити задачу",
  "status": "pending",
  "created_at": "2026-04-28T22:05:18Z"
}
```
 
Позначити виконаною:
 
Linux / macOS / WSL:
```bash
curl -X POST http://localhost:8080/tasks/1/done
```
 
PowerShell:
```powershell
curl.exe -X POST http://localhost:8080/tasks/1/done
```
 
---
  
## Документація по розгортанню
 
### Базовий образ ВМ
 
Використовується офіційний образ **Ubuntu 24.04 LTS** від проекту [Bento](https://github.com/chef/bento) (`bento/ubuntu-24.04`). Vagrant завантажує його автоматично при першому запуску.
 
### Вимоги до ресурсів ВМ (runner та target)
 
| Ресурс | Значення |
|--------|----------|
| CPU | 1 ядро |
| RAM | 1024 MB |
 
 
### Передумови на хост-машині (Windows/macOS/Linux)
 
1. **VirtualBox** — [virtualbox.org](https://www.virtualbox.org)
2. **Vagrant** — [vagrantup.com](https://www.vagrantup.com)
3. **Git** — для клонування репозиторію
### Як завантажити та запустити автоматизацію
 
```bash
# 1. Клонувати репозиторій
git clone https://github.com/Sofi-fi-fi/deployment-course.git
cd deployment-course
 
# 2. Запустити ВМ 'runner' і 'target' 
vagrant up
```
 