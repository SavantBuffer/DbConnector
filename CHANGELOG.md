# [v1.1.1](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.1.1)  | Nov 20, 2019

### Bug Fixes
 - **documentation:** Fixed function documentation and settings for NuGet package
 

# [v1.1](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.1)  | Nov 19, 2019

### Bug Fixes
 - **parameters:** Missing `DbJobParameterCollection.AddFor` overload for non-generic objects
 - **documentation:** Fixed function documentation thus enabling a better IntelliSense experience
 
### Features
 - Faster and simpler overloads ([#1](https://github.com/SavantBuffer/DbConnector/issues/1))
 - Cache clearing via `DbConnectorCache.ClearCache()` or `DbConnectorCache.ClearColumnMapCache()`
 - Get current column map cache count using `DbConnectorCache.GetColumnMapCacheCount()`
 
### Performance Improvements 
 - Faster execution across the entire library's functionality
 

# [v1.0](https://github.com/SavantBuffer/DbConnector/releases/tag/v1.0) | Nov 6, 2019

Initial Release
