# _Шашки_с_ИИ_ — учебная ознакомительная практика 2026

**Студент:** Сергеев Сергей Владимирович  
**Группа:** БИН-24-1  
**Вариант:** Б-28 
**Язык:** C#

## Описание

Игра в русские шашки против компьютерного противника, реализованная на Unity (C#) с экспортом в WebGL. Алгоритмическое ядро — поиск хода с помощью алгоритма минимакс с альфа-бета отсечением и оценочная функция позиции. Проект включает настройку глубины поиска ИИ, подсветку допустимых ходов, журнал партии и статистику игр, сохраняемые в JSON-файлы.

## Структура репозитория
```
.
├── Assets/                                         # Все игровые ресурсы и исходный код Unity
│   ├── Materials/                                  # Материалы для визуального оформления игровых объектов
│   │   ├── Cell_1.mat                              # Материал для светлых клеток доски
│   │   ├── Cell_2.mat                              # Материал для тёмных клеток доски
│   │   ├── Checker.mat                             # Материал для шашек
│   │   ├── SelectedCell.mat                        # Материал для подсветки клетки, возможных ходов
│   │   └── Side.mat                                # Материал для бортиков доски
│   ├── Models/                                     # 3D-модели и префабы игровых объектов
│   │   ├── Prefabs/                                # Готовые сборные объекты (префабы)
│   │   │   ├── Cell.prefab                         # Префаб клетки доски
│   │   │   ├── Checker.prefab                      # Префаб шашки
│   │   │   ├── Corner.prefab                       # Префаб углового элемента доски
│   │   │   ├── HistoryRow.prefab                   # Префаб строки для журнала партии
│   │   │   └── Side.prefab                         # Префаб бокового элемента доски
│   │   ├── Cell.blend                              # Исходная 3D-модель клетки (Blender)
│   │   ├── Checker.blend                           # Исходная 3D-модель шашки (Blender)
│   │   ├── Corner.blend                            # Исходная 3D-модель угла (Blender)
│   │   └── Side.blend                              # Исходная 3D-модель бортика (Blender)
│   ├── Scenes/                                     # Игровые сцены
│   │   ├── GameScene.unity                         # Сцена с игровым процессом
│   │   └── MenuScene.unity                         # Сцена главного меню
│   ├── Scripts/                                    # Модули
│   │   ├── Core/                                   # Алгоритмическое ядро (независимая логика)
│   │   │   ├── DataTypes/                          # Базовые типы данных для ядра
│   │   │   │   ├── CheckerMove.cs                  # Структура хода шашки
│   │   │   │   ├── CheckerType.cs                  # Тип шашки (обычный, дамка)
│   │   │   │   ├── EndOfGameType.cs                # Тип окончания игры (победа, поражение, ничья, ничего)
│   │   │   │   └── OpponentType.cs                 # Тип противника (игрок/ИИ)
│   │   │   ├── BoardPosition.cs                    # Доска с шашками
│   │   │   ├── CheckerData.cs                      # Данные о шашке
│   │   │   ├── Core.asmdef                         # Сборка (assembly) для ядра
│   │   │   └── MinimaxCore.cs                      # Реализация минимакса с альфа-бета отсечением
│   │   ├── GamePlay/                               # Логика игрового процесса
│   │   │   ├── GameSceneScripts/                   # Скрипты для игровой сцены
│   │   │   │   ├── DataTypes/                      # Типы данных для игровой сцены
│   │   │   │   │   ├── BoardCell.cs 	            # Данные о клетке доски
│   │   │   │   │   └── MovesHistory.cs             # История ходов
│   │   │   │   ├── BoardEntities.cs 	            # Класс, через который происходит взаимодействие с изначальной доской. Физическое передвичение шашек
│   │   │   │   ├── CameraController.cs             # Управление камерой
│   │   │   │   ├── Checker.cs                      # Игровой, физический объект шашки
│   │   │   │   ├── CheckersAI.cs                   # ИИ бот для игры в шашки (использует MinimaxCore)
│   │   │   │   ├── CheckersBoard.cs                # Создание физической доски и её логика
│   │   │   │   ├── Game.cs                         # Класс содержащий игровые флаги
│   │   │   │   ├── InputController.cs              # Обработка ввода игрока с помощью мыши и клавиатуры
│   │   │   │   ├── MovesHistoryTab.cs              # Вкладка журнала партии
│   │   │   │   ├── PauseTab.cs                     # Вкладка паузы
│   │   │   │   ├── PositionAssessmentTab.cs        # Вкладка оценки позиции
│   │   │   │   └── UIManager.cs                    # Управление UI на игровой сцене
│   │   │   ├── MenuSceneScripts/                   # Скрипты для меню
│   │   │   │   ├── MenuTab.cs                      # Главная вкладка меню
│   │   │   │   ├── SettingsTab.cs                  # Вкладка настроек
│   │   │   │   └── StatisticTab.cs                 # Вкладка статистики
│   │   │   ├── GamePlay.asmdef                     # Сборка для игрового процесса
│   │   │   ├── GameSettings.cs                     # Настройки игры (сохраняются в JSON)
│   │   │   ├── GameStatistic.cs                    # Статистика игр (сохраняется в JSON)
│   │   │   └── SceneNames.cs                       # Константы имён сцен
│   │   └── Core.meta                               # Мета-файл для папки Core
│   ├── Settings/                                   # Настройки проекта Unity
│   ├── Sprites/                                    # 2D-спрайты
│   │   └── PositionAssessmentForegroundSprite.png 	# Фоновый спрайт для вкладки оценки позиции
│   ├── Tests/                       			    # Тесты
│   │   ├── EditMode/                			    # Юнит-тесты (Edit Mode)
│   │   │   ├── Core/                			    # Тесты алгоритмического ядра
│   │   │   │   ├── BoardPositionTests.cs    	    # Тесты игровой доски - получение общий сведений о текущей игровой позиции, также совершение ходов
│   │   │   │   ├── CheckerDataTests.cs      	    # Тесты шашек - получение всех доступных ходов для шашки, кеширование, клонирование, изменение свойств
│   │   │   │   ├── EdgeCaseTests.cs         	    # Тесты граничных случаев
│   │   │   │   ├── MinimaxCoreGiveawayTests.cs     # Тесты минимакса (режим поддавки)
│   │   │   │   └── MinimaxCoreTests.cs      	    # Основные тесты минимакса
│   │   │   ├── EditMode.asmdef                     # Сборка для EditMode тестов
│   │   │   └── EditMode.meta                       # Мета-файл для папки EditMode
│   │   └── PlayMode/                               # Интеграционные тесты (PlayMode)
│   │       ├── IntegrationTests.cs                 # Интеграционные тесты (ИИ против ИИ)
│   │       ├── PlayMode.asmdef                     # Сборка для PlayMode тестов
│   │       └── PlayMode.meta                       # Мета-файл для папки PlayMode
│   ├── TextMesh Pro/                               # Ресурсы TextMesh Pro (текстовый UI)
│   └── InputSystem_Actions.inputactions            # Настройки Input System
├── Packages/                                       # Управление пакетами Unity (зависимости)
├── ProjectSettings/                                # Настройки всего проекта Unity
├── Dockerfile
├── .gitattributes                   
├── .gitignore                       
├── .vsconfig                                       # Настройки для Visual Studio
├── Checkers.slnx                                   # Файл решения для Visual Studio
└── README.md
```
              
## Установка и запуск
```bash
# 1 Локально
#   1.1 через редактор Unity
#   1.2 через сборку WebGL
#
# 2 В Docker
```

### Локально

#### через редактор Unity

```bash
# 1. Добавить проект из репозитория в Unity Hub
Запустите Unity Hub.
Нажмите кнопку "Add" (добавить проект).
Выберите пункт "Add project from repository" (добавить проект из репозитория).

В появившемся окне выберите "GitHub" в качестве источника.
Авторизуйтесь в GitHub, если потребуется.
вставьте ссылку: https://github.com/Sergey-Sergeev/Checkers.git в поле Repository.
в поле "Location" выберите место куда будет клонирован репозиторий.

Нажмите "Add Project" — Unity Hub автоматически клонирует репозиторий и добавит проект в список.

Unity определит версию проекта (6000.4.10f1) и предложит установить её, если она отсутствует.

# 2. Открыть проект в Unity
В списке проектов Unity Hub нажмите на добавленный проект Checkers.
Дождитесь загрузки и импорта всех ресурсов.

# 3. Запустить игру
# 3.1. В редакторе 
- В окне Project откройте папку Assets/Scenes/.
- Дважды кликните по сцене MenuScene.unity, чтобы открыть её.
- Нажмите кнопку Play (треугольная кнопка) в верхней части редактора.

# 3.2. Полноценно собрать и запустить проект
- Выберите File - Build and Run
- После того как проект соберётся, он автоматически откроет игру в браузере

# 4. Управление в игре
В главном меню выберите нужный пункт с помощью мышки.
В игровой сцене для хода нажмите на свою шашку, затем нажмите на одну из подсвеченных клеток, куда хотите сходить.
ИИ сделает ход автоматически после вашего хода.
```

#### через сборку WebGL

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Sergey-Sergeev/Checkers.git
cd Checkers

# 2. Собрать проект под WebGL
<путь_к_Unity.exe> -projectPath . -batchmode -quit -buildWebGLPlayer ./Build/WebGL

- Где <путь_к_Unity.exe> — например, 
для Windows: "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe".
для macOS: "/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity"
для Linux: "/opt/unity/Editor/Unity"

# 3. Запустить игру в браузере
Перейдите в папку со сборкой и запустите локальный веб-сервер:
cd Build/WebGL

-- Вариант A (Python 3):
python -m http.server 8080

-- Вариант B (Node.js с http-server):
npx http-server -p 8080

-- Вариант C (PHP):
php -S localhost:8080

# 4. Открыть в браузере
Перейдите по адресу:
http://localhost:8080

- Примечания:
Сборка может занять несколько минут (от 10 до 20 минут) в зависимости от мощности вашего компьютера.
Убедитесь, что у вас установлена лицензия Unity (Personal бесплатна).
```

### В Docker

```bash
# 1. Собрать образ
docker build -t checkers-ai .

# 2. Запустить контейнер
docker run -d -p 8080:80 --name checkers-app checkers-ai

# 3. Открыть в браузере
Перейдите по адресу:
http://localhost:8080

# 4. Остановить и удалить контейнер (при необходимости)
docker stop checkers-app
docker rm checkers-app

Сохранение данных между запусками
- Чтобы настройки (settings.json) и статистика (statistics.json) не терялись при перезапуске контейнера, смонтируйте том:
docker run -d -p 8080:80 -v "$(pwd)/data:/usr/share/nginx/html/data" --name checkers-app checkers-ai

Примечания:
Внутри контейнера Unity собирает проект в WebGL автоматически — это может занять 10–20 минут при первой сборке.
Убедитесь, что Docker установлен и запущен на вашем компьютере.
Если порт 8080 занят, замените его на другой, например: -p 8081:80.
Все данные (настройки, статистика) сохраняются в папке data/ на вашем компьютере.
```

## Запуск тестов

В проекте 67 юнит-тестов (EditMode) и 2 интеграционных теста (PlayMode), проверяющих работу алгоритмического ядра и логику игры.

```bash
# 1 через редактор Unity
# 2 из командной строки (Headless режим)
# 3 В Docker
```

### через редактор Unity

```bash
# 1 Откройте окно Test Runner: в верхнем меню Unity выберите Window > General > Test Runner.
# 2 В открывшемся окне вы увидите две вкладки: EditMode (67 юнит-тестов) и PlayMode (2 интеграционных теста).
# 3 Выберите нужную вкладку и нажмите кнопку "Run All", чтобы выполнить все тесты в этой группе. Результаты появятся прямо в окне Test Runner.
```

### из командной строки (Headless режим)

```bash
# Запуск всех EditMode тестов:
<путь_к_Unity.exe> -projectPath . -runTests -batchmode -testPlatform EditMode

# Запуск всех PlayMode тестов:
<путь_к_Unity.exe> -projectPath . -runTests -batchmode -testPlatform PlayMode
```

### В Docker
```bash
# Запуск всех EditMode тестов:
docker run --rm checkers-ai -runTests -batchmode -testPlatform EditMode

# Запуск всех PlayMode тестов:
docker run --rm checkers-ai -runTests -batchmode -testPlatform PlayMode
```

## Зависимости

 - Unity ≥ 6000.4.10f1 (рекомендуется версия, указанная в ProjectSettings/ProjectVersion.txt)
 - .NET: API Compatibility Level - .NET Standard 2.1
 - Язык: C#
 - Платформа сборки: WebGL (для Docker) / Windows, macOS, Linux (для редактора)
 - Docker ≥ 20.10 (для запуска в контейнере)
 - Внешние библиотеки: Не используются (только стандартные пакеты Unity)
