# LowCodeHub.Operations

Single-responsibility operations with a source generator. One operation = one use case; replaces a large service layer with small, injectable operations.

## Quick start

1. **Reference the package** (project reference to this project or its NuGet).

2. **Define an operation** (one partial class, one file). Your `ExecuteAsync` is the only execution method.

```csharp
using ErrorOr;
using LowCodeHub.Operations;

namespace MyApp.Application.Operations;

[Operation]  // or [Operation("IUploadAttachmentOperation")] to set the emitted interface name
public partial class UploadAttachment
{
    public record Request(Stream FileStream, string FileName, string ContentType, long FileSize, string? Language = "en");
    public record Response(Guid Id, string FileName, string Url, DateTime Created);

    public UploadAttachment(ILogger<UploadAttachment> logger, IAttachmentRepository repo, IAttachmentStorage storage)
        => (_logger, _repo, _storage) = (logger, repo, storage);

    private readonly ILogger<UploadAttachment> _logger;
    private readonly IAttachmentRepository _repo;
    private readonly IAttachmentStorage _storage;

    public async Task<ErrorOr<Response>> ExecuteAsync(Request request, CancellationToken ct)
    {
        // Your logic only
        // ...
        return new Response(...);
    }
}
```

3. **Register operations** in DI (e.g. in your module or host):

```csharp
services.AddOperations(typeof(UploadAttachment).Assembly);
```

4. **Use in an endpoint** (short generated interface):

```csharp
public static async Task<IResult> Handle(
    IUploadAttachmentOperation operation,  // generated short name
    CancellationToken cancellationToken)
{
    var result = await operation.ExecuteAsync(request, cancellationToken);
    return result.IsError ? ... : Results.Ok(result.Value);
}
```

## Operations with no request or no response

**No request** – e.g. “get current config”, “refresh cache”:

- Omit the nested `Request` type. Implement **`ExecuteAsync(CancellationToken cancellationToken)`** (single parameter). The generator uses **`Unit`** as the request type and forwards the interface call to your method.

```csharp
[Operation]
public partial class RefreshCache
{
    public record Response(DateTime RefreshedAt);

    public async Task<ErrorOr<Response>> ExecuteAsync(CancellationToken ct)
    {
        // ...
        return new Response(DateTime.UtcNow);
    }
}
```

**No response (use your own type)** – e.g. “delete”, “update”:

- Omit the nested `Response` type. Return **`Task<ErrorOr<T>>`** where **T** is any type you want: **`Success`**, **`Deleted`**, **`Updated`** from the ErrorOr package, or your own marker type. The generator infers the response type from your return type.

```csharp
using ErrorOr;  // Success, Deleted, Updated, etc.

[Operation]
public partial class DeleteItem
{
    public record Request(Guid Id);

    public async Task<ErrorOr<Deleted>> ExecuteAsync(Request request, CancellationToken ct)
    {
        // ...
        return Result.Deleted;
    }
}
```

**No request and no response** – two options:

- Return **`Task<ErrorOr<Unit>>`** and use **`Unit.Value`** (or **`default`**) on success; or
- Return **`Task<ErrorOr<Success>>`** (or **`Deleted`** / **`Updated`**) if you prefer those.

```csharp
[Operation]
public partial class RunMaintenance
{
    public async Task<ErrorOr<Success>> ExecuteAsync(CancellationToken ct) => Result.Success;
}
```

**Summary:** **`Unit`** is only used when there is **no request** (single-parameter `ExecuteAsync(CancellationToken)`). For “no response” you can use **Success**, **Deleted**, **Updated**, or any other type; no nested `Response` and no `Unit` required.

## Using Guid, string, or any type directly (no Request/Response wrapper)

You can use **primitive or simple types** as request or response without defining nested `Request`/`Response` types. The generator infers them from your **ExecuteAsync** signature.

**Request = Guid, Response = some type:**

- Omit nested `Request`. Implement **`ExecuteAsync(Guid id, CancellationToken ct)`** (or `string`, `int`, etc.). The generator uses that parameter type as the request type.

```csharp
[Operation]
public partial class GetAttachmentById
{
    public record Response(string FileName, string Url);  // or omit for a primitive response

    public async Task<ErrorOr<Response>> ExecuteAsync(Guid id, CancellationToken ct)
    {
        // ...
        return new Response(...);
    }
}
// Injects as IOperation<Guid, Response> (or IGetAttachmentByIdOperation)
```

**Request = your type, Response = string (or Guid, etc.):**

- Omit nested `Response`. Return **`Task<ErrorOr<string>>`** (or **`ErrorOr<Guid>`**, etc.). The generator infers the response type from the return type.

```csharp
[Operation]
public partial class GetAttachmentUrl
{
    public record Request(Guid Id);

    public async Task<ErrorOr<string>> ExecuteAsync(Request request, CancellationToken ct)
        => await _repo.GetUrlAsync(request.Id, ct);
}
```

**Request = Guid, Response = string (no nested types):**

```csharp
[Operation]
public partial class GetNameById
{
    public async Task<ErrorOr<string>> ExecuteAsync(Guid id, CancellationToken ct)
        => await _repo.GetNameAsync(id, ct);
}
// IOperation<Guid, string>
```

So you can **wrap in Request/Response when you want a clear contract**, or **use Guid/string/any type directly** when that’s enough.

## Generated interface name (no duplicates)

Your **class name can end with `Operation`** (e.g. `DownloadAttachmentOperation`). The generated interface is always different so there is no duplicate:

| Class name                 | Generated interface name     |
|----------------------------|------------------------------|
| `UploadAttachment`         | `IUploadAttachmentOperation` |
| `DownloadAttachmentOperation` | `IDownloadAttachmentOperation` |
| `GetUser`                  | `IGetUserOperation`           |

**Rule:**

1. If you set **`[Operation("ICustomName")]`** → that exact name is used.
2. Else if the class name **ends with `Operation`** → **`I` + class name** (e.g. `DownloadAttachmentOperation` → `IDownloadAttachmentOperation`).
3. Else → **`I` + class name + `Operation`** (e.g. `UploadAttachment` → `IUploadAttachmentOperation`).

So the interface is always prefixed with `I` and never has the same name as the class.

## Why two references? Why don’t I see the generated interfaces?

The **runtime** (e.g. `IOperation`, `[Operation]`, `AddOperations`) lives in **LowCodeHub.Operations**. The **source generator** lives in **LowCodeHub.Operations.SourceGenerators**. In MSBuild, **analyzers from a ProjectReference do not flow** to the project that references you: so when “UseCase A” references only `LowCodeHub.Operations`, the generator never runs for UseCase A. That’s why the short interfaces (e.g. `IUploadAttachmentOperation`) are not generated unless the **project that contains the `[Operation]` classes** also references the generator.

So you need **two** things in the project that defines the operations:

1. **Runtime:** `ProjectReference` to **LowCodeHub.Operations.csproj** (for `IOperation`, `[Operation]`, `AddOperations`).
2. **Generator:** A reference to **LowCodeHub.Operations.SourceGenerators.csproj** as an **Analyzer** (so the generator runs and emits the short interfaces).

That’s why it looks “duplicated” – one project for runtime, one for the analyzer; both are required in the use-case project if you want generated interface names.

**Ways to add both in one place:**

**Option A – One Import (if your paths allow):** Use the shared props file so a single Import adds both references. Path is relative to your `.csproj`:

```xml
<Import Project="../../../Nugets/LowCodeHub.Operations/LowCodeHub.Operations.props" />
```

(Adjust the path for your project; e.g. from `Backend/Core/UseCases/MyUseCase` the path above is correct.) The props file adds the Operations project and the SourceGenerators analyzer. If the short interfaces still don’t appear after using the Import, use Option B (explicit ProjectReferences) instead.

**Option B – Two ProjectReferences:** Add both explicitly (no Import):

```xml
<ProjectReference Include="..\..\..\Nugets\LowCodeHub.Operations\LowCodeHub.Operations.csproj" />
<ProjectReference Include="..\..\..\Nugets\LowCodeHub.Operations\SourceGenerators\LowCodeHub.Operations.SourceGenerators.csproj"
                 PrivateAssets="all"
                 ReferenceOutputAssembly="false"
                 OutputItemType="Analyzer" />
```

**Option C – No generator:** Reference only **LowCodeHub.Operations** and inject **`IOperation<YourClass.Request, YourClass.Response>`** in endpoints. No short interface names, but only one reference.

## What the source generator does

- Adds a partial that makes your class implement the generated interface (e.g. `IUploadAttachmentOperation`). Your `ExecuteAsync` is the implementation; no extra method name.
- When there is no request, you can implement **`ExecuteAsync(CancellationToken)`**; the generator forwards **`ExecuteAsync(Unit, CancellationToken)`** to it.
- Emits a short-named interface (see table above). You can override with **`[Operation("IYourInterfaceName")]`** or **`[Operation(InterfaceName = "IYourInterfaceName")]`**.

## Requirements

- Your operation class must be **partial**.
- **Request** and **Response**: either nested types (record or class) or omitted.
  - When **Request** is omitted: use **`ExecuteAsync(CancellationToken)`** (request = **Unit**) or **`ExecuteAsync(T request, CancellationToken)`** with **T** = **Guid**, **string**, or any type (request = **T**).
  - When **Response** is omitted: response type is **inferred from your return type** (e.g. **string**, **Guid**, **Success**, **Deleted**, or any type).
- A method **ExecuteAsync** with one of:
  - **`(TRequest request, CancellationToken cancellationToken)`** → **`Task<ErrorOr<TResponse>>`** (nested or inferred types).
  - **`(CancellationToken cancellationToken)`** → **`Task<ErrorOr<TResponse>>`** (no request; request = **Unit**).
- **ErrorOr** package (referenced by this project).

See **DESIGN.md** for the full design and goals.
