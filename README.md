README
======

FileLogger is a logging tool for Unity.

Licensed under MIT license. See LICENSE file in the project root.

![FileLogger](/Resources/coverart.png?raw=true)

Features
--------

- Logging to a file and/or Unity console
- Works in edit and in play mode
- Realtime logging
- Logged messages contain additional metadata to help understand what's happening in the code
- Log messages can display GUID of an object from which logger method was called
- Ability to customize displayed metadata
- Ability to start/stop logging from inspector and from script
- Ability to run logger only for specified classes or methods
- Output messages are formated to be easy to read
- Options to customize logger behavior
- Additional window to control logger even when logger game object is not selected

[Feature Overview](http://bartlomiejwolk.github.io/filelogger/ "FileLogger Feature Overview")   
[API reference](../../wiki/API-Reference "FileLogger API Reference")

Quick Start
------------------

- Clone repository (or extract [zip package](https://github.com/bartlomiejwolk/filelogger/archive/master.zip)) to any location in `Assets` folder.
- In your code, call one of the FileLogger [methods](../../wiki/API-Reference ), eg. `Logger.LogString("test")`. Remember to include `#define DEBUG_LOGGER` and `using FileLogger;` at the top of the source code file.
- Run your game (by default logger will start logging in play mode).
- Exit play mode and check the _log.txt_ file in your project root folder.

See more detailed [QuickStart Tutorial](../../wiki/QuickStart Tutorial) in the project wiki.

Help
-----

Just create an issue and I'll do my best to help.

Contributions
------------

Pull requests, ideas, questions or any feedback at all are welcome.

Versioning
----------

Example: `v0.2.3f1`

- `0` Introduces breaking changes.
- `2` Major release. Adds new features.
- `3` Minor release. Bug fixes and refactoring.
- `f1` Quick fix.
