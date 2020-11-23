# HwSerialSniffer_RS232

This CLI application (and some HW, see bellow) used for connect in the "middle" of RS232 cable and "sniff" data that travel on this cable. Currently this application support only fix size data packets.
Application required for work some HW features. It easy see bellow.

## HW preparation 

For work with application required 2 RS232-to-usb adapters (or one with 2 or more ports). I use and test it with STLAB U-400. And you need apply some changes in your RS232 target that you want to listen on. 

Connect RX of main cable to RX of sniffer port A and connect TX of main cable to RX of sniffer port A. Connect GND of main cable to both sniffers ports. You don't need to cut main cable, just connect to them in parallelly.

## Application configuration

Currently for configurate message size(for threshold) you need to recompile code(VS2019 C#). Change value of i32MessageSize variable to your relevant byte count. 

## Application start

Command for start application is : SerialSniffer.exe Sniffer_Port_A* Sniffer_Port_B* Baudrate

Sniffer_Port_X* - Start as regular serial port name in device manager COMX.  Where X is serial port number



## Output

After you start application and start to receive data you will see next information:

**Diff :** "Diff between this message and previous received message(Only on RX) in ms" **T:** "Time tag in ms" **[RX/TX]:** "Data, each byte separate with **,** delimiter"

Same line written to log file (in the same location where is executable). Name of file ComLog.txt.