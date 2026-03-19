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

Current Settings:
  [1] New version folder:     (not set)
  [2] Old version folder:     (not set)
  [3] Outdated translation:   (not set)
  [4] Translation update:     (not set)
  [5] Source language:        english
  [6] Target language:        russian

Actions:
  [7] Generate delta report (old vs new version) + YML files
  [8] Generate missing keys report (outdated translation vs new version) + YML files
  [9] Merge outdated translation with update
 [10] Generate translation work report (missing + modified keys) + YML files

  [0] Exit
```

### Сохранение настроек

Программа автоматически сохраняет все настройки (пути и языки) в файл `config.json` при их изменении. При следующем запуске настройки будут загружены автоматически.

### Настройка путей

1. **New version folder** — папка с новой версией мода (оригинальный язык)
2. **Old version folder** — папка со старой версией мода (оригинальный язык)
3. **Outdated translation** — папка с устаревшим переводом
4. **Translation update** — папка с обновлением перевода (частичным)

### Генерация отчёта по дельте (пункт 7)

Сравнивает старую и новую версии на оригинальном языке. Создаёт:
- Markdown-отчёт со статистикой по ключам и символам
- Папку с YML-файлами, содержащими недостающие и изменённые ключи (структура файлов как в новой версии)

Отчёт включает:
- Количество добавленных/удалённых/изменённых ключей
- Количество символов в добавленных/удалённых/изменённых значениях
- Список созданных YML-файлов с указанием количества ключей и символов в каждом

### Генерация отчёта по недостаче (пункт 8)

Сравнивает устаревший перевод с новой версией. Показывает:
- Статистику по ключам (всего/переведено/недостаёт)
- Статистику по символам (всего/переведено/недостаёт)
- Процент готовности по символам
- Папку с YML-файлами, содержащими недостающие ключи (структура файлов как в новой версии)

### Отчёт по работе переводчика (пункт 10)

Комплексный отчёт, объединяющий:
- **Новые ключи** — добавлены в новой версии (не было в старой)
- **Изменённые ключи** — значения изменились между старой и новой версией
- **Недостающие ключи** — есть в новой версии, но отсутствуют в устаревшем переводе

Создаёт:
- Markdown-отчёт с полной сводкой по всем категориям
- Папку с YML-файлами, содержащими все ключи, требующие работы

Требует установки всех трёх путей: старая версия, новая версия, устаревший перевод.

### Слияние переводов (пункт 9)

Объединяет устаревший перевод с обновлением:
- Значения из обновления замещают старые
- Структура файлов берётся из новой версии
- Результат сохраняется в указанную папку

---

## Типичный workflow

### Сценарий 1: Обновление мода с новой версией

```
1. Установите путь к новой версии (пункт 1)
2. Установите путь к старой версии (пункт 2)
3. Сгенерируйте дельту (пункт 7)
4. Переведите ключи из YML-файлов в папке delta_yml_*
5. Установите путь к устаревшему переводу (пункт 3)
6. Установите путь к вашему переводу обновления (пункт 4)
7. Выполните слияние (пункт 9)
```

### Сценарий 2: Проверка завершённости перевода

```
1. Установите путь к новой версии (пункт 1)
2. Установите путь к текущему переводу (пункт 3)
3. Сгенерируйте отчёт по недостаче (пункт 8)
4. Проверьте процент готовности по символам
```

### Сценарий 3: Полный отчёт по работе переводчика

```
1. Установите путь к старой версии (пункт 2)
2. Установите путь к новой версии (пункт 1)
3. Установите путь к устаревшему переводу (пункт 3)
4. Сгенерируйте отчёт по работе (пункт 10)
5. Получите полный список: новые + изменённые + недостающие ключи
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
└── Entries: List<LocalizationEntry>

LocalizationEntry
├── Key: string
├── Version: int?
├── Value: string
├── RawValue: string
└── LineNumber: int

WrittenFileInfo
├── FilePath: string
├── FileName: string
├── KeyCount: int
└── CharacterCount: int
```

### Компоненты системы

#### 1. Configuration (`ConfigurationService`)
Управление сохранением и загрузкой настроек.

**Методы:**
- `AppState Load()` — загрузка настроек из JSON
- `void Save(AppState state)` — сохранение настроек в JSON

#### 2. Parser (`ParadoxLocalizationParser`)
Чтение и разбор `.yml` файлов.

**Методы:**
- `LocalizationFile ParseFile(string filePath)`
- `List<LocalizationFile> ParseDirectory(string directoryPath)`

#### 3. Storage (`LocalizationStorage`)
Хранилище загруженных локализаций.

**Методы:**
- `void AddFile(LocalizationFile file)`
- `void AddFiles(IEnumerable<LocalizationFile> files)`
- `LocalizationEntry? GetEntry(string key)`
- `bool ContainsKey(string key)`
- `ImmutableHashSet<string> GetAllKeys()`
- `string? GetSourceFile(string key)`

#### 4. Comparison (`LocalizationComparer`)
Сравнение двух наборов локализаций.

**Результат:**
```
ComparisonResult
├── Added: List<KeyWithSource>
├── Removed: List<KeyWithSource>
├── Modified: List<Modification>
└── Unchanged: List<string>
```

#### 5. Analysis (`MissingKeysAnalyzer`)
Анализ отсутствующих ключей перевода.

**Результат:**
```
MissingKeysResult
├── MissingKeys: List<MissingKeyInfo>
├── TotalSourceKeys: int
├── TranslatedKeys: int
├── TotalSourceCharacters: int
├── TranslatedCharacters: int
└── TranslationPercentageByCharacters: double
```

#### 6. Reporting (`ReportGenerator`)
Генерация отчётов в различных форматах (PlainText, Markdown, JSON).

**Методы:**
- `string GenerateComparisonReport(ComparisonResult result, ReportFormat format, List<WrittenFileInfo>? writtenFiles)`
- `string GenerateMissingKeysReport(MissingKeysResult result, ReportFormat format, List<WrittenFileInfo>? writtenFiles)`

#### 7. YML Writer (`LocalizationYmlWriter`)
Запись локализаций в YML-файлы.

**Методы:**
- `List<WrittenFileInfo> WriteDeltaYmlFiles(ComparisonResult result, string outputDirectory, string language, IEnumerable<LocalizationFile> newVersionFiles)` — запись дельты по файлам в папку
- `List<WrittenFileInfo> WriteMissingKeysYmlFiles(MissingKeysResult result, string outputDirectory, string language, IEnumerable<LocalizationFile> newVersionFiles)` — запись недостающих ключей по файлам в папку
- `void WriteFiles(IEnumerable<LocalizationFile> files, string outputDirectory)` — запись объединённых файлов

#### 8. Merging (`LocalizationMerger`)
Слияние устаревшего перевода с обновлением.

**Метод:**
- `List<LocalizationFile> Merge(LocalizationStorage oldTranslation, LocalizationStorage newTranslation, LocalizationStorage newVersion, string targetLanguage)`

#### 9. CLI (`ConsoleMenuService`)
Интерактивное консольное меню.

**Методы:**
- `int ShowMainMenu()`
- `string ReadFolderPath(string prompt)`
- `string ReadLanguage(string currentValue)`
- `string ReadOutputFolderName()`
- `void ShowSuccess(string message)`
- `void ShowError(string message)`

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
│   └── ConsoleMenuService.cs    # Интерактивное меню
├── Parsing/
│   └── ParadoxLocalizationParser.cs
├── Storage/
│   └── LocalizationStorage.cs
├── Comparison/
│   ├── Models/
│   │   ├── ComparisonResult.cs
│   │   ├── KeyWithSource.cs
│   │   └── Modification.cs
│   └── LocalizationComparer.cs
├── Analysis/
│   ├── Models/
│   │   ├── MissingKeyInfo.cs
│   │   └── MissingKeysResult.cs
│   └── MissingKeysAnalyzer.cs
├── Reporting/
│   ├── ReportFormat.cs
│   └── ReportGenerator.cs
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
using ParadoxLocalisationHelper.Comparison;
using ParadoxLocalisationHelper.Analysis;
using ParadoxLocalisationHelper.Yml;
using ParadoxLocalisationHelper.Merging;
using ParadoxLocalisationHelper.Reporting;

// Загрузка настроек
ConfigurationService configService = new("config.json");
AppState state = configService.Load();

// Загрузка версий
ParadoxLocalizationParser parser = new();
LocalizationStorage oldStorage = new();
oldStorage.AddFiles(parser.ParseDirectory(@"mod\v1.0"));
LocalizationStorage newStorage = new();
newStorage.AddFiles(parser.ParseDirectory(@"mod\v2.0"));
List<LocalizationFile> newFiles = parser.ParseDirectory(@"mod\v2.0");

// Сравнение версий
LocalizationComparer comparer = new();
ComparisonResult changes = comparer.Compare(oldStorage, newStorage);

// Запись дельты в папку с YML-файлами
LocalizationYmlWriter ymlWriter = new();
List<WrittenFileInfo> writtenFiles = ymlWriter.WriteDeltaYmlFiles(
    changes, @"output\delta", "english", newFiles);

// Генерация отчёта с информацией о файлах
string report = ReportGenerator.GenerateComparisonReport(
    changes, ReportFormat.Markdown, writtenFiles);
File.WriteAllText("report.md", report);

// Анализ недостающих переводов
LocalizationStorage translation = new();
translation.AddFiles(parser.ParseDirectory(@"translation\current"));
MissingKeysResult missing = MissingKeysAnalyzer.FindMissingKeys(newStorage, translation);

// Запись недостающих ключей в папку
List<WrittenFileInfo> missingFiles = ymlWriter.WriteMissingKeysYmlFiles(
    missing, @"output\missing", "english", newFiles);

// Слияние переводов
LocalizationStorage oldTranslation = new();
oldTranslation.AddFiles(parser.ParseDirectory(@"translation\old"));
LocalizationStorage newTranslation = new();
newTranslation.AddFiles(parser.ParseDirectory(@"translation\update"));

LocalizationMerger merger = new();
List<LocalizationFile> merged = merger.Merge(
    oldTranslation, newTranslation, newStorage, "russian");

// Сохранение результата
ymlWriter.WriteFiles(merged, @"translation\merged");

// Сохранение настроек
configService.Save(state);
```
