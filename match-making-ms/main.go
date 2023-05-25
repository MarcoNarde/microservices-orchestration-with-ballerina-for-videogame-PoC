package main

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/gorilla/websocket"
)

type Player struct {
	ID       string          `json:"id"`
	Username string          `json:"username"`
	Conn     *websocket.Conn `json:"-"`
}

var players []*Player
var waitingPlayers []*Player

func main() {
	http.HandleFunc("/matchmaking", handleMatchmaking)
	log.Println("Server started. Press any key to stop the server.")
	http.ListenAndServe(":8080", nil)
}

func handleMatchmaking(w http.ResponseWriter, r *http.Request) {
	upgrader := websocket.Upgrader{
		ReadBufferSize:  1024,
		WriteBufferSize: 1024,
	}

	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println(err)
		return
	}

	for {
		var player Player

		err := conn.ReadJSON(&player)
		if err != nil {
			log.Println(err)
			removePlayer(conn)
			return
		}

		player.Conn = conn
		addPlayer(&player)

		for len(waitingPlayers) >= 2 {
			matchPlayers()
		}
	}
}

func addPlayer(player *Player) {
	players = append(players, player)
	waitingPlayers = append(waitingPlayers, player)
}

func removePlayer(conn *websocket.Conn) {
	for i, player := range players {
		if player.Conn == conn {
			players = append(players[:i], players[i+1:]...)
			waitingPlayers = removeWaitingPlayer(waitingPlayers, player)
			break
		}
	}
}

func removeWaitingPlayer(waitingPlayers []*Player, player *Player) []*Player {
	for i, p := range waitingPlayers {
		if p == player {
			waitingPlayers = append(waitingPlayers[:i], waitingPlayers[i+1:]...)
			break
		}
	}
	return waitingPlayers
}

func matchPlayers() {
	var matchedPlayers []*Player
	matchedPlayers = append(matchedPlayers, waitingPlayers[0], waitingPlayers[1])
	waitingPlayers = waitingPlayers[2:]

	response1 := matchedPlayers[1]
	response2 := matchedPlayers[0]

	sendResponse(matchedPlayers[0].Conn, response1)
	sendResponse(matchedPlayers[1].Conn, response2)
}

func sendResponse(conn *websocket.Conn, response *Player) {
	data, err := json.Marshal(response)
	if err != nil {
		log.Println(err)
		return
	}

	err = conn.WriteMessage(websocket.TextMessage, data)
	if err != nil {
		log.Println(err)
		removePlayer(conn)
	}

	conn.Close()
}
