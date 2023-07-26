# FastOptions
A significantly faster drop in replacement for the default OptionsManager provided by ASP.NET core.

## Background
The default OptionsManager implementation for IOptionsSnapshot<> has significant performance deficiencies.
This library provides a workaround for these deficiencies by attempting to utilize IOptionsMonitor<> when possible.
The library falls back to OptionsManager when IOptionsMonitor<> is not available.

[The performance issue is being tracked here.](https://github.com/dotnet/runtime/issues/53793)

## Usage
```csharp
services.AddFastOptions();
```
You can then inject IOptionsSnapshot<> as you normally would.