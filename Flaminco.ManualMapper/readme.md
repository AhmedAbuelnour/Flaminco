# Introduction

this package trying to set a structured method for implementing object mapping in both ways (Sync, Async)

# Getting Started

inject mapper scanner

```csharp
  builder.Services.AddManualMapper<Program>();

```

lets start with an example

```csharp

//Source Object
public class Person
{
    public int Age { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// Destination Object
public class PersonResponse
{
    public int Age { get; set; }
    public string FullName { get; set; }
}

// class to represent the mapping logic.
public class PersonMapper : IMapHandler<Person, PersonResponse>
{
    public PersonResponse Handler(Person source)
    {
        return new PersonResponse
        {
            Age = source.Age,
            FullName = $"{source.FirstName} {source.LastName}"
        };
    }
}


 public class PersonController : ControllerBase
 {
     private readonly IMapper _mapper;
     public PersonController(IMapper mapper)
     {
         _mapper = mapper;
     }

     [HttpGet]
     public PersonResponse Get()
     {
         return _mapper.Map<Person, PersonResponse>(new Person
         {
             Age = 1,
             FirstName = "Ahmed",
             LastName = "Abuelnour"
         });
     }
 }

```

Same example goes for Async Mapper, bus instead of using ```IMapHandler``` use ```IMapAsyncHandler```
and instead of using ``` _mapper.Map ```  use ``` _mapper.MapAsync ``` 
