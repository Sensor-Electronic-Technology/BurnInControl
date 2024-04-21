#!/bin/sh
apk add gcompat
apk add curl
curl -fsSL https://raw.githubusercontent.com/arduino/arduino-cli/master/install.sh | sh
dotnet /app/BurnInControl.StationService.dll