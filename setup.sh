#!/bin/bash

# Create solution
dotnet new sln -n FamilyChat

# Create projects
dotnet new classlib -n FamilyChat.Domain
dotnet new classlib -n FamilyChat.Application
dotnet new classlib -n FamilyChat.Infrastructure
dotnet new webapi -n FamilyChat.API
dotnet new xunit -n FamilyChat.Tests

# Add projects to solution
dotnet sln add FamilyChat.Domain/FamilyChat.Domain.csproj
dotnet sln add FamilyChat.Application/FamilyChat.Application.csproj
dotnet sln add FamilyChat.Infrastructure/FamilyChat.Infrastructure.csproj
dotnet sln add FamilyChat.API/FamilyChat.API.csproj
dotnet sln add FamilyChat.Tests/FamilyChat.Tests.csproj

# Add project references
dotnet add FamilyChat.Application reference FamilyChat.Domain
dotnet add FamilyChat.Infrastructure reference FamilyChat.Application
dotnet add FamilyChat.Infrastructure reference FamilyChat.Domain
dotnet add FamilyChat.API reference FamilyChat.Application
dotnet add FamilyChat.API reference FamilyChat.Infrastructure
dotnet add FamilyChat.Tests reference FamilyChat.Domain
dotnet add FamilyChat.Tests reference FamilyChat.Application

echo "Solution and projects created successfully!"
