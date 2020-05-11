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


TODO

Tasks:


Bugs:
