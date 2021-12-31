# Restaurant reservations sample code base

This is the sample code base that accompanies the book *Code That Fits in Your Head.* It pretends to be a REST API that implements a restaurant reservation system.

## Normal development

Normal development tasks should take place in the *Restaurant* Visual Studio solution. This solution only contains production code and (fast) unit tests. Thus, that code base facilitates test-driven development, because the turnaround time for a test run is tolarable, even with Visual Studio's test runner.

## Build

The *Build* Visual Studio solution contains additional tests that run against an automated database. This is also the solution executed by the `build.sh` script.

The integration tests in that solution take longer to run, so this solution shouldn't be used for day-to-day development. On the other hand, it's practical to load the solution if you need to refactor the interface between the code and the database. It's also useful when you need to add new integration tests.

The database integration tests automatically create and tear down a SQL Server database on each test case. You don't have to configure anything, but you must have SQL Server Express installed on your machine.