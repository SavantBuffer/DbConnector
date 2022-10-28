# DbConnector
A performance-driven and ADO.NET data provider-agnostic ORM library for .NET

>:thumbsup:Tip  
Please visit [https://www.savantbuffer.com](https://www.savantbuffer.com) for the complete documentation!


## Introduction

#### What is a DbConnector?

DbConnector is a performance-driven and ADO.NET data provider-agnostic ORM library for .NET developed for individuals who strive to deliver high-quality software solutions. [Object-Relational Mapping](https://en.wikipedia.org/wiki/Object-relational_mapping) (ORM) is a technique that lets you query and manipulate data from a database using an object-oriented paradigm. This highly efficient library helps with the task of projecting/mapping data from any database, with the support of any third party data provider, into .NET objects and is comparable to the use of raw [ADO.NET](https://en.wikipedia.org/wiki/ADO.NET) data reader implementations.

#### Why use SavantBuffer's DbConnector?

The purpose of this library is not to replace the [Entity Framework](https://en.wikipedia.org/wiki/Entity_Framework) since it can be very useful in certain scenarios.   It's for those who prefer to write SQL queries with optimal performance in mind. Some might say writing plain-text SQL queries is easier to understand and debug than using [LINQ](https://en.wikipedia.org/wiki/Language_Integrated_Query) or .NET’s query syntax. Maybe someone in your team tends to lean towards a "stored procedure only" architecture? You might also be migrating your old [Data Access Layer](https://en.wikipedia.org/wiki/Data_access_layer) code into a more “modern” framework (e.g. .NET MVC). If you can relate to one of the mentioned, you’ll be happy to use this library. This documentation will further serve as guidance while providing examples on how to tackle these situations and more.

#### How fast is DbConnector?

It's extremely fast and memory efficient! Check out the [Performance](https://www.savantbuffer.com/DbConnector/index.html#section-6_) section for more details.


## Requirements

Before we start, this documentation assumes you have knowledge on the following:

* C# 6.0 or later (other .NET languages are supported but examples will not be provided in this documentation)
* .NET Framework 4.5 or later (or .NET Core 1.0 or later)
* SQL
* Visual Studio 2017 or later
* [NuGet](https://www.nuget.org/) packages
* One or more ADO.NET data-providers (e.g. [SqlClient](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/data-providers), [Npgsql](http://www.npgsql.org/), etc.)

These topics are important for you to know since this library was developed leveraging the latest built-in .NET features. To name a few, some of these features are lambda delegates, the task-based async model, and value tuples.


## Installation

DbConnector is installed via Visual Studio's NuGet package manager: [https://www.nuget.org/packages/SavantBuffer.DbConnector ](https://www.nuget.org/packages/SavantBuffer.DbConnector)

```ini 
PM> Install-Package SavantBuffer.DbConnector
```

>:warning:Warning  
[.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0) and its implementations are only supported.


## Getting Started

#### DbConnector Instance

Once you've downloaded and/or included the package into your .NET project, you have to reference the DbConnector and your preferred ADO.NET data provider namespaces in your code:

```csharp
using DbConnector.Core;
using System.Data.SqlClient; //Using SqlClient in this example.
```
Now, we can create an instance of the DbConnector using the targeted [DbConnection](https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbconnection?view=netframework-4.8) type:

```csharp
//Note: You can use any type of data provider adapter that implements a DbConnection.
//E.g. PostgreSQL, Oracle, MySql, SQL Server

//Example using SQL Server connection
DbConnector<SqlConnection> _dbConnector = new DbConnector<SqlConnection>("connection string goes here");
```
>:thumbsup:Tip  
DbConnector instances should be used as a singleton for optimal performance.

#### Main Functions

There are multiple functions available depending on your goals. The ones for reading data start with the word “Read” and “Non” for non-queries. For a better understanding, the [IDbCommand Interface](https://docs.microsoft.com/en-us/dotnet/api/system.data.idbcommand?view=netframework-4.8) method naming convention was followed for all the functions. This was decided in order to assist in the migration of raw data provider code into DbConnector’s design pattern.

##### The following are the main generic functions:

* Read
* [ReadAsAsyncEnumerable (v1.6.0)](https://www.savantbuffer.com/DbConnector/index.html#item-3-9_)
* ReadFirst
* ReadFirstOrDefault
* ReadSingle
* ReadSingleOrDefault
* ReadToList
* ReadToDataTable
* ReadToDataSet
* ReadToKeyValuePairs
* ReadToDictionaries
* ReadToListOfKeyValuePairs
* ReadToListOfDictionaries
* NonQuery
* NonQueries
* Scalar
* Build

#### Basic Example

Let’s say we have an “Employee” class serving as a container for the data we want to fetch from the database. The following function example shows how to synchronously load data from a database entity called “Employees” into an object of type List&lt;Employee&gt;:

```csharp
public List<Employee> GetAll()
{
    return _dbConnector.ReadToList<Employee>(
        onInit: (cmd) =>
        {
            cmd.CommandText = "SELECT * FROM Employees";

        }).Execute();
}

public List<Employee> GetAllSimple()
{
    //v1.1 Allows the use of simpler overloads:
    return _dbConnector.ReadToList<Employee>("SELECT * FROM Employees").Execute();
}
```

#### What happened here?

The DbConnector was used to call the **ReadToList** function with the use of an “Employee” generic type. This function is requesting an Action&lt;IDbJobCommand&gt; delegate to be declared which is being explicitly provided by the use of the “onInit” named parameter. Inside this delegate, the **IDbJobCommand** argument object is then used to set the text command to run against the data source. 

Noticed how a property called “CommandText” was set? This is because the [IDbCommand Interface](https://docs.microsoft.com/en-us/dotnet/api/system.data.idbcommand?view=netframework-4.8) naming convention is being followed like previously mentioned. 

Lastly, a function called **Execute** was called. A functional design pattern is being used and, consequently, an IDbJob object is being returned. This pattern allows for a lot of flexibility and you'll learn more in the following sections.


## More Documentation
Please visit [https://www.savantbuffer.com](https://www.savantbuffer.com) for the complete documentation!


## Examples and Tests
* [https://github.com/SavantBuffer/DbConnector/tree/master/DbConnector.Example](https://github.com/SavantBuffer/DbConnector/tree/master/DbConnector.Example)
* [https://github.com/SavantBuffer/DbConnector/tree/master/DbConnector.Tests](https://github.com/SavantBuffer/DbConnector/tree/master/DbConnector.Tests)


## Release Notes

Please see DbConnector’s [CHANGELOG](https://github.com/SavantBuffer/DbConnector/blob/master/CHANGELOG.md) for details.

#### Known-Issues

Issues can be reported via DbConnector’s [GitHub Issues](https://github.com/SavantBuffer/DbConnector/issues) feature.

#### Pending Features

New feature requests can be submited using DbConnector’s [GitHub](https://github.com/SavantBuffer/DbConnector/issues) repo.


## License

>:loudspeaker:Notice   
Copyright 2019 Robert Orama  
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at <br><br>http://www.apache.org/licenses/LICENSE-2.0<br><br>
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

## Contact

>Hello, <br> My name is Robert Orama and I'm the author of the DbConnector library. As a computer engineer and software enthusiast, my goal is to share knowledge by means of this project. I hope the use of DbConnector is taken into serious consideration and can help provide efficient results for any type of .NET solution.<br><br>You can reach me via email if really necessary:
[rorama@savantbuffer.com](rorama@savantbuffer.com)<br><br>[Don’t forget to donate](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=JZVZHQMVAG3HE&item_name=Donation+for+DbConnector+open-source+library+www.savantbuffer.com&currency_code=USD&source=url)<br><br>Thank you for your support!
