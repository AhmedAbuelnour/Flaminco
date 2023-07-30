# Introduction

This package should be used for cases that when you want to apply a set of rules and you want these rules to have an effect to your object, in that case you should think about using this package. 
By applying the Pipeline design pattern, each pipe will have an effect on your source object based on your custom logic in each pipe.

# Getting Started 

1- Inject into your DI 
 ``` 
builder.Services.AddPipelines<IPipelinesScanner>();
 ``` 
2- Define your object 
  
 ``` 
public class Example
  {
      public int Value { get; set; }
  }
```
  
3- Define a pipe handler  
   using Pipeline attribute to define some metadata, such as the pipeline group name and the order of execution.
   
 ``` 

    [Pipeline<Example>(Order = 1)]
    public class AddingOnePipeline: IPipelineHandler<Example>
    {
        public ValueTask Handler(Example source, CancellationToken cancellationToken = default)
        {
            source.Value = 1;
            
            return ValueTask.CompletedTask;
        }
    }
    
    [Pipeline<Example>(Order = 2)]
    public class AddingTwoPipeline: IPipelineHandler<Example>
    {
        public ValueTask Handler(Example source, CancellationToken cancellationToken = default)
        {
            source.Value = 2;
            
            return ValueTask.CompletedTask;
        }
    }
     
``` 
4- Fire the pipeline engine 
 ``` 
       private readonly IPipeline _pipeline;
       public DemoController(IPipeline pipeline)
       {
            _pipeline = pipeline;
       }
       
        public async Task<Example> Get()
        {
            Example example = new Example 
            {
                Value = 0
            };
            
            Example  newExample = await  _pipeline.ExecutePipeline(example, cancellationToken);
            
            return newExample; // result value = 2
        }
 ``` 
