-- ============================================
-- 30 навыков
-- ============================================
INSERT INTO skills ("Id", "Name", "Category") VALUES
(gen_random_uuid(), 'JavaScript', 'tech'),
(gen_random_uuid(), 'React', 'tech'),
(gen_random_uuid(), 'Node.js', 'tech'),
(gen_random_uuid(), 'Python', 'tech'),
(gen_random_uuid(), 'C#', 'tech'),
(gen_random_uuid(), 'Английский язык', 'languages'),
(gen_random_uuid(), 'Французский язык', 'languages'),
(gen_random_uuid(), 'Немецкий язык', 'languages'),
(gen_random_uuid(), 'Испанский язык', 'languages'),
(gen_random_uuid(), 'Итальянский язык', 'languages'),
(gen_random_uuid(), 'Гитара', 'music'),
(gen_random_uuid(), 'Фортепиано', 'music'),
(gen_random_uuid(), 'Барабаны', 'music'),
(gen_random_uuid(), 'Вокал', 'music'),
(gen_random_uuid(), 'Сведение треков', 'music'),
(gen_random_uuid(), 'Йога', 'sports'),
(gen_random_uuid(), 'Фитнес', 'sports'),
(gen_random_uuid(), 'Пилатес', 'sports'),
(gen_random_uuid(), 'Теннис', 'sports'),
(gen_random_uuid(), 'Шахматы', 'sports'),
(gen_random_uuid(), 'Фотография', 'art'),
(gen_random_uuid(), 'Рисование', 'art'),
(gen_random_uuid(), 'Photoshop', 'art'),
(gen_random_uuid(), 'Дизайн интерьеров', 'art'),
(gen_random_uuid(), 'Кулинария', 'cooking'),
(gen_random_uuid(), 'Итальянская кухня', 'cooking'),
(gen_random_uuid(), 'Выпечка', 'cooking'),
(gen_random_uuid(), 'Суши', 'cooking'),
(gen_random_uuid(), 'Кондитерское дело', 'cooking'),
(gen_random_uuid(), 'Психология', 'other')
ON CONFLICT ("Name") DO NOTHING;

-- ============================================
-- 10 тестовых пользователей
-- ============================================

-- 1. Анна Кузнецова
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000001', 'anna@mail.com', 'Анна Кузнецова', 'Москва', 'Frontend-разработчик. Люблю React и красивый UI.', false, NOW());

-- 2. Дмитрий Волков
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000002', 'dmitry@mail.com', 'Дмитрий Волков', 'Санкт-Петербург', 'Программист и музыкант. Играю на гитаре 10 лет.', true, NOW());

-- 3. Елена Морозова
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000003', 'elena@mail.com', 'Елена Морозова', 'Казань', 'Преподаватель французского. Жила в Париже 5 лет.', false, NOW());

-- 4. Михаил Иванов
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000004', 'mikhail@mail.com', 'Михаил Иванов', 'Новосибирск', 'Спортивный тренер, йога-инструктор. ЗОЖ — мой образ жизни.', true, NOW());

-- 5. Ольга Петрова
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000005', 'olga@mail.com', 'Ольга Петрова', 'Екатеринбург', 'Шеф-повар итальянского ресторана. Научу готовить как профи.', false, NOW());

-- 6. Сергей Соколов
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000006', 'sergey@mail.com', 'Сергей Соколов', 'Москва', 'Фотограф с 8-летним стажем. Снимаю свадьбы и портреты.', true, NOW());

-- 7. Мария Белова
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000007', 'maria@mail.com', 'Мария Белова', 'Краснодар', 'Художница. Рисую маслом и акварелью. Веду мастер-классы.', false, NOW());

-- 8. Алексей Громов
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000008', 'alexey@mail.com', 'Алексей Громов', 'Воронеж', 'Backend-разработчик на C# и Node.js. Могу научить с нуля.', false, NOW());

-- 9. Татьяна Волкова
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000009', 'tatiana@mail.com', 'Татьяна Волкова', 'Самара', 'Кондитер. Торты, капкейки, macarons. Всё handmade.', true, NOW());

-- 10. Игорь Никитин
INSERT INTO users ("Id", "Email", "Name", "City", "Bio", "IsPremium", "CreatedAt") VALUES
('a1000000-0000-0000-0000-000000000010', 'igor@mail.com', 'Игорь Никитин', 'Москва', 'Python-разработчик и Data Scientist. Помогу с анализом данных.', false, NOW());

-- ============================================
-- Навыки пользователей (offer — чему учит, seek — чему хочет научиться)
-- ============================================

-- Анна: учит React, хочет английский и йогу
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000001', "Id", 'offer' FROM skills WHERE "Name" = 'React'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000001', "Id", 'offer' FROM skills WHERE "Name" = 'JavaScript'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000001', "Id", 'seek' FROM skills WHERE "Name" = 'Английский язык'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000001', "Id", 'seek' FROM skills WHERE "Name" = 'Йога';

-- Дмитрий: учит гитаре и шахматам, хочет французский
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000002', "Id", 'offer' FROM skills WHERE "Name" = 'Гитара'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000002', "Id", 'offer' FROM skills WHERE "Name" = 'Шахматы'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000002', "Id", 'seek' FROM skills WHERE "Name" = 'Французский язык';

-- Елена: учит французскому, хочет рисование
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000003', "Id", 'offer' FROM skills WHERE "Name" = 'Французский язык'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000003', "Id", 'seek' FROM skills WHERE "Name" = 'Рисование';

-- Михаил: учит йоге и фитнесу, хочет психологию
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000004', "Id", 'offer' FROM skills WHERE "Name" = 'Йога'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000004', "Id", 'offer' FROM skills WHERE "Name" = 'Фитнес'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000004', "Id", 'seek' FROM skills WHERE "Name" = 'Психология';

-- Ольга: учит итальянской кухне и выпечке, хочет английский
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000005', "Id", 'offer' FROM skills WHERE "Name" = 'Итальянская кухня'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000005', "Id", 'offer' FROM skills WHERE "Name" = 'Выпечка'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000005', "Id", 'seek' FROM skills WHERE "Name" = 'Английский язык';

-- Сергей: учит фотографии и Photoshop, хочет немецкий
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000006', "Id", 'offer' FROM skills WHERE "Name" = 'Фотография'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000006', "Id", 'offer' FROM skills WHERE "Name" = 'Photoshop'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000006', "Id", 'seek' FROM skills WHERE "Name" = 'Немецкий язык';

-- Мария: учит рисованию, хочет фото
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000007', "Id", 'offer' FROM skills WHERE "Name" = 'Рисование'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000007', "Id", 'seek' FROM skills WHERE "Name" = 'Фотография';

-- Алексей: учит C# и Node.js, хочет английский и теннис
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000008', "Id", 'offer' FROM skills WHERE "Name" = 'C#'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000008', "Id", 'offer' FROM skills WHERE "Name" = 'Node.js'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000008', "Id", 'seek' FROM skills WHERE "Name" = 'Английский язык'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000008', "Id", 'seek' FROM skills WHERE "Name" = 'Теннис';

-- Татьяна: учит кондитерскому делу, хочет фото
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000009', "Id", 'offer' FROM skills WHERE "Name" = 'Кондитерское дело'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000009', "Id", 'seek' FROM skills WHERE "Name" = 'Фотография';

-- Игорь: учит Python, хочет React
INSERT INTO user_skills ("UserId", "SkillId", "Type")
SELECT 'a1000000-0000-0000-0000-000000000010', "Id", 'offer' FROM skills WHERE "Name" = 'Python'
UNION ALL
SELECT 'a1000000-0000-0000-0000-000000000010', "Id", 'seek' FROM skills WHERE "Name" = 'React';