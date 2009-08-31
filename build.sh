#!/bin/bash

gmcs -out:PixClip.exe -pkg:gtk-sharp-2.0 /r:Mono.Cairo.dll -pkg:gnome-sharp-2.0 /noconfig /nologo /warn:4 /optimize- /codepage:utf8 /t:exe *.cs
