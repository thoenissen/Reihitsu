# Statements
Some statement should be preced with an empty line. This rule should not be applied if it is located at the beginning of a scope. 

## Declaration statements
```csharp
var x = 0;
var y = 0;
```

## Expression statements
Assignment and method call should be seperated by a single empty line.

```csharp
stepCount = 2;
x += stepCount;
y += stepCount;

Console.Writeline($"X: {x});
```

```csharp
var isValid = entry.PropertyOne && && entry.PropertyTwo;

var isValid = entry.PropertyOne
           && entry.PropertyTwo
           && entry.PropertyThree;

var isValid = entry.PropertyOne
           || (entry.PropertyTwo
            && entry.PropertyThree);
```

```csharp
var value = CheckEntry(entry)
               ? entry
               : GetDefaultValue();
```

```csharp
var entries = GetEntries().Where(obj => obj.IsValid)
                          .ToList();

var entries = GetEntries().Where(obj =>
                                 {
                                     if (obj.IsValid)
                                     {
                                         return true;
                                     }

                                     return CheckEntry(obj);
                                 })
                          .Select(obj => new
                                         {
                                             obj.Id,
                                             obj.Description
                                         })
                          .ToList();
```

## Selection statements
### if
```csharp
if (expression)
{
    ...
}
```

### case

Single line

```csharp
switch (value)
{
    case 0:
    case 1:
        ...
        break;

    case 2:
        ...
        break;

    default:
        ...
        break;
}
```

Multi line

```csharp
switch (value)
{
   case 0:
   case 1:
       {
           ...
       }
       break;

   case 2:
       {
           ...
       }
       break;

   default:
       {
           ...
       }
       break;
}
```

## Iteration statements
### do
```csharp
Debug.WriteLine("Starting loop");

do
{
    ...
}
while (true)
```
### for
```csharp
var entries = GetEntries();

for (var i = 0; i < entries.Length; i++)
{
    ...
}
```
### foreach
```csharp
var filtedEntries = entries.Where(OnFilter)
                           .OrderBy(obj => ob.Id);

foreach (var enry in entries)
{
    ...
}
```
### while
```csharp
var entries = GetEntries();

while (entries.Count > 0)
{
    ...
}
```

## Jump statements
### break

```csharp
if (abortOperation)
{
   Console.WriteLine("Operation aborted!");

   break;
}
```

### continue
```csharp
if (entry.IsRelevant == false)
{
   Console.WriteLine($"Entry skipped: {entry}");

   continue;
}
```

### goto
```csharp
if (abortOperation)
{
   Console.WriteLine("Operation aborted!");

   goto Abort;
}
```
### return
In general it is prefered that you use only one return statemtn in a method. Exceptions can be made if the method is less then ten lines.

```csharp
if (entry.IsValid)
{
   Debug.WriteLine($"Returning entry: {entry}")

   return entry;
}
```

### yield
```csharp
if (entry.IsValid)
{
   Debug.WriteLine($"Returning entry: {entry}")

   yield return entry;
}
```

## Exception-handling statements
### throw
```csharp
_value = value ?? throw new ArgumentNullException(nameof(value));
```

```csharp
Debug.WriteLine("Invalid entry");

throw new InvalidEntryException(entry);
```
### try-catch-finally
```csharp
try
{
    ...
}
catch (FileNotFoundException ex)
{
   ...
}
catch (Exception ex)
{
   ...
}
finally
{
    ...
}

```

## checked and unchecked
```csharp
unchecked
{
    _currentPosition += step;
}

checked
{
    _currentPosition += step;
}
```

## The `await` statement
```csharp
var entry = await GetEntry().ConfigureAwait(false);

if (await entry.IsValid().ConfigureAwait(false))
{
   await Process().ConfigureAwait(false);
}
```
## The `fixed` and `unsafe` statement
```csharp
unsafe
{
    var array = GetArray();

    fixed (byte* firstElement = array)
    {
        ...
    }
}
```

## The `lock` statement
```csharp
var entries = GetEntries();

lock
{
    ...
}
```

## The `using` statement
```csharp
using (var connection = CreateConnection())
{
    ...
}
```

# Types

## Initialization
### Fields
Fields can be initialitzed through direct assignment but only if the assignment is single lined. Multi line assignments should be made in the constructor oder seperated into a own methid.

### Properties
Properties should be initialized only through the constructor.

## Template arguments
```csharp
public class Container<TKey, TElement> : Base<T>,
                                         IEnumerable<T>
   where TKey : class
   where TElement : class
{
    ...
}
```
## Properties
```csharp
public int AutoProperty { get; set; }

public int BackingFieldProperty
{
   get => _field;
   set => _field = value;
}

public int LogicProperty
{
   get => _field;
   set
   {
      _field = value;

      RaisePropertyChanged();
   }
}
````

## Methods
```csharp
public void ExampleMethod(int parameterOne, int parameterTwo, int parameterThree, int parameterFour)
{
    ....
}

public void ExampleMethod(int parameterOne,
                          int parameterTwo,
                          int parameterThree,
                          int parameterFour,
                          int parameterFive)
{
    ....
}
```

```csharp
ExampleMethod(1, 2, 3, 4);

ExampleMethod(1, 2, 3, 4, 5, 6);

ExampleMethod(GetValueOne(), GetValueTwo(), GetValueThree(), GetValueFour()); 

ExampleMethod(GetValueOne(),
              GetValueTwo(),
              GetValueThree(),
              GetValueFour()
              GetValueFive());

ExampleMethod(GetEntries().Where(obj => obj.IsValid)
                          .ToList(),
              GetValueTwo());
```

# Regions
The `region` and `endregion` tag should be surrounded by blank lines. The start should have the format `#region [Name]` and end of the region the format `#endregion // [Name]`. The indentation is based on the current source code section.

```csharp
#region Properties

/// <summary>
/// Counter
/// </summary>
public int Counter { get; set; }

#endregion // Properties
```

# LINQ
While using LINQ the method syntax should be prefered.