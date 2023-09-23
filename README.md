# Power Diary Interview

## Author: Marcin Kern
### Date: 23.09.2023

## Database migration / creation

### Disclaimer

Bear in mind, that first start will create a database and fill it with data for the _past year_.

The process may take **several minutes**.

### How to shorten first start

If pressed for time, feel free to adjust how far back you want to fill the database.

You can adjust that in `Blazor/Chat.Blazor.Server/Program.cs`, line 48:

`var howFarBack = new TimeSpan(365, 0, 0, 0);`

## How to start

### Using Solution view (e.g. Visual Studio or Rider)
- locate solution folder _Blazor_
- run project Chat.Blazor.Server

### Using Terminal
- `dotnet run --project Blazor/Chat.Blazor/Server`

## Assumptions
- there's only one chat room

## Technical Decisions
- simplistic migration "by hand" to let the DB be reused between app starts