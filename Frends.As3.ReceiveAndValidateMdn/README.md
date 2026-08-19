# Frends.As3.ReceiveAndValidateMdn

Task to receive and validate MDN receipts for sent AS3 messages

[![ReceiveAndValidateMdn_build](https://github.com/FrendsPlatform/Frends.As3/actions/workflows/ReceiveAndValidateMdn_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.As3/actions/workflows/ReceiveAndValidateMdn_test_on_main.yml)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.As3/Frends.As3.ReceiveAndValidateMdn|main)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

## Installing

You can install the Task via Frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.As3.git`

### Build the project

`dotnet build`

### Run tests

Run the tests

`dotnet test`

Make sure Docker is running.

### Create a NuGet package

`dotnet pack --configuration Release`

### StyleCop.Analyzers Version
This project uses StyleCop.Analyzers 1.2.0-beta.556, as recommended by the author, to get the latest fixes and improvements not available in the last stable release.
