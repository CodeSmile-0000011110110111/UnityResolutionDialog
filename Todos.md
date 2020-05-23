Flow:

- Populate dropdown lists
	do not make any changes yet!
- Select current item
	happens initially
	happens after a change was made at the end of a "waitforendofframe" coroutine - for changes that take effect at the end of a frame (ie resolution)
	base this on current settings (ie sometimes a settings gets modified from the desired setting)
	apply changes without raising the OnValueChanges event
- Update disabled status of controls
- OnValueChanged
	apply value
		reset vsync state when quality level changes (override quality level's vsync)
	select current item (possibly next frame)
	update disabled status
	update Hz dropdown (when one of the following changed: resolution, vsync, fullscreen mode)

Side-effects:

Changes to the following items may cause other items to change or become irrelevant.

- Resolution affects:
	Refresh rate
- Refresh rate affects:
	calculated max fps (vsync count)
- Fullscreenmode affects:
	resolution
	refresh rate (disable)
	vsync count (disable)
- Vsync count affects:
	refresh rate (disable)
	calculated max fps (vsync count)
- quality affects:
	reset vsync count to dialog's value as it may be changed by quality level
- display affects:
	resolution
	refresh rate
	fullscreen mode (?)

TODO

Tasks:
- popup funktion (play = ok)
- popup key einstellbar

Bugs:

Idee:
- Tool, das ausgibt welche Resolution was ist (Display, Screen, Window, Desktop, usw.)
