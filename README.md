# LFG Synchronization

This is a program that simulates a server that hosts raid parties. The amount of raid parties that can run in together are dependent on the server, and players will wait until a slot opens. 


# Input
When ran, the program will require user input for setting the hypothetical server's capabilities, amount of players, and their play time (to some extent).

### Max Instances
Max amount of active dungeons or running instances.
Range is `1` to `5000`

### DPS Count
Amount of DPS players in queue.
Range is `1` to `2147483647`

### Healer Count
Amount of Healers in queue.
 Range is `1` to `2147483647`


### Tank Count
Amount of Tank players in queue.
 Range is `1` to `2147483647`
 
### Minimum Time
Minimum time that the players need to complete a dungeon.
Range is `0` to `15`

### Maximum Time
Minimum time that the players need to complete a dungeon.
Range is `minTime` to `15`


### Log Medium
Medium where logs will be printed.
`0` for the console (recommended for few `instances`)
`1` for writing to a file (recommended for many `instances`)

### Verbose Logging
Whether to log all events or only the final event
`0` only log the summary
`1` log all events

## Input Validation
Input not meeting the required range will ask the user to input again. Will not stop unless the program is terminated or a valid input is entered.

# Building the Program
1. Import into Visual Studio and select the project solution file `P2.sln`
2. Build the program.
3. Depending on whether you've built it on Release or Debug, the executable will be located in the following folder
	`.\bin\<Release/Debug>\net8.0\`
4. Run the executable.
5. The log will be located in the same folder as the executable when ran.
# Additional Notes
- Instances will run an async function when assigned a party, this function is threaded via the Task class of C#.
- Semaphores are used to limit the access to a shared resource (i.e. Dictionary of Instance Ids and their respective statuses)
- Party assignment is decided using Round Robin to prevent starvation.
- For 0 seconds time and/or many instances, logging performance depends on your CPU and SSD performance.
	- In addition to this, outputs will be slower than the simulation (i.e. logs will be outputted/written way after the simulation has finished)
- `End of Program` means that it is safe to terminate the program.



Credits
--
Author: [SealTheTiel](https://github.com/SealTheTiel)

Repository: [Link](https://github.com/SealTheTiel/P2-LFG-Synchronization)
