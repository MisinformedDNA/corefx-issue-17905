## Applications
There are two applications in the solution, DownloadDrop and ReportGenerator.

### DownloadDrop
This application should be run first. This will download all the issues included in the drop.

As the issues are generally put out as a list of files in a GitHub comment, DownloadDrop will go to the issue, locate the comment, locate the list and all files there. Then it will download all those files into a "files" directory.

### ReportGenerator
This application is run second. It accesses all the files downloaded in the previous step and then combines and processes each file. A filter is applied to remove all non-issues and false positives, giving us only one file with all the most likely issues. The file is called `issues.csv`.


If you happen to find other commonalities that can be used to filter the list further, file an issue or a PR.
