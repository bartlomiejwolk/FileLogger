README
======

FileLogger is a debug tool for Unity.

Licensed under MIT license. See LICENSE file in the project root.

![FileLogger](/Resources/coverart.png?raw=true "FileLogger Coverart")

Features
--------

- Logging to a file and/or Unity console
- Works in edit and in play mode
- Realtime logging
- Logged messages contain additional metadata to help understand what's happening in the code
- Log messages can display GUID of an object from logger method was called
- Ability to customize displayed metadata
- Ability to start/stop logging from inspector and from script
- Multiple logging methods for different purposes
- Ability to enable/disable logging methods from inspector
- Ability to run logger only for specified classes or methods
- Output messages are formated to be easy to read
- Options to customize logger behavior
- Additional window to control logger event when logger game object is not selected
- Quick Start tutorial on youtube.
- [API reference](http://filelogger.airtime-productions.com "Online API").

See [video teaser](https://youtu.be/wS1hQ5641zQ "AnimationPath Animator Unity 5 Extension Teaser ")

Quick Start
------------------

- Clone repository (or extract [zip package](https://github.com/bartlomiejwolk/filelogger/archive/master.zip)) to any location in `Assets` folder.
- Go to _Components -> FileLogger_ to add FileLogger component to a game object.
- Go to _Window -> FileLogger_ to open logger control window.
- In your code, call one of the FileLogger methods, eg. `Logger.LogString("test")`. Remember to include `using FileLogger;` at the top of the file.
- Run your game (by default logger will start logging in play mode).
- Stop play mode and check the _log.txt_ file in your project root folder.

You can also check the _Echo To Console_ checkbox to see logged messages in the Unity console.

Short API Reference
-------------------

- StartLogging()
- StopLogging()
- LogString(**string** format, **params object[]** paramList)
- LogString(**string** format, **object** objectreference, **params object[]** paramList)
- LogCall()
- LogCall(**object** objectReference)
- LogResult(**object** result)
- LogResult(**object** result, **object** objectReference)
- LogStackTrace()
- ClearLogFile()

In the `object objectReference` argument put `this` keyword to have object GUID displayed in the log message.


Help
-----

Just create an issue and I'll do my best to help.

Contributions
------------

Pull requests, ideas, questions and any feedback at all are welcome.

Versioning
----------

Example: `v0.2.3f1`

- `0` Introduces breaking changes.
- `2` Major release. Adds new features.
- `3` Minor release. Bug fixes and refactoring.
- `f1` Quick fix.
