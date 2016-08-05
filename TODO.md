* Iterate over all TODO comments in code.
* Create documentation for all public methods.

## Main window
* Open more than one archive.
* "Archive" menu:
	* "Find" - find file

## File List View
* make export methods work again
* update style, add icons, use Windows Explorer as reference.
* maybe change ListView to Table to show additional info.
* Fallout4 - MeshesExtra -> precombined folder takes 7 seconds to open it. fix this somehow, maybe in async way? in parallel?
* optimize stuff, reduce allocations
	* ArchiveFilePathService.GetRoots() => array Split()
		* if slow, use indices, split only required string that will be displayed in UI.
	* ArchiveFilePathService.GetRoots() => list
		* don't allocate levelDirHashes each time
	* somehow cache already calculated stuff
	* precache large archives (ex. Fallout4 - MeshesExtra with 125000 paths)
		* generate index file

### File preview control
* Fix FastTextBlock control.