package main

import (
	// Console output & string manipulation
	"fmt"
	"strconv"
	"strings"

	// Server and WebSocket
	"net"
	"net/http"
	"github.com/gorilla/websocket"

	// QR code
	"github.com/skip2/go-qrcode"

	// Keyboard input
	"github.com/micmonay/keybd_event"

	// Pipe Server for Controller Input
	"github.com/Microsoft/go-winio"
	"io"

	// Custom utilities
	"toasterballController/util"
)

var controllerPipe net.Conn

var players map[string]map[string]*keybd_event.KeyBonding
var playerNumbers map[string]int

var upgrader = websocket.Upgrader {
	ReadBufferSize: 1024,
	WriteBufferSize: 1024,
}

/**
* Handle WebSocket Requests to press buttons
*/
func wsHandler(w http.ResponseWriter, r *http.Request) {
	// Upgrade to WebSocket Connection
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		fmt.Println("Error during upgrade to WebSocket:", err)
		return
	}
	defer conn.Close()

	// Add a player based on the remote host address
	host, _, err := net.SplitHostPort(r.RemoteAddr) // Exclude the port from the host id
	if err != nil {
		fmt.Println("Failed to parse remote address:", err)
	}
	id := host
	plrNum := addPlayer(id)
	fmt.Println("Player", plrNum, "joined:", id)
	conn.WriteMessage(websocket.TextMessage, []byte(strconv.Itoa(plrNum)))

	// Read messages until error indicates connection was closed (player left/closed the web app)
	for {
		msgType, msg, err := conn.ReadMessage()
		if err != nil {
			if (strings.Contains(err.Error(), "wsarecv: An existing connection was forcibly closed by the remote host.") ||
			    strings.Contains(err.Error(), "websocket: close 1006 (abnormal closure): unexpected EOF")) {
				fmt.Println("Player", plrNum, "left")
				unpressButtons(id)
				break
			}
		}

		if msgType == websocket.BinaryMessage && len(msg) >= 2 {
			button := int(msg[0])
			isPressed := int(msg[1])

			updateButtons(id, button, isPressed);
		}
	}
}

/**
* Set up player's id and controller
*
* Returns player number (1 or 2)
*/
func addPlayer(id string) int {
	var plrNum = len(players) + 1

	// Check if player already exists (probably reloaded the page)
	_, ok := players[id]
	if ok {
		plrNum = playerNumbers[id]
		fmt.Println("Player", plrNum, "already exists:", id)
	} else {
		// New player (as long as there are no more than 2)
		if plrNum <= 4 {
			// Connect controller
			io.WriteString(controllerPipe, strconv.Itoa(plrNum))

			// Create Key Bondings
			players[id] = make(map[string]*keybd_event.KeyBonding)
			playerNumbers[id] = plrNum

			/*leftKb, err := keybd_event.NewKeyBonding()
			if err != nil {
				fmt.Println("Error creating left button key bonds:", err)
			}
			rightKb, err := keybd_event.NewKeyBonding()
			if err != nil {
				fmt.Println("Error creating right button key bonds:", err)
			}
			backKb, err := keybd_event.NewKeyBonding()
			if err != nil {
				fmt.Println("Error creating back button key bonds:", err)
			}
			middleKb, err := keybd_event.NewKeyBonding()
			if err != nil {
				fmt.Println("Error creating middle button key bonds:", err)
			}
			nextKb, err := keybd_event.NewKeyBonding()
			if err != nil {
				fmt.Println("Error creating next button key bonds:", err)
			}

			// Bond keys unique to player
			if plrNum == 1 {
				leftKb.SetKeys(keybd_event.VK_A)
				rightKb.SetKeys(keybd_event.VK_D)
				middleKb.SetKeys(keybd_event.VK_S)
				backKb.SetKeys(keybd_event.VK_ESC)
				nextKb.SetKeys(keybd_event.VK_SPACE)
				fmt.Println("P1 keys bound")
			} else {
				leftKb.SetKeys(keybd_event.VK_LMENU)
				rightKb.SetKeys(keybd_event.VK_RMENU)
				middleKb.SetKeys(keybd_event.VK_DOWN)
				backKb.SetKeys(keybd_event.VK_BACKSPACE)
				nextKb.SetKeys(keybd_event.VK_ENTER)
				fmt.Println("P2 keys bound")
			}

			// Save Bondings
			players[id]["left"] = &leftKb;
			players[id]["right"] = &rightKb;
			players[id]["back"] = &backKb;
			players[id]["middle"] = &middleKb;
			players[id]["next"] = &nextKb;*/
		}
	}

	return plrNum
}

/**
* Launch button presses
*/
func updateButtons(plr string, button int, isPressed int) {
	io.WriteString(controllerPipe, strconv.Itoa(playerNumbers[plr]) + strconv.Itoa(button) + strconv.Itoa(isPressed))
}
func unpressButtons(plr string) {
	io.WriteString(controllerPipe, strconv.Itoa(playerNumbers[plr]))
}
func updateButtonsOLDVERSION(plr string, button int, isPressed int) {
	if button == 0 {
		// LEFT
		if isPressed == 1 {
			players[plr]["left"].Press()
		} else {
			players[plr]["left"].Release()
		}
	} else if button == 1 {
		// RIGHT
		if isPressed == 1 {
			players[plr]["right"].Press()
		} else {
			players[plr]["right"].Release()
		}
	} else if button == 2 {
		// BACK
		if isPressed == 1 {
			players[plr]["back"].Press()
		} else {
			players[plr]["back"].Release()
		}
	} else if button == 3 {
		// MIDDLE
		if isPressed == 1 {
			players[plr]["middle"].Press()
		} else {
			players[plr]["middle"].Release()
		}
	} else {
		// NEXT
		if isPressed == 1 {
			players[plr]["next"].Press()
		} else {
			players[plr]["next"].Release()
		}
	}
}

func main() {
	// Connect to Pipe Server for sending Controller Input
	const pipeName = `\\.\pipe\ControllerInputPipe`

	fmt.Println("Connecting to pipe server...")
	var err error
	controllerPipe, err = winio.DialPipe(pipeName, nil)
	if err != nil {
		fmt.Println("Failed to connect to pipe server", err)
		return
	}
	defer controllerPipe.Close()

	fmt.Println("Pipe connected")

	// Initialize players container
	players = make(map[string]map[string]*keybd_event.KeyBonding)
	playerNumbers = make(map[string]int)

	// Serve HTML
	http.Handle("/", http.FileServer(http.Dir("static")))

	// Handle websocket
	http.HandleFunc("/ws", wsHandler)

	// Start http
	addr := "http://" + util.GetWifiIP()
	fmt.Println("Server running at " + addr)

	// Display QR Code
	qr, err := qrcode.New(addr, qrcode.Medium)
	if (err != nil) {
		fmt.Println("Error creating QR code:", err)
	}
	fmt.Println(qr.ToString(false))

	// Serve
	http.ListenAndServe("0.0.0.0:8080", nil)
}