# RentalCarApplication


## Описание
Настольное WPF-приложение для автоматизации аренды автомобилей. 
Разделение ролей:
*   **Клиент:** каталог с фильтрацией, бронирование, загрузка документов, отзывы.
*   **Администратор:** управление авто (CRUD), заказами, клиентами и модерация отзывов.

## Стек технологий
*   **Язык:** C#
*   **Платформа:** .NET 8.0, WPF (XAML)
*   **Архитектура:** MVVM, Repository, Unit of Work
*   **БД:** MS SQL Server, Entity Framework Core 8.0
*   **Среда:** Visual Studio 2022

## Структура
*   `RentalCarApplication` — UI, ViewModels, команды, инфраструктура.
*   `RentalCarApplication.Core` — бизнес-сущности и интерфейсы.
*   `RentalCarApplication.EntityFramework` — контекст БД и репозитории.

**Данные администратора по умолчанию:**
*   Email: `lebedzpolina@gmail.com`
*   Пароль: `Adm1nk4a`

## База данных
Таблицы: `Users`, `Cars`, `Orders`, `Reviews`. Связи реализованы через внешние ключи (Email, CarId, OrderId).
