# Microsoft eShopOnWeb ASP.NET Core Reference Application

This Microsoft reference application is now being maintained by [NimblePros](https://nimblepros.com/).

If you're looking for the .NET Aspire eShop sample it's [here](https://github.com/dotnet/eShop). Microsoft also recommends the [Reliable Web App](https://learn.microsoft.com/azure/architecture/web-apps/guides/reliable-web-app/overview) patterns guidance for building web apps with enterprise app patterns.

Sample ASP.NET Core reference application, powered by Microsoft, demonstrating a single-process (monolithic) application architecture and deployment model. If you're new to .NET development, read the [Getting Started for Beginners](https://nimblepros.github.io/eShopOnWeb/getting-started-for-beginners.html) guide.

A list of Frequently Asked Questions about this repository can be found [here](https://github.com/nimblepros/eShopOnWeb/wiki/Frequently-Asked-Questions).

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Overview Video](#overview-video)
- [eBook](#ebook)
- [Topics (eBook TOC)](#topics-ebook-toc)
- [Running the sample locally](#running-the-sample-locally)
  - [Configuring the sample to use SQL Server](#configuring-the-sample-to-use-sql-server)
- [Running the sample using Docker](#running-the-sample-using-docker)
- [Getting the GitHub Single Sign-On Working](#getting-the-github-single-sign-on-working)
- [Community Extensions](#community-extensions)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Overview Video

[Steve "ardalis" Smith](https://twitter.com/ardalis) recorded [a live stream providing an overview of the eShopOnWeb reference app](https://www.youtube.com/watch?v=vRZ8ucGac8M&ab_channel=Ardalis) in October 2020.

## eBook

This reference application is meant to support the free .PDF download ebook: [Architecting Modern Web Applications with ASP.NET Core and Azure](https://aka.ms/webappebook), updated to **ASP.NET Core 8.0**. [Also available in ePub/mobi formats](https://dotnet.microsoft.com/learn/web/aspnet-architecture).

You can also read the book in online pages at the .NET docs here:
https://docs.microsoft.com/dotnet/architecture/modern-web-apps-azure/

[<img src="https://dotnet.microsoft.com/blob-assets/images/e-books/aspnet.png" height="300" />](https://dotnet.microsoft.com/learn/web/aspnet-architecture)

The **eShopOnWeb** sample is related to the [eShopOnContainers](https://github.com/dotnet/eShopOnContainers) sample application which, in that case, focuses on a microservices/containers-based application architecture. However, **eShopOnWeb** is much simpler in regards to its current functionality and focuses on traditional Web Application Development with a single deployment.

The goal for this sample is to demonstrate some of the principles and patterns described in the [eBook](https://aka.ms/webappebook). It is not meant to be an eCommerce reference application, and as such it does not implement many features that would be obvious and/or essential to a real eCommerce application.

> ### VERSIONS
> #### The `main` branch is currently running ASP.NET Core 10.0.
> #### Older versions are tagged.

## Topics (eBook TOC)

- Introduction
- Characteristics of Modern Web Applications
- Choosing Between Traditional Web Apps and SPAs
- Architectural Principles
- Common Web Application Architectures
- Common Client Side Technologies
- Developing ASP.NET Core MVC Apps
- Working with Data in ASP.NET Core Apps
- Testing ASP.NET Core MVC Apps
- Development Process for Azure-Hosted ASP.NET Core Apps
- Azure Hosting Recommendations for ASP.NET Core Web Apps

## Running the sample locally
Most of the site's functionality works with just the web application running. However, the site's Admin page relies on Blazor WebAssembly running in the browser, and it must communicate with the server using the site's PublicApi web application. You'll need to also run this project. You can configure Visual Studio to start multiple projects, or just go to the PublicApi folder in a terminal window and run `dotnet run` from there. After that from the Web folder you should run `dotnet run --launch-profile https`. Now you should be able to browse to `https://localhost:5001/`. The admin part in Blazor is accessible to `https://localhost:5001/admin`

Note that if you use this approach, you'll need to stop the application manually in order to build the solution (otherwise you'll get file locking errors).

After cloning or downloading the sample you must setup your database.
To use the sample with a persistent database, you will need to run its Entity Framework Core migrations before you will be able to run the app.

You can also run the samples in Docker (see below).

### Running the sample with SQLite

For local development (the `Development` environment, used by the `https` launch profile) both applications store data in SQLite database files shared by Web and PublicApi, so data survives restarts without installing a database server. The files live in the `.data` folder at the repository root (configured by the `SqliteCatalogConnection` and `SqliteIdentityConnection` connection strings in `appsettings.json`; relative paths are resolved against each project folder). Other environments (Docker, production) keep using SQL Server.

The database mode is selected with two settings, `UseSqliteDatabase` and `UseOnlyInMemoryDatabase` (`UseSqliteDatabase` takes precedence). You can set them in two ways:

1. **Configuration files** (used by default) – SQLite is enabled for the `Development` environment in both:
   - `src/Web/appsettings.Development.json`
   - `src/PublicApi/appsettings.Development.json`

   ```json
   {
     "UseSqliteDatabase": true
   }
   ```

2. **Environment variables** – override the files without editing them, e.g. to switch to the in-memory database: `UseSqliteDatabase=false UseOnlyInMemoryDatabase=true dotnet run ...` (PowerShell: `$env:UseSqliteDatabase="false"; $env:UseOnlyInMemoryDatabase="true"; dotnet run ...`).

Web creates and seeds the database on first start, so start Web first and PublicApi second (each in its own terminal, from the repository root):

```bash
dotnet run --project src/Web --launch-profile https
dotnet run --project src/PublicApi --launch-profile https
```

Delete the `.data` folder to start again with fresh sample data. Tests always use in-memory databases.

### Configuring the sample to use SQL Server

1. By default, the project uses a real database. If you want an in memory database, you can add in the `appsettings.json` file in the Web folder

    ```json
   {
       "UseOnlyInMemoryDatabase": true
   }
    ```

1. Ensure your connection strings in `appsettings.json` point to a local SQL Server instance.
1. Ensure the tool EF was already installed. You can find some help [here](https://docs.microsoft.com/ef/core/miscellaneous/cli/dotnet)

    ```
    dotnet tool update --global dotnet-ef
    ```

1. Open a command prompt in the Web folder and execute the following commands:

    ```
    dotnet restore
    dotnet tool restore
    dotnet ef database update -c catalogcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj
    dotnet ef database update -c appidentitydbcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj
    ```

    These commands will create two separate databases, one for the store's catalog data and shopping cart information, and one for the app's user credentials and identity data.

1. Run the application.

    The first time you run the application, it will seed both databases with data such that you should see products in the store, and you should be able to log in using the demouser@microsoft.com account.

    Note: If you need to create migrations, you can use these commands:

    ```
    -- create migration (from Web folder CLI)
    dotnet ef migrations add InitialModel --context catalogcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj -o Data/Migrations

    dotnet ef migrations add InitialIdentityModel --context appidentitydbcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj -o Identity/Migrations
    ```

## Running the sample using Docker

You can run the Web sample by running these commands from the root folder (where the .sln file is located):

```
docker-compose build
docker-compose up
```

You should be able to make requests to localhost:5106 for the Web project, and localhost:5200 for the Public API project once these commands complete. If you have any problems, especially with login, try from a new guest or incognito browser instance.

You can also run the applications by using the instructions located in their `Dockerfile` file in the root of each project. Again, run these commands from the root of the solution (where the .sln file is located).

## Getting the GitHub Single Sign-On Working

We include GitHub as our external provider for single sign-on.

To get it running locally, you'll want to register an application in GitHub and store values in user secrets for the client ID and client secret.

We explain the code in detail in our course on [ASP.NET Identity in Action: Implementing Individual Accounts](https://academy.nimblepros.com/p/applying-identity-to-asp-net).

## Community Extensions

We have some great contributions from the community, and while these aren't maintained by Microsoft we still want to highlight them.

[eShopOnWeb VB.NET](https://github.com/VBAndCs/eShopOnWeb_VB.NET) by Mohammad Hamdy Ghanem

[FShopOnWeb](https://github.com/NitroDevs/FShopOnWeb) An F# take on eShopOnWeb by Sean G. Wright and Kyle McMaster
