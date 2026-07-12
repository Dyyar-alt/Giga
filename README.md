# Giga — Кроссплатформенный клиент для GigaChat

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/MAUI-10.0-blue.svg)](https://github.com/dotnet/maui)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Giga** — это кроссплатформенное приложение для работы с нейросетевой моделью GigaChat от Сбера. Позволяет общаться с ИИ, генерировать изображения, сохранять историю и отслеживать использование токенов. Работает на Windows и Android.

---

## 📱 Поддерживаемые платформы

| Платформа | Версия | Статус |
|-----------|--------|--------|
| **Windows** | 10 / 11 | ✅ Полностью поддерживается |
| 📱 **Android** | 8.0+ (API 26+) | ✅ Полностью поддерживается |
| 🍏 **macOS** | (планируется) | ⏳ В разработке |

---

## 🚀 Возможности

- 💬 **Чат с ИИ** — Общайтесь с GigaChat в удобном интерфейсе
- 🎨 **Генерация изображений** — Создавайте картинки по текстовому описанию
- 💾 **История диалогов** — Все сессии сохраняются локально в JSON
- 📚 **История генераций** — Сохраняйте промпты и изображения
- 🔢 **Подсчёт токенов** — Контролируйте использование API
- 📊 **Глобальная статистика** — Общая информация о работе приложения
- 🌙 **Тёмная тема** — Поддержка системной темы (планируется)

---

## 🔑 Как получить ключ API GigaChat

Для работы приложения требуется ключ API, который можно получить бесплатно в **Сбер разработке**.

### Шаг 1: Регистрация
1. Перейдите на сайт [developers.sber.ru](https://developers.sber.ru/)
2. Нажмите **«Войти со СберId»**


### Шаг 2: Создание проекта
1. В личном кабинете перейдите в раздел **«Мои проекты»**
2. Нажмите **«Создать проект»**
3. Выберите тип **«API»**
4. Укажите название проекта (например, «Giga Client»)
5. Выберите **«GigaChat»** в качестве продукта

### Шаг 3: Получение ключа
1. В созданном проекте перейдите в **«Настройки»** → **«Ключи доступа»**
2. Нажмите **«Создать ключ»**
3. Выберите **«API Key»**
4. Скопируйте сгенерированный ключ и **сохраните его в надёжном месте** (ключ показывается только один раз!)

> ⚠️ **Важно!** Утерянный ключ невозможно восстановить. Придётся создавать новый.

---

## 📦 Сборка проекта

### Требования
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022 17.13+](https://visualstudio.microsoft.com/vs/) с рабочими нагрузками:
  - .NET Multi-platform App UI (MAUI)
  - Разработка приложений для Android
  - Разработка классических приложений .NET (для Windows)

### Клонирование репозитория
```bash
git clone https://github.com/Dyyar-alt/Giga.git
cd Giga

## 🔑 Как получить ключ API GigaChat

Для работы приложения требуется ключ API, который можно получить бесплатно в **Сбер разработке**.

### Шаг 1: Регистрация
1. Перейдите на сайт [developers.sber.ru](https://developers.sber.ru/)
2. Нажмите **«Войти»** → **«Зарегистрироваться»**
3. Заполните форму регистрации и подтвердите email

### Шаг 2: Создание проекта
1. В личном кабинете перейдите в раздел **«Мои проекты»**
2. Нажмите **«Создать проект»**
3. Выберите тип **«API»**
4. Укажите название проекта (например, «Giga Client»)
5. Выберите **«GigaChat»** в качестве продукта

### Шаг 3: Получение ключа
1. В созданном проекте перейдите в **«Настройки»** → **«Ключи доступа»**
2. Нажмите **«Создать ключ»**
3. Выберите **«API Key»**
4. Скопируйте сгенерированный ключ и **сохраните его в надёжном месте** (ключ показывается только один раз!)

> ⚠️ **Важно!** Утерянный ключ невозможно восстановить. Придётся создавать новый.

---

## 📦 Сборка проекта

### Требования
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022 17.13+](https://visualstudio.microsoft.com/vs/) с рабочими нагрузками:
  - .NET Multi-platform App UI (MAUI)
  - Разработка приложений для Android
  - Разработка классических приложений .NET (для Windows)

### Клонирование репозитория
```bash
git clone https://github.com/Dyyar-alt/Giga.git
cd Giga
Сборка для Windows
bash
# Release сборка
dotnet build -c Release -f net10.0-windows10.0.19041.0

# Запуск
dotnet run -f net10.0-windows10.0.19041.0
Сборка для Android
bash
# Release сборка с подписью
dotnet build -c Release -f net10.0-android

# Создание APK
dotnet publish -c Release -f net10.0-android
Запуск из Visual Studio
Откройте Giga.slnx (или создайте .sln через VS)

Выберите нужную платформу в выпадающем списке:

Windows Machine — для Windows

Android Emulator или Android Device — для Android

Нажмите F5 для запуска с отладкой

🔐 Подпись Android-приложения
Для публикации APK требуется подпись цифровым сертификатом. Если у вас нет своего сертификата, создайте его в Visual Studio:

В Solution Explorer → правой кнопкой по проекту → Свойства

Перейдите на вкладку «Android» → «Подпись»

Нажмите «Создать тестовый сертификат...»

Заполните поля (пароль запомните)

Нажмите «ОК»

Или через командную строку (создайте myapp.keystore):

bash
keytool -genkey -v -keystore myapp.keystore -alias myapp -keyalg RSA -keysize 2048 -validity 10000
В Giga.csproj укажите:

xml
<PropertyGroup>
  <AndroidKeyStore>true</AndroidKeyStore>
  <AndroidSigningKeyStore>myapp.keystore</AndroidSigningKeyStore>
  <AndroidSigningKeyAlias>myapp</AndroidSigningKeyAlias>
  <AndroidSigningKeyPass>your_password</AndroidSigningKeyPass>
  <AndroidSigningStorePass>your_password</AndroidSigningStorePass>
</PropertyGroup>
⚠️ Никогда не загружайте .keystore и .pfx в публичный репозиторий! Добавьте их в .gitignore.

📂 Структура проекта
text
📁 Giga/
├── 📁 Converters/          # Конвертеры значений для UI
├── 📁 Helpers/             # Вспомогательные утилиты
├── 📁 Models/              # Модели данных (ChatMessage, ChatSession)
├── 📁 Platforms/           # Платформозависимый код (Android, Windows)
├── 📁 Resources/           # Иконки, шрифты, изображения
├── 📁 SDK/                 # Неофициальная сборка GigaChat SDK
├── 📁 Services/            # Сервисы (ApiKey, ChatStorage, GigaChat)
├── 📁 ViewModels/          # Модели представления (MVVM)
├── 📁 Views/               # Страницы (ChatPage, ImageGenPage)
├── 📄 App.xaml             # Корневой Application
├── 📄 App.xaml.cs
├── 📄 AppShell.xaml        # Навигация
├── 📄 AppShell.xaml.cs
├── 📄 Giga.csproj          # Файл проекта
├── 📄 MauiProgram.cs       # Настройка DI и сервисов
└── 📄 README.md
🛠️ Используемые технологии
Компонент	Технология
Фреймворк	.NET MAUI 10.0
Язык	C# 12
Архитектура	MVVM
MVVM-библиотека	CommunityToolkit.Mvvm 8.4.2
SDK	Неофициальная сборка GigaChatSDK
Хранение	Preferences, JSON
UI	XAML
🤝 Участие в разработке
Форкните репозиторий

Создайте ветку для фичи: git checkout -b feature/amazing-feature

Сделайте коммит: git commit -m 'Add amazing feature'

Запушьте: git push origin feature/amazing-feature

Откройте Pull Request

📝 Лицензия
Проект распространяется под лицензией MIT. Подробности в файле LICENSE.

🙏 Благодарности
GigaChat за предоставленный API

.NET MAUI за кроссплатформенный фреймворк

Всем друзьям-айтишникам за идеи и тестирование! 😊

Разработано с ❤️ для друзей-айтишников 🚀

