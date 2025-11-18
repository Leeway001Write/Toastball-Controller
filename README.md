# Toastball-Controller
A web app for mobile to remote control a multiplayer game of Toasterball. This was a personal project for me to learn how to use tools like http, web scrapers, pipe servers, etc.

## To-do
1. Test with 4 players (so far only 2)
2. Make P1 keyboard input to reduce strain from virtual controllers
3. Make a remote control for the menu (pre-round, post-round, paused round, etc.)

# Documentation

## Input message lifecycle

#### Axis Message (5 bytes)
| Byte | Field       | Type                        | Notes |
| ---- | ----------- | --------------------------- | ----- |
| 0    | Player      | uint8                       | 0x00 in WebSocket Message, because server tracks player numbers per host
| 1    | MessageType | uint8 (0x01)                |
| 2    | AxisID      | uint8                       |
| 3    | X           | int8                        |
| 4    | Y           | int8                        |

#### Button Message (4 bytes)
| Byte | Field       | Type         | Notes |
| ---- | ----------- | ------------ | ----- |
| 0    | Player      | uint8        | 0x00 in WebSocket Message
| 1    | MessageType | uint8 (0x02) |
| 2    | ButtonID    | uint8        |
| 3    | IsPressed   | uint8        |

### Steps (for joystick input)
1. **Frontent JS:**  
    Normalized input from joystick (`-1..1`)  
    converted into 8-bit signed value (`-128..127`)  
    sent as 5-byte buffer via WebSocket to backend.
2. **Backend GOlang** (functionally a router)**:**  
    Receives 5-byte buffer with player number blank.
    Identifies player via host id, then sends buffer with player number to C# controller emulator via Pipe Server.
3. **Backend C#** (controller emulator) **:**  
    Receives 5-byte buffer including player number.

