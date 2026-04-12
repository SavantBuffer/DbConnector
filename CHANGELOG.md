# [v1.8.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.8.0)  | Apr 12, 2026

### Features
 - New support for mapping database query results directly to `ValueTuple` and classic `Tuple` types, enabling lightweight, class-free projections from query columns to strongly-typed tuple elements by ordinal position.
 - Integration of tuple mapping into Read, ReadToList, ReadFirst, Scalar, and their async/enumerable variants. These enhancements make it easier to work with ad-hoc result sets without defining custom classes, improving developer productivity and flexibility.
 - Additional XML documentation and code comments for tuple-related logic.

### Performance Improvements 
 - Minor logic optimizations for list and enumerable functions, improving performance when working with large result sets.
 - Caching for dynamic setter delegates and Enum.TryParse methods to improve performance.


# [v1.7.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.7.0)  | Mar 29, 2026

### Performance Improvements 
 - Replaced Activator.CreateInstance with ILObjectFactory for high-performance, allocation-free object instantiation throughout the codebase.
 - Refactored cache models in DbConnectorCache for robust, efficient hashing and equality, improving cache hit rates and concurrency.
 - Enhanced column map cache to capture more details and prevent collisions. Optimized ordinal column hash methods for speed and order sensitivity.

### Dependency Updates
 - Upgraded project dependencies (Dapper, EF Core, etc.), switched to Microsoft.Data.SqlClient, and updated target frameworks.


# [v1.6.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.6.0)  | Oct 27, 2022

### Features
 - New `IDbConnector.ReadAsAsyncEnumerable` functions. This great new feature can be used to build APIs (starting with ASP.NET Core 6.0) which can stream data from a database source. 
 
### Performance Improvements 
 - Faster execution for all `IDbConnector` reader functions.

### Dependency Updates
 - All

### New Dependencies
 - System.Linq.Async
 - Microsoft.Bcl.AsyncInterfaces


# [v1.5.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.5.0)  | Jul 20, 2021

### Bug Fixes
 - **execution:** Fixed .NET 5.0 `System.Reflection.AmbiguousMatchException` exception.

### Features
 - Support for `.NET 5.0`.

### Dependency Updates
 - System.ComponentModel.Annotations


# [v1.4.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.4.0)  | Sep 22, 2020

### Features
 - Property mapping is now case-insensitive for better flexibility.

# [v1.3.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.3.0)  | Jun 11, 2020

### Bug Fixes
 - **execution:** Fixed `System.NullReferenceException` exception in `IDbConnector.Scalar` functions when empty query results are encountered.
 
### Features
 - New `IDbJob.Execute` overloads which allow a custom/external `DbTransaction` as an argument. (Also applies to static `DbJob` functions)
 - New `IDbConnector.ReadToHashset` functions.
 - Minor documentation improvements.
 
### Performance Improvements 
 - Faster execution for `IDbConnector.Scalar` functions.
 

# [v1.2.1](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.2.1)  | Jun 03, 2020

### Bug Fixes
 - **execution:** Fixed `System.InvalidProgramException`, related to internally generated "IL Dynamic Methods", bug when targeting .NET Core 3.0 or later.


# [v1.2.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.2.0)  | May 25, 2020
 
### Features
 - Non-generic `IDbConnector` base interface. This allows to implement cleaner declarations when knowledge of connection types aren't necessary.
 - The main DbConnector class now provides new `ConnectionString` and `ConnectionType` properties.


# [v1.1.2](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.1.2)  | Apr 29, 2020

### Bug Fixes
 - **execution:** Batch-Reading, with "Read" or "ReadToList", would incorrectly assign NULL values instead of empty lists when encountering empty query results.
 - **documentation:** Fixed function documentation thus enabling a better IntelliSense experience.

### Dependency Updates
 - Microsoft.CSharp
 - System.ComponentModel.Annotations
 - System.Reflection.Emit.Lightweight


# [v1.1.1](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.1.1)  | Nov 20, 2019

### Bug Fixes
 - **documentation:** Fixed function documentation and settings for NuGet package.
 

# [v1.1](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.1)  | Nov 19, 2019

### Bug Fixes
 - **parameters:** Missing `DbJobParameterCollection.AddFor` overload for non-generic objects.
 - **documentation:** Fixed function documentation thus enabling a better IntelliSense experience.
 
### Features
 - Faster and simpler overloads ([#1](https://github.com/SavantBuffer/DbConnector/issues/1)).
 - Cache clearing via `DbConnectorCache.ClearCache()` or `DbConnectorCache.ClearColumnMapCache()`.
 - Get current column map cache count using `DbConnectorCache.GetColumnMapCacheCount()`.
 
### Performance Improvements 
 - Faster execution across the entire library's functionality.
 

# [v1.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.0) | Nov 6, 2019

Initial Release
