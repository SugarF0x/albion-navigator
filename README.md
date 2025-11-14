![alt text](/preview.jpg)

# Albion Navigator

**Albion Navigator** (**AN**) is a _Roads of Avalon_ cartography management tool.
It helps in taking notes on portals as well as in managing them.

## Installation

Head over to [Releases](https://github.com/SugarF0x/albion-navigator/releases) and get the latest
`AN_{version}.7z` from there. Unpack it into any directory and run `Albion Navigator.exe`.

The government does not want you to know this but the source code is free and you can build
the executable yourself.

## Content

Planned features:

- [ ] On-map zone interaction menu
- [ ] Zone favorites
- [ ] Shorter path found alert
- [ ] Map data sharing
- [ ] Manual link management

### Portal Registration

_Currently only screen capture is supported, manual link registration will be added later on.
Screen capture is supported on windows only. I probably will not bother with other platforms
but feel free to contribute._

When the app is running, pressing **Ctrl + S** takes a screenshot of the primary monitor.
If you do so when hovering over the portal icon in Local Map mode, **AN** will parse the
image for current zone name, portal name and its expiration timer.

**AN** uses Tesseract for OCR which is not always on-point, especially considering it is using
the default _traineddata_ - not whatever fonts Albion uses. In case a portal timer is misread
you can refire the screen capture to override the timer until it gets it right.
The registered portal timer can be seen in the LogBox menu at the bottom.

Manual link removal is not currently supported, but it is planned.

This feature is resolution-agnostic, however it has only been tested in FullHD and 4K.
Game language should also be irrelevant, however it has only been tested in English and Russian.

Taking screenshots is **not agains ToS**
and what we do with these screenshots afterwards is none of SBI concern.

### Navigation

Navigation tab is where you go to find shortest paths from point A to point B, which includes both
continents and roads. Given enough roads data, it can highlight a shortcut through the portals.

If you are looking for the shortest path to **any** mainland exit from roads or want to see all
shortest paths from any black zone or road to the Royal Continent - you can use the
**Find All Paths Out** control box.

If you want to see all the portals interconected by an unbroken chain 
(e.g. you can get from one portal to another without leaving roads)
you can use the **Connected Portals** control box.

Any paths found here can be copied to clipboard in a ready-to-paste to a Discord server.
The copy shall have the ordered list of zones with an icon for each zone type.
If the path includes a portal connection - the earliest portal expiration shall also be included
in a discord countdown format
(the one that tells users at what time something happens regardless of their timezone)

### Zone Info

Adding a new link or searching for a path in the [Navigation](#navigation) tab highlights it on map.
All zones within this highlight at any time are displayed in the [Zone Info](#zone-info) tab.

The list specifies all the zones and shows any specific info on the zone like what resources it has
or what kind of chests you can find in the given road.

Currently only Road zones have any metadata associated with them. Expanding this to include
on-land zones too is planned.

### Settings

Here you can customize several **AN** features.

#### Volume

Some actions like adding/removing portal connections have associated sound-effects to them.
You can disable these in case you get annoyed by it. It is, however, useful when you dont have
a second monitor to see the update status and need some que to know how image regocnition went.

#### Icon Map

Here you can specify what icons you want to be copied into your clipboard when
copying navigation path. Useful if you dont want to have any icons associated with
zone types or when your discord server has some custom icons to represent zone types.
