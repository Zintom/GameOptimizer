## Zintoms Game Optimizer
Boosts gaming performance by limiting the 'priority' of other processes and optionally limiting how many CPU cores they can use (processor 'Affinity').

### Download
To download, go to the [releases](https://github.com/Zintom/GameOptimizer/releases) section, find your platform and download the corresponding *.zip* file; Alternatively go to the [main/Compiled](https://github.com/Zintom/GameOptimizer/tree/main/Compiled) directory and download the zip from there.

### First start
When in the Main Menu, you have two methods of boosting, using the pre-built modes under *'Quick Options'* which I *highly* suggest you use, or, by manually entering a command via the *'Command Input'* option. To view more info on commands see *'Help'* in the Main Menu.

### Must-do's before running
All processes you do *NOT* want affected by the optimizer are kept in a whitelist file called *"process_whitelist.txt"*, which is automatically generated when you first run the app. By default, it contains some processes that I *suggest* you do not remove (i.e Steam), this ensures that your Steam Overlay etc will not lag out when you run the optimizer.
	
To add new apps/games to the white-list; first find the app/game executable name (either through Task Manager or by going to the installation path of the app/game and finding the *.exe* file); Then copy the application name into the *"process_whitelist.txt"* file on a new line **without** the '*.exe*' extension.

If you wish to group different applications in the file together, you can use the comment symbol '**##**' to label groups, the optimizer ignores whatever is written in front of this symbol so you can label sections like '**## Games**' and '**## Apps**' etc.

***If you use communication apps like Discord, Teamspeak or Mumble, I highly suggest you add these to the whitelist file.***

### Last bit
If you find any bugs please report them on the [issues](https://github.com/Zintom/GameOptimizer/issues) page; Likewise if you want to contribute feel free to fork the project and make a pull request with your changes/improvements and I'll take a look.

Show your love and support by scanning the app with your favourite anti-virus and marking it as *safe*, as well as going to [VirusTotal.com](https://www.virustotal.com) to upload/scan it there and leave an upvote showing that this application is safe for future users.

Thanks for using my app 😃
