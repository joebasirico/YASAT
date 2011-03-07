YASAT
======

YASAT (Yet Another Static Analysis Tool) is a very basic static analysis tool, really it's less about static analysis and more about running a group of regular expressions on a code base and generating a report on the resulting matches. 

I found myself grepping a code base repeatedly and found that I had accumulated a decent set of Regex's that I'd use to point me in the right direction during a code review.

I wrote this tool to make that process less painful.

*Program Flow*

* YASAT loads any it can find in the "Rules" directory and will report the number of rules it found.
* Select any file in the source directory. YASAT will recursively load and match each file to rules
* Click "Scan for Issues" - this step may take a while depending on the number of rules and the number of source files.
* When YASAT has finished click the "Generate Report" button to save an HTML version of the issues discovered


Primary Developer: Joe Basirico(https://github.com/joebasirico)