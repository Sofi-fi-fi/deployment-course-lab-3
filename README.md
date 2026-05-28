# Лабораторна робота №1, №2
 
Проєкт — веб-застосунок **Task Tracker** з автоматизованим розгортанням на віртуальній машині через Vagrant (Лаб. 1) та у контейнерах через Docker Compose (Лаб. 2).
 
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
 
## Структура репозиторію
 
```
deployment-course/
├── config/
│   ├── config.json              # Конфігурація для Vagrant (127.0.0.1)
│   └── config.docker.json       # Конфігурація для Docker (0.0.0.0, host=db)
├── migrations/
│   └── 001_init.sql             # SQL-міграція (створення таблиці tasks)
├── nginx/
│   ├── mywebapp.conf            # Конфігурація nginx для Vagrant
│   └── mywebapp.docker.conf     # Конфігурація nginx для Docker
├── scripts/
│   ├── install.sh               # Скрипт автоматичного розгортання (Vagrant)
│   └── migrate.sh               # Скрипт міграції БД (Vagrant)
├── src/
│   └── mywebapp/                # Вихідний код C# застосунку (.NET 10)
├── Dockerfile                   # Образ застосунку для Docker
├── docker-compose.yml           # Опис сервісів для Docker Compose
├── Vagrantfile                  # Опис віртуальної машини (Vagrant)
└── README.md                    # Документація
```
 
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
 
---
 
# Лабораторна робота №1 — Vagrant
 
## Документація по розгортанню
 
### Базовий образ ВМ
 
Використовується офіційний образ **Ubuntu 24.04 LTS** від проекту [Bento](https://github.com/chef/bento) (`bento/ubuntu-24.04`). Vagrant завантажує його автоматично при першому запуску.
 
### Вимоги до ресурсів ВМ
 
| Ресурс | Значення |
|--------|----------|
| CPU | 2 ядра |
| RAM | 2048 MB |
 
 
### Передумови на хост-машині (Windows/macOS/Linux)
 
1. **VirtualBox** — [virtualbox.org](https://www.virtualbox.org)
2. **Vagrant** — [vagrantup.com](https://www.vagrantup.com)
3. **Git** — для клонування репозиторію
### Як завантажити та запустити автоматизацію
 
```bash
# 1. Клонувати репозиторій
git clone https://github.com/Sofi-fi-fi/deployment-course.git
cd deployment-course
 
# 2. Запустити розгортання 
vagrant up
```
 
### Як увійти на ВМ
 
```bash
vagrant ssh
```
 
Після першого розгортання користувач `vagrant` блокується. Для входу використовуйте інших користувачів через `sudo` або через консоль VirtualBox:
 
| Користувач | Пароль | Призначення |
|------------|--------|-------------|
| `student` | `student` | Адміністративний (sudo). Зміна пароля при першому вході. |
| `teacher` | `12345678` | Перевірка роботи (sudo). Зміна пароля при першому вході. |
| `app` | — | Системний, запускається застосунком |
| `operator` | `12345678` | Обмежений sudo для управління сервісом mywebapp і nginx. Зміна пароля при першому вході. |
 
### Корисні команди Vagrant
 
```bash
vagrant up          # Створити та запустити ВМ
vagrant ssh         # Підключитися по SSH
vagrant halt        # Зупинити ВМ (без видалення)
vagrant reload      # Перезапустити ВМ (застосувати зміни Vagrantfile)
vagrant provision   # Повторно запустити install.sh без перестворення ВМ
vagrant destroy -f  # Видалити ВМ повністю
```
 
## Інструкція з тестування (Vagrant)
 
### 1. Перевірка веб-застосунку через браузер на хості
 
Відкрити в браузері: **http://localhost:8080**
 
Має відобразитися HTML-сторінка зі списком API-ендпоінтів.
 
### 2. Перевірка списку задач
 
Відкрити: **http://localhost:8080/tasks**
 
Має відобразитися пуста таблиця.
 
### 3. Створення задачі
 
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

Знову відкрити (оновити): **http://localhost:8080/tasks**

Має відобразитися таблиця зі створеною задачею.

### 4. Позначення задачі як виконаної
 
Linux / macOS / WSL:
```bash
curl -X POST http://localhost:8080/tasks/1/done
```
 
PowerShell:
```
curl.exe -X POST http://localhost:8080/tasks/1/done
```
 
У відповіді має повернутись JSON зі зміненим статусом `"status": "done"`.

### 5. Перевірка що health-ендпоінти закриті ззовні
 
Відкрити в браузері:
- http://localhost:8080/health/alive — має повернути `404`
- http://localhost:8080/health/ready — має повернути `404`
### 6. Перевірка статусу сервісів на ВМ
 
```bash
vagrant ssh
sudo systemctl status mywebapp
sudo systemctl status nginx
sudo systemctl status postgresql
```
 
Усі три сервіси повинні бути `active (running)`.
 
### 7. Перевірка користувачів та їх прав
 
```bash
# Усі створені користувачі
getent passwd student teacher app operator
 
# Хто в групі sudo
getent group sudo
 
# Sudo-правила оператора
sudo cat /etc/sudoers.d/operator
 
# Чи заблокований vagrant
sudo passwd -S vagrant
```
 
### 8. Перевірка файлу gradebook
 
```bash
sudo cat /home/student/gradebook
# Має вивести: 13
```
 
### 9. Перевірка обмеженого sudo для operator
 
```bash
sudo -u operator sudo -l
```
 
Має показати дозволені команди (управління mywebapp і reload nginx).


### Завершення роботи
 
Щоб повністю видалити віртуальну машину і всі її дані:
 
```bash
vagrant destroy -f
```
 
Якщо папку з проєктом вже видалено і `vagrant destroy` недоступний — потрібно видалити ВМ вручну через VirtualBox.
 
---
 
---
 
# Лабораторна робота №2 — Docker
 
## Документація по розгортанню
 
### Архітектура контейнерів
 
Застосунок розгортається за допомогою **Docker Compose** і складається з трьох сервісів, об'єднаних у внутрішню мережу `task_network`:
 
| Сервіс | Образ | Призначення |
|--------|-------|-------------|
| `db` | `postgres:16-alpine` | База даних PostgreSQL |
| `webapp` | Збирається з `Dockerfile` | ASP.NET Core застосунок (.NET 10) |
| `nginx` | `nginx:alpine` | Reverse proxy, відкриває порт `8080` назовні |
 
Webapp не виставляє жодного порту назовні напряму — весь трафік іде через nginx. Ендпоінти `/health/*` закриті так само, як і у Vagrant-варіанті.
 
### Передумови на хост-машині (Windows/macOS/Linux)
 
1. **Docker** — [docs.docker.com/get-docker](https://docs.docker.com/get-docker/)
2. **Docker Compose** — входить до складу Docker Desktop; для Linux: [docs.docker.com/compose/install](https://docs.docker.com/compose/install/)
3. **Git** — для клонування репозиторію
 
### Як завантажити та запустити
 
> **Важливо! — Docker має бути запущений перед виконанням будь-яких команд:**
> - **Windows (включно з WSL) / macOS:** запустіть **Docker Desktop** і дочекайтесь поки іконка у треї стане активною. Після цього `docker` стане доступним і у PowerShell, і у WSL-терміналі автоматично — жодних додаткових команд не потрібно.
> - **Linux (нативний, без Docker Desktop):** `sudo systemctl start docker`
 
```bash
# 1. Клонувати репозиторій
git clone https://github.com/Sofi-fi-fi/deployment-course.git
cd deployment-course
 
# 2. Зібрати образи та запустити всі сервіси у фоновому режимі
docker compose up -d --build
```
 
При першому запуску Docker автоматично:
- збере образ застосунку з `Dockerfile`;
- завантажить образи `postgres:16-alpine` та `nginx:alpine`;
- виконає SQL-міграцію `migrations/001_init.sql` через механізм `docker-entrypoint-initdb.d` PostgreSQL.
### Корисні команди Docker Compose
 
```bash
docker compose up -d --build   # Зібрати та запустити (фоновий режим)
docker compose up --build      # Запустити з виводом логів у консоль
docker compose down            # Зупинити та видалити контейнери (дані зберігаються у volume)
docker compose down -v         # Зупинити та видалити контейнери разом з volume (дані БД стираються)
docker compose ps              # Переглянути статус сервісів
docker compose logs -f         # Переглядати логи всіх сервісів в реальному часі
docker compose logs webapp     # Логи лише застосунку
docker compose restart webapp  # Перезапустити окремий сервіс
```
 
---
 
## Інструкція з тестування (Docker)

### 1. Перевірка статусу контейнерів
 
```bash
docker compose ps
```
 
Усі три сервіси (`db`, `webapp`, `nginx`) повинні мають відображатись і бути активними.
 
### 2. Перевірка веб-застосунку через браузер на хості
 
Відкрити в браузері: **http://localhost:8080**
 
Має відобразитися HTML-сторінка зі списком API-ендпоінтів.
 
### 3. Перевірка списку задач
 
Відкрити: **http://localhost:8080/tasks**
 
Має відобразитися порожня таблиця (або задача яка була створена до цього).
 
### 4. Створення задачі

 Linux / macOS / WSL:
```bash
curl -X POST http://localhost:8080/tasks \
     -H "Content-Type: application/json" \
     -d '{"title": "Тестова задача"}'
```
 
PowerShell:
```powershell
curl.exe -X POST http://localhost:8080/tasks `
     -H "Content-Type: application/json" `
     -d '{\"title\": \"Тестова задача\"}'
```

Знову відкрити (оновити): **http://localhost:8080/tasks**
 
Має відобразитися таблиця зі створеною задачею.

### 5. Позначення задачі як виконаної
 
Linux / macOS / WSL:
```bash
curl -X POST http://localhost:8080/tasks/1/done
```
 
PowerShell:
```
curl.exe -X POST http://localhost:8080/tasks/1/done
```
 
У відповіді має повернутись JSON зі зміненим статусом `"status": "done"`.
 
### 6. Перевірка що health-ендпоінти закриті ззовні
 
Відкрити в браузері:
- http://localhost:8080/health/alive — має повернути `404`
- http://localhost:8080/health/ready — має повернути `404`
### 7. Перевірка логів застосунку
 
```bash
docker compose logs webapp
```
 
Має бути відсутні помилки підключення до БД; при першому старті видно виконання міграції.
 
### 8. Перевірка збереження даних після перезапуску
 
```bash
# Зупинити та повторно запустити без видалення volume
docker compose down
docker compose up -d
```
 
Перевірити що задачі збереглись:
 
Linux / macOS / WSL:
```bash
curl http://localhost:8080/tasks
```
 
PowerShell / CMD:
```
curl.exe http://localhost:8080/tasks
```

 Знову відкрити: **http://localhost:8080/tasks**

Раніше створені задачі повинні залишитись — дані зберігаються у Docker volume `pgdata`.
### Завершення роботи 
 
Зупинити контейнери і видалити всі дані (включно з БД):
 
```bash
docker compose down -v
```
 
Перевірити що volume видалився:
 
```bash
docker volume ls
```
 
Якщо `deployment-course_pgdata` ще є — видалити вручну:
 
```bash
docker volume rm deployment-course_pgdata
```