# Structure

## Using directives
The file starts with all needed using directives. These directives are sorted alphabetical and group by the first part of the namespace. The exception is `System` which is always at the beginning.

```csharp
using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
```

# Namespace
The using directives are followed by the namespace. The declaration using the file scoped syntax are prefered.

```csharp
namespace Reihitsu.Analyzer.Rules.Documentation;
```

## Type declaration

The following sequence should be followed when using keywords:
- `public`
- `protected`
- `internal`
- `private`
- `static`
- `new`
- `abstract`
- `virtual`
- `override`
- `sealed`
- `readonly`
- `extern`
- `unsafe`
- `volatile`
- `async`

### Class
A class is structure by the following order. Nesting of types should also be avoided if possible.  

- Nested classes, enumerations, delegates and events
- Const field
- Static readonly fields
- Static fields
- Readonly Fields
- Fields
- Constructor
- Finalizer
- Events
- Commands (including Command-Method)
- Static Properties
- Properties
- Static Methods
- Methods
- Interface-Implementations

Additional to the order all members are wrapped into the following regions. Subordinate regions are only used if more than one modifier is used.

- Nested types
- Constants 
- Fields  
- Constructor / Finalizer
- Events
- Commands
   - Public commands
   - Internal commands
   - Protected commands
   - Private commands
- Properties
  - Public properties
  - Internal properties
  - Protected properties
  - Private properties
- Methods
  - Public methods
  - Internal methods
  - Protected methods
  - Private methods
- [Name of Interface]

### Struct
In general structs should be kept simple and only used for transfer a limited amount of data.

- Nested classes, enumerations, delegates and events
- Const field
- Static readonly fields
- Static fields
- Readonly Fields
- Fields
- Constructor
- Finalizer
- Events
- Static Properties
- Properties
- Static Methods
- Methods
- Interface-Implementations

Additional to the order all members are wrapped into the following regions.

- Nested types
- Constants 
- Fields  
- Constructor
- Events
- Properties
- Methods
- [Name of Interface]

### Interface
A interface is structure by the following order.

- Fields
- Events
- Commands
- Properties
- Methods

Additional to the order all members are wrapped into the following regions.

- Fields  
- Events
- Commands
- Properties
- Methods

### Enum
Only one enum should be defined per file. Each enum member should on it's one separated line.

### Delegate 
Only one delegate should be defined per file.