# Decorator Design Pattern

## Adding Telemetry to HttpClient

This repository demonstrates how to use the decorator design pattern to add telemetry to an HttpClient. 

The **Decorator Pattern** is a design pattern that allows developers to add additional functionality to existing 
classes/objects without modifying the existing class/object. 

This repository demonstrates how to use the decorator design pattern to add telemetry 
to an HttpClient in C#. By the end of this demonstration, you will have a good understanding 
of the decorator pattern and how to use it to add telemetry to an HttpClient.

By using the Decorator Pattern, developers can add 
additional properties and methods to the existing HttpClient class to add telemetry data to the request. 
This allows for easy tracking of requests and their associated data, such as request time, response time, and response size. 
By using the Decorator Pattern, the original HttpClient class remains unchanged, and the telemetry data is added 
through a “wrapper” or “decorator” class. This makes the code more maintainable, as the changes are isolated to 
the decorator class, while the original HttpClient class remains unchanged.

## Motiviation
To have a working version of the decorator pattern in C# with a real world example.
This repository demonstrates the power and flexibility of the decorator design pattern, 
and shows how to use it to add telemetry to an HttpClient in C#. 


### HttpClient & HttpClientFactory
HttpClient is a modern, easy-to-use, and powerful HTTP client library that is built 
into .NET. It provides a simple, asynchronous, and extensible way to make HTTP requests.
This project uses the IHttpClientFactory to create an HttpClient instance. 


### What is Telemetry?
Telemetry is the collection, transmission, and analysis of data related to the performance and 
usage of an application. Telemetry helps us to monitor the performance of an application, 
identify and troubleshoot issues, and understand how an application is being used.


## Build status
![Build Workflow](https://github.com/markhazleton/HttpClientDecoratorPattern/actions/workflows/dotnet.yml/badge.svg)

## Tech/framework used
Asp.Net C# code style, with out of the box Visual Studio 2022 settings.

## Features
 1. **Joke**        - Make an API call to the public JOKE API and display the results.
    - Single Call passing with Model from Joke API
    ![Joke Page](https://raw.githubusercontent.com/markhazleton/HttpClientDecoratorPattern/main/Images/JokeRazorPageResults.JPG)

 1. **Many Calls**  - Make multiple API calls and display results so you can compare telemetry values. 
    - Demontrate using Semphore Slim Apporach to limiting the number of concurrent calls. 
    ![Many Calls](https://raw.githubusercontent.com/markhazleton/HttpClientDecoratorPattern/main/Images/ListPageResults.JPG)

### List.cshtml Razor Page
This page is responsible for making multiple HTTP GET requests concurrently using asynchronous programming. 

The main method is called CallEndpointMultipleTimes amd retirms a list of Api call results.
The method takes three parameters:
- **maxThreads**: Represents the maximum number of threads to use for making the GET requests.
- **itterationCount**: Represents the number of GET requests to be made.
- **endpoint**: Represents the endpoint to send the GET request to.

This code creates a semaphore, SemaphoreSlim, to limit the number of concurrent requests to maxThreads. 
Then it creates a list of tasks, tasks, to store the results of the GetAsync calls. 
For each iteration of the loop, it waits for the semaphore to be available and then adds a new task to the tasks list, 
making a call to the GetAsync method. 
The result of each GetAsync call is added to a List<HttpGetCallResults> object, results.

Once all the tasks are complete, the method returns the results list and logs a message indicating that all 
calls have been completed. The SemaphoreSlim is used to limit the number of concurrent requests and ensure 
that the number of requests does not exceed the maxThreads value.

## Code Example
Show what the library does as concisely as possible, developers should be able to figure out **how** your project solves their problem by looking at the code example. Make sure the API you are showing off is obvious, and that your code is short and concise.

## Installation
  1. Clone this repository to your local machine.
  1. Open the solution file in Visual Studio.
  1. Run the Web project to see the demonstration in action.

## API Reference
- This project uses a freely availible Joke API. ( https://jokeapi.dev/ ) Joke API Project on GitHub: https://github.com/Sv443/JokeAPI

## Tests
Some limited test cases have been created, for the models created for this prooject. 

## How to use?
If people like your project they’ll want to learn how they can use it. To do so include step by step guide to use your project.

## Contribute
You welcome to create pull requests for update to this repository.  Please review the  [contributing guideline](https://github.1com/markhazleton/HttpClientDecoratorPattern/blob/main/CONTRIBUTING.md) before making an pull requests.

## Credits
Many great online examples and tutorials were used to create this repository.

I also watched several Pluralsight.com courses on the Decorator Pattern.

## License
MIT © [Mark Hazleton](https://markhazleton.controlorigins.com)
