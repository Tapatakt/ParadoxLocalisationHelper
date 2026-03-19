# Style Guide

## Общие правила

### Не использовать операторы верхнего уровня

Писать класс Program в явном виде.

### Типы и var
- **Не использовать** `var`, кроме случаев, когда название класса очень длинное и его запись очевидно избыточна
- Примеры допустимого использования `var`:
  ```csharp
  // Хорошо
  int a = 10;
  Dictionary<string, List<SomeNamespace.AnotherType>> dict = [];
  foreach (var kvp in dict) { /*...*/ }
  // Плохо
  var a = 10;
  var dict = new Dictionary<string, List<SomeNamespace.AnotherType>>();
  ```

### Конструкторы
- Опускать имя класса в конструкторе, когда оно и так очевидно из контекста (target-typed new)
  ```csharp
  // Хорошо
  Person person1 = new("John");
  Person person2 = new() { Name = "John" };
  // Плохо
  Person person1 = new Person("John");
  Person person2 = new Person() { Name = "John" };
  ```

### Фигурные скобки
- Не писать необязательные фигурные скобки для однострочных блоков
  ```csharp
  // Хорошо
  if (condition)
      DoSomething();
  
  // Плохо
  if (condition)
  {
      DoSomething();
  }
  ```

### Expression-bodied members
- Писать функции из одного `return` через `=>`
  ```csharp
  // Хорошо
  public int Calculate(int x) => x * 2;
  
  // Плохо
  public int Calculate(int x)
  {
      return x * 2;
  }
  ```

### Collection expressions
- Использовать collection expressions где уместно
  ```csharp
  // Хорошо
  int[] numbers = [1, 2, 3];
  List<string> items = ["a", "b", "c"];
  Span<int> span = [1, 2, 3];
  
  // Плохо
  int[] numbers = new[] { 1, 2, 3 };
  List<string> items = new List<string> { "a", "b", "c" };
  ```

## Синхронизация

- Для синхронизации использовать `Lock` (C# 13+), а не `object`
  ```csharp
  // Хорошо
  private readonly Lock _lock = new();
  
  // Плохо
  private readonly object _lock = new();
  ```

## Обработка ошибок

### Не допускать "тихих провалов"
- При неправильных аргументах функций должно выбрасываться исключение
  ```csharp
  // Хорошо
  public void DoWork(string name)
  {
      ArgumentException.ThrowIfNullOrEmpty(name);
      // ...
  }
  
  // Плохо
  public void DoWork(string name)
  {
      if (string.IsNullOrEmpty(name))
          return; // Тихий провал!
      // ...
  }
  ```

- При неожиданном значении для switch expression — выбрасывать исключение
  ```csharp
  // Хорошо
  var result = status switch
  {
      Status.Active => "Active",
      Status.Inactive => "Inactive",
      _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unexpected status value")
  };
  
  // Плохо
  var result = status switch
  {
      Status.Active => "Active",
      Status.Inactive => "Inactive",
      _ => null // Тихий провал!
  };
  ```

## Документация

- Все методы, свойства и публичные поля должны иметь XML-комментарии
  ```csharp
  /// <summary>
  /// Calculates the area of the rectangle.
  /// </summary>
  /// <param name="width">The width of the rectangle.</param>
  /// <param name="height">The height of the rectangle.</param>
  /// <returns>The calculated area.</returns>
  /// <exception cref="ArgumentException">Thrown when width or height is negative.</exception>
  public double CalculateArea(double width, double height)
  {
      ArgumentOutOfRangeException.ThrowIfNegative(width);
      ArgumentOutOfRangeException.ThrowIfNegative(height);
      return width * height;
  }
  ```

## Размер кода

- Стараться избегать методов длиннее ~30-35 строк
- Стараться избегать классов длиннее ~200 строк
- При превышении этих лимитов — декомпозировать на более мелкие методы/классы
