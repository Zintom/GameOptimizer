## Zintoms Game Optimizer
Boosts gaming performance by limiting the 'priority' of other processes and optionally limiting how many CPU cores they have access to (processor 'Affinity').

## Download
To download, go to the GitHub releases section and download the *"Zintoms.Game.Optimizer.1.\*.\*.zip"* file;
Alternatively go to the `main\Compiled` directory and download the zip from there.

### Usage
When on the Main Menu, you have two methods of boosting, using the pre-built modes under *'Quick Options'***(highly suggested)**, or by manually entering a command via the *'Command Input'* option. To view more info on commands see *'help'* in the Main Menu.
	
All processes you do *NOT* want affected by the optimizer are kept in a whitelist file called *"process_whitelist.txt"*, which is automatically generated when you first run the app. By default, it contains some of the processes that I *suggest you do not remove* (i.e Steam), this ensures that your Steam Overlay etc will not lag out when you run the optimizer.
	
To add new apps/games to the program; first find the app/game executable name (either through Task Manager or by going to the installation path of the app/game and finding the *.exe* file); Copy the application name into the *"process_whitelist.txt"* file on a new line **without** the '*.exe*' extension. If you wish to group different applications in the file together, you can use the comment symbol *'##'* to label sections, the optimizer ignores whatever is written in front of this symbol so you can label sections like '## Games' and '## Apps' etc etc.

***If you use communication apps like Discord, Teamspeak or Mumble, I highly suggest you add these to the whitelist file.***
