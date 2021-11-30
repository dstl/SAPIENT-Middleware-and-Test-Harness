# SAPIENT Middleware and Test Harness
A repository for the latest version of the SAPIENT Middleware and Test Harness.

## Dependencies
Prior to compiling and running any of the components of the SAPIENT Middleware and Test Harness, the following operating system, applications and utilities must be installed.

| Dependency | Version (Latest Tested) |
|------------|-------------------------|
| [Windows 10](https://www.microsoft.com/en-gb/software-download/windows10) | 20H2, Build 19042.1110 |
| [Microsoft Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) | 16.10.4 |
| [Microsoft .NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework) | 4.8.04084 |
| [PostgreSQL](https://www.postgresql.org/download/) | 12.6.1 |
| [pgAdmin 4](https://www.pgadmin.org/download/) | 5.0 |

## Incompatibilities
Version 2.7.4 of the SAPIENT Middleware and Test Harness is incompatible with a default installation of PostgreSQL 13. For the SAPIENT Middleware and Test Harness to initialise a database, create the associated tables and connect to the database, PostgreSQL 12 required.

## License
Except where noted otherwise, the SAPIENT Middleware and Test Harness software is licensed under the Apache License, Version 2.0. Please see [License](LICENSE.txt) for details.
