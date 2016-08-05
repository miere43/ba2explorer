* Iterate over all TODO comments in code.
* Create documentation for all public methods.

## Main window
* Open more than one archive.
* "Archive" menu:
	* "Find" - find file

## File List View
* make export methods work again
* make double click on "Labels" work
* update style, add icons, use Windows Explorer as reference.
* move to separate control
* maybe change ListView to Table to show additional info.
* optimize stuff, reduce allocations
	* ArchiveFilePathService.GetRoots() => array Split()
		* for step 1 can replace Split() to push stuff into existing array, so no new array allocations, but still having splitted strings allocs
		* if still slow, use indices, split only required string that will be displayed in UI.
	* ArchiveFilePathService.GetRoots() => list
		* use existing list, don't allocate new one each GetRoots() call.
	* somehow cache already calculated stuff
	* precache large archives (ex. Fallout4 - MeshesExtra with 40000 paths)
		* generate index file

### File preview control
* Fix FastTextBlock control.