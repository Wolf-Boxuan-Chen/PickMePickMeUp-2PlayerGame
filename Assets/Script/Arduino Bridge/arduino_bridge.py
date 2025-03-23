import serial
import time
import keyboard
import sys

# Determine port based on platform
import platform
if platform.system() == 'Darwin':  # macOS
    default_port = '/dev/tty.usbmodem'  # This is a prefix, may need to be completed
elif platform.system() == 'Linux':
    default_port = '/dev/ttyACM0'
else:  # Windows
    default_port = 'COM3'

# Allow port override via command line
port = sys.argv[1] if len(sys.argv) > 1 else default_port
baud_rate = 9600

print(f"Arduino Button to Keyboard Bridge")
print(f"Attempting to connect to Arduino on {port}...")

# Connect to Arduino
try:
    arduino = serial.Serial(port, baud_rate, timeout=0.1)
    print(f"Connected to Arduino on {port}")
    time.sleep(2)  # Give the connection a moment to establish
except Exception as e:
    print(f"Failed to connect to Arduino: {e}")
    print("Available ports:")
    
    # Try to list available ports
    import serial.tools.list_ports
    ports = list(serial.tools.list_ports.comports())
    for p in ports:
        print(f" - {p}")
        
    print("\nTry specifying the port: python arduino_bridge.py PORT_NAME")
    exit(1)

print("Bridge is running! Press Ctrl+C to exit.")
print("Waiting for Arduino button signals...")

# Main loop
try:
    while True:
        if arduino.in_waiting > 0:
            command = arduino.readline().decode('utf-8').strip()
            print(f"Received: {command}")
            
            if command == "LEFT_DOWN":
                keyboard.press('left')
                print("Pressing LEFT arrow key")
            elif command == "LEFT_UP":
                keyboard.release('left')
                print("Releasing LEFT arrow key")
            elif command == "RIGHT_DOWN":
                keyboard.press('right')
                print("Pressing RIGHT arrow key")
            elif command == "RIGHT_UP":
                keyboard.release('right')
                print("Releasing RIGHT arrow key")
                
        time.sleep(0.01)  # Small delay to reduce CPU usage
        
except KeyboardInterrupt:
    print("\nProgram terminated by user")
finally:
    if 'arduino' in locals():
        arduino.close()
        print("Arduino connection closed")