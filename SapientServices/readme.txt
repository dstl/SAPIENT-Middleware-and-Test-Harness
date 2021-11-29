Readme File for Sapient Comms Library
=====================================

This is a c# / .NET 4 library for wrapping
the client server interactions on the Sapient system

30/07/14 -rejigged to make it easier to replay to messages by including SendMesage in IConnection interface
31/07/14 - bug fix in reported size of received messages
21/09/15 - final fields for SAPIENT Phase 2
13/06/16 - additional HL alert fields for SAPIENT Phase 4
01/09/16 - v2.3.3 - added 'Follow Track' request
26/09/16 - v2.3.4 - added start/Stop/Play Video request
11/06/17 - v2.3.5 - added additional database and ground truth table for SAPIENT cUAS
27/06/17 - v2.3.5.1 - bug fix in ground truth table - detection_id
27/11/17 - v2.3.6 - Fixed Regions and Add/Delete Regions from Map parsed via HDA, Added Region trigger builder
05/03/19 - v2.3.7 - bug fix in dbAlert - allow alert messages when not connected to database
08/03/19 - v2.3.8 - more friendly error reporting of database and client socket errors
12/01/20 - v2.5.1 - rebuilt for DSTL SAPIENT Workshop release
03/05/20 - v2.6.1 - refactored Middleware code and added automated testing to test harness
31/07/20 - v2.6.4 - Zodiac Pre-Alpha Virtual Integration
31/03/21 - v2.7.4 - Zodiac Pre-Alpha Demonstration
