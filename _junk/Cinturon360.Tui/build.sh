#!/bin/bash

dotnet publish `
  -c Release `
  -r win-x64 `
  -o .\bin\win-x64 `
  -p:SelfContained=true `
  -p:PublishSingleFile=true `
  -p:PublishTrimmed=false
