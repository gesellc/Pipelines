# Refactoring To Async Tutorial

toc


## Prep

* Download the [sample exercise](https://github.com/refactoring-pipelines/PipelinesExercise) 
* Install the VsCode and the plugin [Graphviz (dot) language support for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=joaompinto.vscode-graphviz) by João Pinto
CTRL-SHIFT-V to preview a `.dot` file as a rendered graph.


## Step by Step

## Notes

 1. All pipeline setup code occurs at the top of the method.
 2. Then the approvals call
 3. Then the sending through the pipeline

### Template 

```cs
// Set up Pipeline
// ApprovalPipeline
// Send thru pipeline
// Original code
```
### 1. Call the method in question from a test

Simply call the method you wish to refactor from a test.
Don't think of this as a traditional unit test. Think of it more as a specialized `main()` method

### 2. Take the 1st thing to move to async and create an `InputPipe` of it's parameters directly above it.

``` cs
var startingPointInput = new InputPipe<InputType>("InputName");
```

### 3. Place a ApprovalTests to get insight into the pipeline.

Place this right in the middle of the production code you want to refactor. It is a temporary step.

``` cs
PipelineApprovals.Verify(startingPointInput);
```

With a DotReporter (on the test)

``` cs
[UseReporter(typeof(DotReporter))]
```

### 4. Inspect result in VsCode

### 5. Add a process as a delegate

If the code isn't in a method, extract it to one first

``` cs
 var methodCallPipe = startingPoint.Process(TheMethodCall)
```

Do this above the `PipelineApprovals.Verify()`. Run it again for feedback.

### 6. Add a collector, and send input in

``` cs
 var methodCallCollector = methodCallPipe.Collect();
 startingPoint.Send(firstParameter);
 var variable = methodCallCollector.SingleResult;
```

### 7. Process and Collect the next step in the pipeline

### 8. Delete any newly-dead code

Previously you were using `.Collect()` to pull out values from intermediate steps, but as they become unnecessary you can delete them. ReSharper is good for finding dead code.

### 9. Repeat until all code is converted

### 10. Use `GetInputs<>` to collect all the inputs and outputs in a single object:

``` cs
        var inputsAndOutputs = bestSandwichCollector.GetInputs<ZipCode>().AndOutputs<Sandwich>().AsTuple();
        inputsAndOutputs.Send(zipCode);
        inputsAndOutputs.Outputs...
```
### 10. Extract a CreatePipe function 

### 11. Move the `PipelineApprovals.Verify()` call into a unit test

``` cs
    var pipe = SimpleCalls.CreatePipe();
    PipelineApprovals.Verify(pipe.Input1);
```

## Handling multiple parameters

Every function has a single input and a single output. To use multiple input parameters, you'll have to join them into `Tuple`s. If this requires new inputs, you'll need multiple `InputPipe`s.
