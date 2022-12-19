# Rover
Hobby project to build an autonomous rover robot. Uses .NET Micro Framework, GHI Electronics ARM microcontroller, XBee wireless RS232 modules. Code for archival purposes only. Project sunset due to hardware obsolescence. 
# Components
TATER: Rover firmware
SOUP: PC control & observation program
# Release Notes
v0.7.0.0 [12-07-12]
General Notes:
	- On-going development beta release
Features Added:
	- Implemented Subversion control (rev 2)
Bugs Fixed:
	- None
Known Issues:
	- Image capture on TATER might throw out of memory error if jpg is larger than available memory.
	- "ACK, Feedback, Done." concept may not be idea. Especially when autonomous mode may need multiple feedback.	
	- Various menu features with no relevance are disabled.
	- Image doesn't zoom or resize with window size change.
	- Rare exception thrown in SOUP during auto image save (related to filename in ticks?).
	- Rare exception thrown in TATER when saving to SD Card.
	- Some minor cosmetic bugs.

v0.5.5.0 [10-12-10]
General Notes:
	- On-going development beta release
Features Added:
	- Updated to NETMF SDK 4.1 (GHI SDK 1.0.8, USBizi 4.1.1.0, Visual C# Express 2010)
Bugs Fixed:
	- None
Known Issues:
	- Image capture on TATER might throw out of memory error if jpg is larger than available memory.
	- "ACK, Feedback, Done." concept may not be idea. Especially when autonomous mode may need multiple feedback.	
	- Various menu features with no relevance are disabled.
	- Image doesn't zoom or resize with window size change.
	- Rare exception thrown in SOUP during auto image save (related to filename in ticks?).
	- Rare exception thrown in TATER when saving to SD Card.
	- Some minor cosmetic bugs.

v0.5.0.0 [5-14-10]
General Notes:
	- On-going development beta release
Features Added:
	- Overhauled and simplified serial communications framework ("ACK, Feedback, Done." concept).
	- Added basic code to do rover movement, distance detection, etc.
	- added simple Autonomous roving code
Bugs Fixed:
	- Serial commands only have dummy framework in TATER code.
	- Removed unused USB Host assembly to free some memory and make 640x480 image capture more robust.
Known Issues:
	- Image capture on TATER might throw out of memory error if jpg is larger than available memory.
	- "ACK, Feedback, Done." concept may not be idea. Especially when autonomous mode may need multiple feedback.	
	- Various menu features with no relevance are disabled.
	- Image doesn't zoom or resize with window size change.
	- Rare exception thrown in SOUP during auto image save (related to filename in ticks?).
	- Rare exception thrown in TATER when saving to SD Card.
	- Some minor cosmetic bugs.

v0.4.1.0 [5-3-10]
General Notes:
	- On-going development beta release
Features Added:
	- Updated TATER project to support new FEZ Firmware (V 4.0.2.0) and GHI NETMF SDK (1.0.4)
Bugs Fixed:
	- none
Known Issues:
	- same as previous release

v0.4.0.0 [4-19-10]
General Notes:
	- On-going development beta release
Features Added:
	- Added background worker to SOUP to handle serial file download (was hanging the app before).
	- Added status bar to SOUP to contain progress bar and command/rover status text field.
	- Replaced "TST" and "RDY" functions with more common ping function "PNG."
	- Added some miscellaneous error handling and cleaned up some code.
Bugs Fixed:
	- SOUP hangs during image download.
	- Progress bar in SOUP not enabled.
Known Issues:
	- Various menu features with no relevance are disabled.
	- Image doesn't zoom or resize with window size change.
	- Serial commands only have dummy framework in TATER code.
 	- Rare exception thrown in SOUP during auto image save (related to filename in ticks?).
	- Image capture on TATER might throw out of memory error if jpg is larger than available memory.
	- Rare exception thrown in TATER when saving to SD Card.
	- Some minor cosmetic bugs.

v0.3.0.0 [4-7-10]
General Notes:
	- On-going development beta release
Features Added:
	- Completely rebuild SOUP from ground-up. No dependancy on QuickView project anymore.
	- Consolidated both TATER and SOUP projects into one solution for simplicity.
	- Added function to save downloaded image to PC as jpg.
	- Added function to open saved image for display.
	- Copy image to clipboard
	- Added remote reboot support to TATER and SOUP
Bugs Fixed:
	- None
Known Issues:
	- SOUP hangs during image download.
	- Progress bar in SOUP not enabled.
	- Various menu features with no relevance are disabled.
	- Image doesn't zoom or resize with window size change.
	- Serial commands only have dummy framework in TATER code.
	- Numerous other minor issues.

v0.2.0.0 [4-5-10]
General Notes:
	- On-going development beta release
Features Added:
	- Cleaned code a bit
	- Began support for remote control by sending serial commands
Bugs Fixed:
	- Reduced package size in camera driver from 512 to 256. Seems like image grabbing is more stable now.
	- "String input format error" fixed in SOUP by doing a Readline(string.trim()) instead of read byte by byte
Known Issues:
	- SOUP hangs during image download.
	- Image doesn't zoom or resize with window size change.
	- Serial commands only have dummy framework in TATER code.
	- Numerous other minor issues.

v0.1.0.0 [4-2-10]
General Notes:
	- Initial release
Features Added:
	- Download of images from Rover to PC
Bugs Fixed:
	- N/A
Known Issues:
	- Too many to list :)
