package main

import (
	"fmt"

	"net/http"
	"github.com/gorilla/websocket"
	//"encoding/binary"
	"github.com/micmonay/keybd_event"
)

var leftKb keybd_event.KeyBonding;
var rightKb keybd_event.KeyBonding;
var backKb keybd_event.KeyBonding;
var middleKb keybd_event.KeyBonding;
var nextKb keybd_event.KeyBonding;

var upgrader = websocket.Upgrader {
	ReadBufferSize: 1024,
	WriteBufferSize: 1024,
	CheckOrigin: func(r *http.Request) bool {
        return true // allow all origins (only for testing!)
    },
}

/**
* Handle WebSocket Requests to press keys
*/
func wsHandler(w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		fmt.Println("Error during upgrade to WebSocket:", err)
		return
	}
	defer conn.Close()

	fmt.Println("WebSocket connected successfully")

	for {
		msgType, msg, err := conn.ReadMessage()
		if err != nil {
			fmt.Println("Error reading message:", err)
		}
		//fmt.Println("Message from client:", msgType, string(msg))

		if msgType == websocket.BinaryMessage && len(msg) >= 6 {
			//phoneId := int(binary.BigEndian.Uint32(msg[:4]))
			side := int(msg[4])
			isPressed := int(msg[5])

			//fmt.Println("phoneId =", phoneId, "side = ", side, "isPresed = ", isPressed)

			updateKeys(side, isPressed);
		}
	}
}

/**
* Launch key presses
*/
func updateKeys(button int, isPressed int) {
	if button == 0 {
		// LEFT
		if isPressed == 1 {
			leftKb.Press()
		} else {
			leftKb.Release()
		}
	} else if button == 1 {
		// RIGHT
		if isPressed == 1 {
			rightKb.Press()
		} else {
			rightKb.Release()
		}
	} else if button == 2 {
		// BACK
		if isPressed == 1 {
			backKb.Press()
		} else {
			backKb.Release()
		}
	} else if button == 3 {
		// MIDDLE
		if isPressed == 1 {
			middleKb.Press()
		} else {
			middleKb.Release()
		}
	} else {
		// NEXT
		if isPressed == 1 {
			nextKb.Press()
		} else {
			nextKb.Release()
		}
	}
}

func main() {
	var err error;

	// Start key bonds
	leftKb, err = keybd_event.NewKeyBonding()
	if err != nil {
		fmt.Println("Error creating left button key bonds:", err)
	}
	rightKb, err = keybd_event.NewKeyBonding()
	if err != nil {
		fmt.Println("Error creating right button key bonds:", err)
	}
	backKb, err = keybd_event.NewKeyBonding()
	if err != nil {
		fmt.Println("Error creating back button key bonds:", err)
	}
	middleKb, err = keybd_event.NewKeyBonding()
	if err != nil {
		fmt.Println("Error creating middle button key bonds:", err)
	}
	nextKb, err = keybd_event.NewKeyBonding()
	if err != nil {
		fmt.Println("Error creating next button key bonds:", err)
	}

	leftKb.SetKeys(keybd_event.VK_A)
	rightKb.SetKeys(keybd_event.VK_D)
	backKb.SetKeys(keybd_event.VK_ESC)
	middleKb.SetKeys(keybd_event.VK_S)
	nextKb.SetKeys(keybd_event.VK_SPACE)

	// Serve HTML
	http.Handle("/", http.FileServer(http.Dir("static")))

	// Handle websocket
	http.HandleFunc("/ws", wsHandler)

	// Start http
	fmt.Println("Server running at http://172.20.10.2:8080")
	http.ListenAndServe("0.0.0.0:8080", nil)
}