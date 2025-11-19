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
    Converts 8-bit signed values to 16-bit (`-32768..32767`).

Maybe experiment with variable message sizes, reading first the message type and then reading the right number of bits to follow. This could even include the signal sent to connect the controller:
```
io.WriteString(controllerPipe, strconv.Itoa(plrNum))
```
This could instead be sent as a buffer just like input signals, and just only include the player and message type (message type being one indicating that this is just a ping to connect the controller; a value the frontend never sends for message type (or maybe one it does, if the frontend is triggering it anyway!)).
This would also make it so buttons and triggers only need to be 4 bytes and not all 5.

OR, a fixed-byte approach would look like this:
| Byte | Connect Msg | Axis Msg | Button Msg | Comment             |
| ---- | ----------- | -------- | ---------- | ------------------- |
| 0    | PlayerID    | PlayerID | PlayerID   | Who triggered it    |
| 1    | MsgType     | MsgType  | MsgType    | Axis, button, etc   |
| 2    |             | AxisID   | ButtonID   | Which control       |
| 3    |             | X        | Value      | Axis value or 0/1   |
| 4    |             | Y        | -          | Axis value          |
| 5+   | -           | Extra    | -          | Optional future use |

Pros of this would mainly be avoiding extra issues with partial reads (Chat GPT says they could happen, I'm skeptical at this point) and avoiding complexity in general.  
Here are Chat's reasons for including more than 5 bytes (future use):
- Additional axes
- Trigger pressure
- Multi-touch gestures
- Vibration / haptics commands