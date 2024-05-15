1. Compile to standalone executable
2. Run as console app if desired, note that the app will create a database and directory for log files, recommend to run in a separate folder
3. To set up and run as a service using Service Control:
`sc create bgScan start=auto binPath=/path/to/exectuable`
