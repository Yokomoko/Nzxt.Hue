# Nzxt.Hue

A simple program to control the NZXT Hue+ lighting controller.
Because fuck CAM.

Use the core library and Manager class or the launcher;

Eg; `Nzxt.Hue.Controller.exe /r <red> /g <green> /b <blue> [/on | /off] [/channel 1|2]`

* red/green/blue are RGB values for the lighting color (all three must be specified, /r 255 /g 0 /b 0 is red).
* on or off enables or disables the lighting on the hub itself. This is just a white LED.
* channel sets lighting for that channel only


All options are optional.
