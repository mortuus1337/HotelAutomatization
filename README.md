# HotelAutomatization



\##

1. \[Описание проекта](#description)
2. \[Скриншоты](#screenshots)
3. \[Запуск](#start)







<a id="description"></a>

\## Описание

Проект создан с использованием .................

\### Основные функции

\* 





\_\_\_

<a id="screenshots"></a>

\## Скриншоты

!\[Image alt](img/1.png)



\_\_\_

<a id="start"></a>

\## Запуск

Для запуска проект нужно сделать следующие шаги.

1\. Устанавливаем зависимости для php и js

```bash

composer install

npm install

```

2\. Делаем сборку

```bash

npm run build

```

3\. Изменяем .env.examle на .env и заполняем необходимые настройки бд

4\. Генерируем ключ шифрования

```bash

php artisan key:generate

```

5\. Делаем миграцию.

```bash

php artisan migrate

```

6\. Заполняем бд данными

```bash

php artisan db:seed

```

7\. Добавляем ссылку на картинки

```bash

php artisan storage:link

```

8\. Запускаем проект

```bash

php artisan serve

```



Начальный аккаунт администратора  

email: test@gmail.com  

password: password



