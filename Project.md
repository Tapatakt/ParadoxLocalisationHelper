# ParadoxLocalisationHelper

## Описание

Инструмент для работы с файлами локализации модов к играм Paradox Interactive.

## Цель

Упростить процесс обновления переводов модов при выходе новых версий:
- Отслеживать изменения в оригинальных текстах между версиями
- Находить недостающие переводы
- Объединять устаревшие переводы с новыми обновлениями

## Технологии

- C# / .NET 10

## Статус

✅ **Реализовано** — интерактивное консольное меню со всеми функциями.

---

## Формат файлов локализации

Файлы имеют расширение `.yml` и YAML-подобную структуру:

```yaml
l_english:
 # Комментарий
 key_name:0 "Localized text with §Gcolor codes§! and $references$"
 key_name_desc:0 "Description with \n newlines"
 another_key: "Simple value without version suffix"
 empty_key: ""
```

### Особенности формата

1. **Языковой тег** — первая строка файла (`l_english:`, `l_russian:` и т.д.)
2. **Версия ключа** — необязательный числовой суффикс `:0`, `:1` и т.д.
3. **Значение** — строка в двойных кавычках (может быть пустой)
4. **Комментарии** — строки, начинающиеся с `#`
5. **Пустые строки** — игнорируются
6. **Цветовые коды** — `§R`, `§G`, `§B` и т.д. с закрывающим `§!`
7. **Ссылки на ключи** — `$key_name$`
8. **Escape-последовательности** — `\n` для переноса строки
9. **Переменные скрипта** — `[root.owner.variable]`
10. **BOM** — файлы начинаются с Byte Order Mark (UTF-8)

---

## Использование

Запустите программу без аргументов:

```bash
dotnet run
# или
ParadoxLocalisationHelper.exe
```

Откроется интерактивное меню:

```
=== Paradox Localisation Helper ===
      Помощник локализации Paradox

Текущие настройки:
  [1] Новая версия (оригинал):     (не задан)
  [2] Старая версия (оригинал):    (не задан)
  [3] Устаревший перевод:          (не задан)
  [4] Обновление перевода:         (не задан)
  [5] Язык оригинала:              l_english
  [6] Язык перевода:               l_russian

Действия:
  [7] Сгенерировать отчёт
       (анализ изменений и прогресса перевода, с YML для оставшейся работы)

 [10] Объединить переводы
       (слияние устаревшего перевода с обновлением)

  [0] Выход
```

### Сохранение настроек

Программа автоматически сохраняет все настройки (пути и языки) в файл `config.json` при их изменении. При следующем запуске настройки будут загружены автоматически.

### Настройка путей

1. **Новая версия (оригинал)** — папка с новой версией мода (оригинальный язык)
2. **Старая версия (оригинал)** — папка со старой версией мода (оригинальный язык)
3. **Устаревший перевод** — папка с устаревшим переводом
4. **Обновление перевода** — папка с обновлением перевода (частичным, только новые/изменённые ключи)

### Генерация отчёта (пункт 7)

Единый отчёт, анализирующий все доступные данные:

**Изменения между версиями:**
- Ключи, добавленные в новой версии
- Ключи, удалённые из старой версии
- Ключи, изменённые между версиями

**Работа по переводу:**
- Новые ключи (добавленные в новой версии)
- Изменённые ключи (значения изменились)
- Ключи, недостающие в устаревшем переводе
- **Work to do** — всё, что нужно перевести
- **Work done** — что уже есть в обновлении перевода
- **Work remaining** — что осталось сделать

**Проверка качества:**
- **Duplicate Keys** — дубликаты в обновлении и устаревшем переводе
- **Unnecessary Keys** — ключи в обновлении, которых нет в новой версии

**Выходные данные:**
- Markdown-отчёт с одной большой таблицей по всем файлам
- Пофайловая статистика (ключи/символы) для каждой категории
- Строка **Total** с суммами внизу таблицы
- Папка с YML-файлами, содержащими оставшуюся работу (если есть)

### Слияние переводов (пункт 10)

Объединяет устаревший перевод с обновлением:
- Значения из обновления замещают старые
- Структура файлов берётся из новой версии
- Результат сохраняется в указанную папку

---

## Типичный workflow

### Сценарий 1: Анализ изменений

```
1. Установите путь к новой версии (пункт 1)
2. Установите путь к старой версии (пункт 2)
3. Сгенерируйте отчёт (пункт 7)
→ Увидите все изменения между версиями
```

### Сценарий 2: Проверка прогресса перевода

```
1. Установите путь к новой версии (пункт 1)
2. Установите путь к устаревшему переводу (пункт 3)
3. Установите путь к обновлению перевода (пункт 4)
4. Сгенерируйте отчёт (пункт 7)
→ Увидите что сделано и что осталось
```

### Сценарий 3: Полный анализ

```
1. Установите все 4 пути (пункты 1-4)
2. Сгенерируйте отчёт (пункт 7)
→ Полная картина: изменения версий, прогресс перевода, дубликаты, лишние ключи
3. Получите YML с оставшейся работой
4. Выполните слияние (пункт 10) когда перевод готов
```

---

## Архитектура программы

### Модель данных

```
AppState
├── NewVersionPath: string
├── OldVersionPath: string
├── OldTranslationPath: string
├── NewTranslationPath: string
├── SourceLanguage: string
└── TargetLanguage: string

LocalizationFile
├── Language: string
├── FilePath: string
├── FileName: string
└── Entries: List<LocalizationEntry>

LocalizationEntry
├── Key: string
├── Version: int?
├── Value: string
├── RawValue: string
└── LineNumber: int
```

### LocalizationStorage — центральный компонент

Хранилище загруженных локализаций с операциями над множествами:

**Свойства:**
- `Name: string` — имя хранилища (для отображения)
- `KeyCount: int` — количество ключей
- `FileCount: int` — количество файлов
- `IsEmpty: bool` — пустое ли хранилище
- `Entries: IReadOnlyDictionary<string, LocalizationEntry>`
- `Files: IReadOnlyDictionary<string, LocalizationFile>`

**Методы загрузки:**
- `AddFile(LocalizationFile file)`
- `AddFiles(IEnumerable<LocalizationFile> files)`

**Операции над множествами:**
- `Except(LocalizationStorage other)` — ключи, которые есть в this, но нет в other
- `Modified(LocalizationStorage other)` — ключи с разными значениями в обоих
- `Unchanged(LocalizationStorage other)` — ключи с одинаковыми значениями
- `Union(LocalizationStorage other)` — объединение (this приоритетнее)
- `Intersect(LocalizationStorage other)` — пересечение

**Утилиты:**
- `SetName(string name)` — установить имя (fluent API)
- `Describe()` — вывести в консоль и вернуть this
- `ContainsKey(string key)` — проверка наличия ключа
- `TryGetEntry(string key, out LocalizationEntry? entry)` — получить запись
- `GetAllKeys()` — все ключи
- `GetSourceFile(string key)` — файл, содержащий ключ
- `GetFileStats(string filePath)` — статистика по файлу (ключи, символы)

**Пример цепочки операций:**
```csharp
LocalizationStorage workToDo = addedKeys
    .Union(modifiedKeys)
    .Union(missingInOldTranslation)
    .SetName("Work To Do")
    .Describe();
```

### ReportGenerator

Генератор отчётов с анализом данных.

**Входные данные (инициализация):**
```csharp
ReportGenerator generator = new()
{
    OldVersion = oldVersion,           // старая версия оригинала
    NewVersion = newVersion,           // новая версия оригинала
    OldTranslation = oldTranslation,   // устаревший перевод
    TranslationUpdate = translationUpdate // обновление перевода
};
```

**Вычисляемые данные (после Analyze):**
- `AddedKeys` — ключи, добавленные в новой версии
- `RemovedKeys` — ключи, удалённые из старой версии
- `ModifiedKeys` — ключи с изменёнными значениями
- `MissingInOldTranslation` — ключи, недостающие в старом переводе
- `WorkToDo` — всё, что нужно перевести
- `WorkDone` — что уже сделано в обновлении
- `WorkRemaining` — что осталось сделать
- `UnnecessaryKeys` — ключи в обновлении, которых нет в новой версии

**Методы:**
- `Generate()` — выполняет анализ, выводит сводку, генерирует отчёт

### Другие компоненты

#### Configuration (`ConfigurationService`)
Управление сохранением и загрузкой настроек.

**Методы:**
- `AppState Load()` — загрузка настроек из JSON
- `void Save(AppState state)` — сохранение настроек в JSON

#### Parser (`ParadoxLocalizationParser`)
Чтение и разбор `.yml` файлов.

**Методы:**
- `LocalizationFile ParseFile(string filePath)`
- `List<LocalizationFile> ParseDirectory(string directoryPath)`

#### Analysis (`MissingKeysAnalyzer`)
Анализ orphaned и duplicate ключей.

**Методы:**
- `TranslationAnalysisResult AnalyzeTranslation(LocalizationStorage source, LocalizationStorage translation)`

#### YML Writer (`LocalizationYmlWriter`)
Запись локализаций в YML-файлы.

**Методы:**
- `List<WrittenFileInfo> WriteStorage(LocalizationStorage storage, string outputDirectory, string language)` — запись хранилища по файлам
- `void WriteFiles(IEnumerable<LocalizationFile> files, string outputDirectory)` — запись объединённых файлов

#### Merging (`LocalizationMerger`)
Слияние устаревшего перевода с обновлением.

**Метод:**
- `List<LocalizationFile> Merge(LocalizationStorage oldTranslation, LocalizationStorage newTranslation, LocalizationStorage newVersion, string targetLanguage)`

#### CLI (`ConsoleMenu`, `MenuItem`, `ConsoleMenuService`)
Интерактивное консольное меню.

---

## Структура проекта

```
src/
├── Models/
│   ├── AppState.cs              # Состояние приложения
│   ├── LocalizationEntry.cs     # Запись локализации
│   └── LocalizationFile.cs      # Файл локализации
├── Configuration/
│   └── ConfigurationService.cs  # Управление настройками
├── Cli/
│   ├── ConsoleMenu.cs           # Меню с пунктами
│   ├── MenuItem.cs              # Пункт меню
│   ├── MenuActions.cs           # Действия пунктов меню
│   └── ConsoleMenuService.cs    # Утилиты консоли
├── Parsing/
│   └── ParadoxLocalizationParser.cs
├── Storage/
│   └── LocalizationStorage.cs   # Хранилище с операциями над множествами
├── Analysis/
│   ├── Models/
│   │   ├── DuplicateKeyInfo.cs
│   │   ├── MissingKeyInfo.cs
│   │   ├── MissingKeysResult.cs
│   │   ├── OrphanedKeyInfo.cs
│   │   └── TranslationAnalysisResult.cs
│   └── MissingKeysAnalyzer.cs
├── Reporting/
│   ├── MarkdownTableBuilder.cs  # Построитель markdown-таблиц
│   └── ReportGenerator.cs       # Генератор отчётов
├── Yml/
│   ├── LocalizationYmlWriter.cs # Запись YML-файлов
│   └── WrittenFileInfo.cs       # Информация о созданном файле
└── Merging/
    └── LocalizationMerger.cs    # Слияние переводов
```

---

## Пример использования (API)

```csharp
using ParadoxLocalisationHelper.Configuration;
using ParadoxLocalisationHelper.Parsing;
using ParadoxLocalisationHelper.Storage;
using ParadoxLocalisationHelper.Reporting;
using ParadoxLocalisationHelper.Yml;
using ParadoxLocalisationHelper.Merging;

// Загрузка настроек
ConfigurationService configService = new("config.json");
AppState state = configService.Load();

// Загрузка данных
ParadoxLocalizationParser parser = new();
LocalizationStorage oldVersion = new();
oldVersion.AddFiles(parser.ParseDirectory(@"mod\v1.0"));
LocalizationStorage newVersion = new();
newVersion.AddFiles(parser.ParseDirectory(@"mod\v2.0"));
LocalizationStorage oldTranslation = new();
oldTranslation.AddFiles(parser.ParseDirectory(@"translation\old"));
LocalizationStorage translationUpdate = new();
translationUpdate.AddFiles(parser.ParseDirectory(@"translation\update"));

// Анализ и отчёт
ReportGenerator generator = new()
{
    OldVersion = oldVersion,
    NewVersion = newVersion,
    OldTranslation = oldTranslation,
    TranslationUpdate = translationUpdate
};
string reportPath = generator.Generate();

// Генерация YML с оставшейся работой
if (!generator.WorkRemaining.IsEmpty)
{
    LocalizationYmlWriter ymlWriter = new();
    ymlWriter.WriteStorage(generator.WorkRemaining, @"output\todo", "english");
}

// Слияние переводов
LocalizationMerger merger = new();
List<LocalizationFile> merged = merger.Merge(
    oldTranslation, translationUpdate, newVersion, "russian");

// Сохранение результата
LocalizationYmlWriter writer = new();
writer.WriteFiles(merged, @"translation\merged");

// Сохранение настроек
configService.Save(state);
```
