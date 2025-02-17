![alt text](/preview.jpg)

# Albion Navigator

**Albion Navigator** (**AN**) is a _Roads of Avalon_ cartography management tool.
It helps in taking notes on portals as well as in managing them.

## Main

_TBA_

## Cartography

Cartography tab is where all the mapping takes place. New portals can be registered here.

### Portal Registrations

Start typing cluster or road name in either of the text fields. Auto-complete will suggest
an appropriate name. Invalid names disallow registration.

You can cycle between suggested auto-completions by pressing **Up** and **Down** arrows.
You don't need to fill the name out completely - the name suggested by auto-complete will be used.

Specifying expiration time marks this portal connection for removal in given time from the moment
of registration. Thus, registering a portal with 10 minutes specified will have it removed in 10
minutes.

If a portal is being added from a screen capture, then the time difference between what has been
entered and when the capture has been taken is automatically accounted for (see [Screen Captures](#Screen-Captures))

### Screen Captures

_Currently supported on windows only. I probably will not bother with other platforms but feel free to contribute._

When the app is running, pressing **Ctrl + S** takes a screenshot of the primary monitor.
This can be used to take quick snaps of portals and their timers to write the findings down later
when back in a safe environment. 

Snaps also provide timestamp on when it was taken. This data is automatically accounted for when
registering a portal connection from a snap. Thus, entering a portal of 9 hours about 10 minutes
after having taken a snap will register it as a portal of 8 hours 50 minutes respectively.

## Navigation

Navigation tab is where you go to find shortest paths from point A to point B, which includes both
continents and roads. Given enough roads data, it can highlight a shortcut through the portals.

### Pathfinder

This section allows you to find the shortest path from point A to point B. Enter starting zone
and target zone to calculate shortest path with proper formatting to paste to a Discord channel.
This path will also contain timestamp to when the portal shall close.

### Way out finder

This section allows you to find all possible paths leading out of a road to either of the continents.
You can cycle through the possible paths via the control buttons.

Select the road you want to find exists from and hit `Find`. You can then copy either individual paths
or all possible ways out. This comes very handy when you are looking for a good way out of a Hideout
to transport goods out to the Royal continent.

## Settings

_TBA_
