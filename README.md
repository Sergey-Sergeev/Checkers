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
├── Assets/              # Все игровые ресурсы и исходный код Unity
│   ├── Materials/       # Материалы для визуального оформления игровых объектов
│   │   ├── Cell_1.mat             # Материал для светлых клеток доски
│   │   ├── Cell_2.mat             # Материал для тёмных клеток доски
│   │   ├── Checker.mat            # Материал для шашек
│   │   ├── SelectedCell.mat       # Материал для подсветки клетки, возможных ходов
│   │   └── Side.mat               # Материал для бортиков доски
│   ├── Models/                    # 3D-модели и префабы игровых объектов
│   │   ├── Prefabs/               # Готовые сборные объекты (префабы)
│   │   │   ├── Cell.prefab            # Префаб клетки доски
│   │   │   ├── Checker.prefab         # Префаб шашки
│   │   │   ├── Corner.prefab          # Префаб углового элемента доски
│   │   │   ├── HistoryRow.prefab      # Префаб строки для журнала партии
│   │   │   └── Side.prefab            # Префаб бокового элемента доски
│   │   ├── Cell.blend                   # Исходная 3D-модель клетки (Blender)
│   │   ├── Checker.blend                # Исходная 3D-модель шашки (Blender)
│   │   ├── Corner.blend                 # Исходная 3D-модель угла (Blender)
│   │   └── Side.blend                   # Исходная 3D-модель бортика (Blender)   
│   ├── Scenes/                          # Игровые сцены
│   │   ├── GameScene.unity                # Сцена с игровым процессом
│   │   └── MenuScene.unity                # Сцена главного меню  
│   ├── Editor/                               
│   │   └── BuildScript.cs                      # специальный скрипт для сборки проекта, под WebGL
│   ├── Scripts/                                # Модули
│   │   ├── Core/                      # Слой алгоритмического ядро (независимая логика)
│   │   │   ├── DataTypes/                      # Базовые типы данных для ядра
│   │   │   │   ├── CheckerMove.cs              # Структура хода шашки
│   │   │   │   ├── CheckerType.cs              # Тип шашки (обычный, дамка)
│   │   │   │   └── OpponentType.cs             # Тип противника (игрок/ИИ)
│   │   │   ├── BoardPosition.cs                # Доска с шашками
│   │   │   ├── CheckerData.cs                  # Данные о шашке
│   │   │   ├── Core.asmdef                     # Сборка (assembly) для ядра
│   │   │   └── MinimaxCore.cs                  # Реализация минимакса с альфа-бета отсечением
│   │   ├── GameLogic/                      # Слой игровой логики
│   │   │   ├── BoardCell.cs                     # Игровой, физический объект клетки доски
│   │   │   ├── BoardManager.cs                  # Управление доской и шашками
│   │   │   ├── CameraController.cs              # Управление камерой
│   │   │   ├── Checker.cs                       # Игровой, физический объект шашки
│   │   │   ├── CheckersAI.cs                    # ИИ бот для игры в шашки (использует MinimaxCore)
│   │   │   ├── CheckersBoard.cs                 # Создание физической доски и её логика
│   │   │   ├── GameLogic.asmdef            # Сборка для слоя игровой логики
│   │   │   ├── GameManager.cs                   # класс, через который происходит взаимодействие со слоем представления
│   │   │   ├── InputController.cs               # Обработка ввода игрока (мышь и клавиатура)
│   │   │   └── MovesHistory.cs                  # История ходов
│   │   ├── Infrastructure/                    # Слой инфраструктуры (вспомогательный)
│   │   │   ├── EndOfGameType.cs                  # Тип окончания игры (победа, поражение, ничья, ничего)
│   │   │   ├── FileStorage.cs                      # Абстрактный класс для работы с файлами 
│   │   │   ├── GameSettings.cs                     # Настройки игры (сохраняются в JSON)
│   │   │   ├── GameStatistics.cs                   # Статистика игр (сохраняется в JSON)
│   │   │   ├── Infrastructure.asmdef          # Сборка для слоя инфраструктуры
│   │   │   ├── PlatformService.cs                  # Абстракция над классом Application
│   │   │   └── SceneNames.cs                       # Константы имён сцен
│   │   └── Presentation/                      # Слой игровой представления
│   │       ├── GameScene/                   # Скрипты для игровой сцены
│   │       │   ├── GameTabManager.cs                # класс, через который происходит взаимодействие со всем слоем
│   │       │   ├── MovesHistoryTab.cs               # Вкладка журнала партии
│   │       │   ├── PauseTab.cs                      # Вкладка паузы
│   │       │   └── PositionAssessmentTab.cs         # Вкладка оценки позиции
│   │       ├── MenuScene/                   # Скрипты для меню
│   │       │   ├── MenuTab.cs                      # Главная вкладка меню
│   │       │   ├── SettingsTab.cs                  # Вкладка настроек
│   │       │   └── StatisticsTab.cs                # Вкладка статистики
│   │       └── Presentation.asmdef            # Сборка для слоя представления
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
│   │   │   └── EditMode.asmdef                     # Сборка для EditMode тестов
│   │   │ 
│   │   └── PlayMode/                               # Интеграционные тесты (PlayMode)
│   │       ├── IntegrationTests.cs                 # Интеграционные тесты (ИИ против ИИ)
│   │       └── PlayMode.asmdef                     # Сборка для PlayMode тестов
│   ├── TextMesh Pro/                               # Ресурсы TextMesh Pro (текстовый UI)
│   └── InputSystem_Actions.inputactions            # Настройки Input System
├── Build/                                          # готовый, собранный проект 
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
#   1.1 через готовую сборку 
#   1.2 через редактор Unity
#   1.3 через сборку WebGL
#
# 2 В Docker
```

### Локально

#### через готовую сборку 

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Sergey-Sergeev/Checkers.git

# 2. Перейдите в папку с собранной игрой
cd Checkers/Build

# 3. Запустить игру в браузере
python -m http.server 8080

# 4. Открыть в браузере
Перейдите по адресу:
http://localhost:8080
```


#### через редактор Unity

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Sergey-Sergeev/Checkers.git

# 2. Добавить проект из репозитория в Unity Hub
Запустите Unity Hub.
Нажмите кнопку "Add".
Выберите пункт "Add project from disk"
Unity определит версию проекта (6000.4.10f1) и предложит установить её, если она отсутствует.

# 3. Открыть проект в Unity
В списке проектов Unity Hub нажмите на добавленный проект Checkers.
Дождитесь загрузки и импорта всех ресурсов.

# 3. Запустить игру
# 3.1. В редакторе 
- В окне Project откройте папку Assets/Scenes/.
- Дважды кликните по сцене MenuScene.unity, чтобы открыть её.
- Нажмите кнопку Play (треугольная кнопка) в верхней части редактора.

# 3.2. Полноценно собрать и запустить проект
- Перейдите File - Build Profiles, выберите из списка платформ Web, затем нажмите на кнопку Switch Platform
- Выберите File - Build and Run
- После того как проект соберётся, он автоматически откроет игру в браузере
```

#### через сборку WebGL

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Sergey-Sergeev/Checkers.git
cd Checkers

# 2. Собрать проект под WebGL
<путь_к_Unity.exe> -projectPath . -batchmode -quit -logFile .\build.log -executeMethod BuildScript.Build

- Где:
<путь_к_Unity.exe>
например может быть: 
--для Windows: "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe".
--для macOS: "/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity"
--для Linux: "/opt/unity/Editor/Unity"

-projectPath
путь к папке проекта

-batchmode
запуск в режиме без отображения окна редактора

-quit
сразу выйти после выполнения 

-logFile
путь куда будет сохранён лог файл

-executeMethod BuildScript.Build
вызывает специально подготовленный сборочный скрипт

# 3. Запустить игру в браузере
Перейдите в папку со сборкой и запустите локальный веб-сервер:
cd Build/WebGL/index.html/

Запускаем через Python
python -m http.server 8080

# 4. Открыть в браузере
Перейдите по адресу:
http://localhost:8080

- Примечания:
Рекомендуется очистить папку Build перед сборкой.
Если при запуске игры в игровой сцене не отображается шашечная доска, то просто попробуйте собрать проект ещё один раз.
Сборка может занять несколько минут (от 15 до 25 минут) в зависимости от мощности вашего компьютера.
Сборка закончиться, когда в папке index.html появится файл index.html.
Убедитесь, что у вас установлена лицензия Unity (Personal бесплатна).
```

```bash
---- Примечание для двух последних случаев:
Убедитесь что у вас установлен модуль поддержки WebGL. Он устанавливается через Unity Hub:
1 Через Unity Hub:
# 1 Открываете Unity Hub
# 2 В открывшемся окне переходите слева во вкладку Installs
# 3 Находите в списке свою версию
# 4 нажимаете на настроки редактора, они выглядят как шестерёнка
# 5 нажимаете Add modules
# 6 Во вкладке Platforms находите модуль Web Build Support
# 7 выбираете его из списка, нажимете на кнопку Continue, и Unity установит модуль

2 Через комманду:
# 1 Откройте консоль и вставьте эту команду:
<путь_к_Unity_Hub.exe> -- --headless install-modules --version <ваша_версия> -m webgl

Где:
<ваша_версия>
например целевая для этого проекта 6000.4.10f1
```


### В Docker

```bash
# 1. Собрать образ
docker build -t checkers-ai .

# 2. Запустить контейнер
docker run -d -p 8080:8080 --name checkers-app checkers-ai

# 3. Открыть в браузере
Перейдите по адресу:
http://localhost:8080

# 4. Остановить и удалить контейнер (при необходимости)
docker stop checkers-app
docker rm checkers-app

Примечания:
Убедитесь, что Docker установлен и запущен на вашем компьютере.
Если вы до этого собрали проект сами, то файлы, лежащие в папке Checkers/Build/WebGL/index.html, переместите в папку Checkers/Build.
Если порт 8080 занят, замените его на другой, например: -p 8081:8081.
```

## Запуск тестов

В проекте 61 юнит-тест (EditMode) и 2 интеграционных теста (PlayMode), проверяющих работу алгоритмического ядра и логику игры.

```bash
# 1 через редактор Unity
# 2 из командной строки (Headless режим)
```

### через редактор Unity

```bash
# 1 Откройте окно Test Runner: в верхнем меню Unity выберите Window > General > Test Runner.
# 2 В открывшемся окне вы увидите две вкладки: EditMode (61 юнит-тест) и PlayMode (2 интеграционных теста).
# 3 Выберите нужную вкладку и нажмите кнопку "Run All", чтобы выполнить все тесты в этой группе. Результаты появятся прямо в окне Test Runner.
```

### из командной строки (Headless режим)

```bash
# Запуск всех EditMode тестов:
<путь_к_Unity.exe> -projectPath . -runTests -batchmode -testPlatform EditMode -testResults ./test-results.xml

# Запуск всех PlayMode тестов:
<путь_к_Unity.exe> -projectPath . -runTests -batchmode -testPlatform PlayMode -testResults ./test-results.xml

- Где:
<путь_к_Unity.exe>
например может быть: 
--для Windows: "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe".
--для macOS: "/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity"
--для Linux: "/opt/unity/Editor/Unity"

-projectPath
путь к папке проекта

-runTests
флаг для запуски тестов

-batchmode
запуск в режиме без отображения окна редактора

-testPlatform
платформа на которой будут выполняться тесты

-testResults
путь куда будет сохранены результаты тестов


----После завершения тестов появится файл test-results.xml, в котором и будут результаты всех тестов
```

## Зависимости

### Для запуска

 - Docker ≥ 20.10 (для запуска в контейнере)
 - Для локального запуска: Python 3.x

### Для сборки

 - Unity ≥ 6000.4.10f1 (рекомендуется версия, указанная в ProjectSettings/ProjectVersion.txt)
 - .NET: API Compatibility Level - .NET Standard 2.1
 - Модуль поддержки WebGL (можно установить через Unity Hub ≥ 3.18.0)
