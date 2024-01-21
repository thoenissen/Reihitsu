# Casing

The casing should be according to following table. The casing conventions are not impacted by the keywords static or readonly.

Abbreviations should be avoided while naming. For acronyms, only the first letter should be capitalized when using PascalCase.

| Type                 | Casing              | Example                                                 |
| -------------------- | ------------------- | ------------------------------------------------------- |
| Namespaces           | PascalCase          | `namespace Company.Department.Application;`             |
| Classes              | PascalCase          | `class CasingDocumentation`                             |
| Attributes           | PascalCaseAttribute | `class CasingAttribute : Attribute`                     |
| Structures           | PascalCase          | `struct CasingDocumentation`                            |
| Enumerations         | PascalCase          | `enum CasingType`                                       |
| Enum members         | PascalCase          | `PascalCase`                                            |
| Interfaces           | IPascalBase         | `interface ICasingDocumention`                          |
| Events               | PascalCase          | `public event EventHandlerCasingChanged CasingChanged`  |
| Delegates            | PascalCase          | `public void CasingChanged(object sender, EventArgs e)` |
| Methods              | PascalCase          | `public void GetDocumentation()`                        |
| Local Functions      | PascalCase          | `void GetDocumentation()`                               |
| Method parameters    | camelCase           | `(string description)`                                  |
| Private fields       | _camelCase          | `private string _description;`                          |
| Protected fields     | _camelCase          | `protected string _description;`                        |
| Public fields        | PascalCase          | `public string Description`                             |
| Internal fields      | PascalCase          | `internal string Description`                           |
| Const fields         | PascalCase          | `private int DocumentionId = 1;`                        |
| Private Propertes    | PascalCase          | `private string Description { get; set; }`              |
| Protected Properties | PascalCase          | `protected string Description { get; set; }`            |
| Internal Properties  | PascalCase          | `internal string Description { get; set; }`             |
| Public Properties    | PascalCase          | `public string Description { get; set; }`               |
| Enumerations         | PascalCase          | `public enum DocumentationType`                         |
| Local variables      | camalCase           | `var description = string.empty;`                       |
| Tuples               | PascalCase          | `(int Id, string Description)`                          |
| Tuple Decontruction  | camelCase           |`var (id, description)`                                  |
|                      |                     |                                                         |
| Files                | PascalCase          | `CasingDocumentation.cs`                                |

# Naming
## Variable
If you declare a variable the Hungarian Notation should not be used. 

### `bool`
All type of variables (fields, properties, local variables, etc.) of the type `bool` should start with `Is` if it es possible.

## Types
### Template arguments
A type argument of a template class should always start with a `T`. If only one type argument is used it can be called `T` or `T[Specification]`.

```csharp
public class Container<T>
   where T : class
{
    ...
}
```

If using more then one type argument it should always be named `T[Specification]`.

```csharp
public class Container<TKey, TElement>
   where TKey : class
   where TElement : class
{
    ...
}
```

### Events
A methode which raises a event should by called `Raise[EventName]`. This is explicit used to raise the event.

```csharp
private void RaiseDocumentationChanged()
{
   DocumentationChanged?.Invoke();
}
```

When subscribing to an event the method should be name like the event (`On[EventName]`). Also the short syntax to subscribe should be used.

```csharp
object.DocumentaitonChanged += OnDocumentationChanged;
```

If the naming causes duplicate the name of the member should be added to the name to all methods (`On[MemberName][EventName]`).

```csharp
general.DocumentaitonChanged += OnGeneralDocumentationChanged;
type.DocumentationChanged += OnTypeDocumentationChanged;
```

When declaring an event, the name of the event handler should be adopted (`[Event handler name without EventHandler]`).

```csharp
public delegate void DocumentationChangedEventHandler();

...

public event DocumentationChangedEventHandler DocumentationChanged;
```

You can extent the name to give the event more context. (`[Additional specification][Event handler name without EventHandler]`).

```csharp
public event DocumentationChangedEventHandler GeneralDocumentationChanged;

...

public event DocumentationChangedEventHandler TypeDocumentationChanged;
```

### Commands
If you declare a command you should always to use the type `ICommand`. The private field should also use the type `ICommand` to create a uniform overall picture.

```csharp
private ICommand _cmdOpenDocumentation;

...

public ICommand CmdOpenDocumentation => _cmdOpenDocumentation ??= new RelayCommand(OnOpenDocumentation);
```

## Files
The Filename must match the class, enumeration or structure. For template classes, template arguments are ignored when forming the file name. 