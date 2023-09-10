# Wrapper
Cross-platform program written in C# that launches and watches another executable.

Website: https://uwap.org/projects/wrapper

Guides: https://uwap.org/guides/wrapper

## Main features
- Managing the console output of the program as a log file with timestamps and a length limit
- Automatically restarting the program if it exits unexpectedly
- Managing versions (updating on program restart, rolling back a version to the previous one)
- Automatically changing the permissions to make the program to be watched executable before starting (Linux only, Windows still needs testing and doesn't really need it and I don't know about macOS)
- Commands that the watched program can run to manage the wrapper

## Installation
- Download a binary for your system from the releases on GitHub (SC means self-contained, FD means framework-dependent so it needs dotnet installed) or build the code yourself
- Create a folder for the entire application and enter it
- Create a subfolder Live/ and place the program to be watched in there (this is where the current version is always kept)
- Create a subfolder Wrapper/ and place the Wrapper binary in there
- Create a file Wrapper.config according to the guide (see above, at least TargetFile must be set)
- Execute the Wrapper binary in Wrapper/ (maybe create a service for it)
