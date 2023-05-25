import ballerina/websocket;
import ballerina/http;
import ballerina/io;


// Player definition
type Player record {|
    string id;
    string username;
|};

// Lobby definition for 2 players
type Lobby record{|
    Player opponent;
    Player you;
|};

// Match definition with match id
type Match record{|
    Player playerX;
    Player playerO;
    Player starter;
    string matchId;
|};

// Move definition for a play of the game
type Move record{|
    Player p;
    int pos;
    string gameKey;
|};

// Initial Message to sincronize the client with orch.
type InitialMessage record{
    string matchId;
    string username;
};

// Auxiliary for the match
type GameStartConf record{
    string gameKey;
    Player startingPlayer;
};

// Data Structure To track games and clients
map<map<websocket:Caller>> matchesMap = {};

// EndPoint for the find game service
listener http:Listener findGameEP = new(9092);

service /orchestrator/find on findGameEP {
    isolated resource function get getmatch/[string username](http:Caller caller) returns error? {
        // Player Information
        Player player = {id: caller.localAddress.ip, username: username};

        // Call the FindOpponent function to get the opponent
        io:println("Finding opponent for player " + username);
        Player opponent = check FindOpponent(caller, player);
        // Call the GetStarter function to start the game server and decide which player starts
        io:println("Getting match information for player " + username + " - " + opponent.username);
        var matchInfo = check GetMatchInfo(player, opponent);

        // Build the response
        var response = new http:Response();
        response.setPayload(matchInfo.toJsonString());
        check caller->respond(response);

        io:println("Reply to player: " + username);
    }
}

isolated function GetMatchInfo(Player player, Player opponent) returns Match|error {
    // Create client HTTP with matchmaking server
    http:LoadBalanceClient findstarterClient = check new ({
        timeout: 200,
        targets: [
            {url: "http://game-server-ct:9090"}
        ],
        retryConfig: {
            // The initial retry interval in seconds.
            interval: 3,

            // The number of retry attempts before stopping.
            count: 3,

            // The multiplier of the retry interval exponentially increases the retry interval.
            backOffFactor: 2.0,

            // The upper limit of the retry interval is in seconds. If the `interval` into the `backOffFactor`
            // value exceeded the `maxWaitInterval` interval value, `maxWaitInterval` is considered as the retry interval.
            maxWaitInterval: 20
        },
        circuitBreaker: {
            // The failure calculation window measures how long the circuit breaker keeps the
            // statistics for the operations.
            rollingWindow: {

                // The period is in seconds for which the failure threshold is calculated.
                timeWindow: 10,

                // The granularity (in seconds) at which the time window slides.
                // The rolling window is divided into buckets and slides by these increments.
                bucketSize: 2,

                // The minimum number of requests in the rolling window that trips the circuit.
                requestVolumeThreshold: 0

            },
            // The threshold for request failures. When this threshold exceeds, the circuit trips.
            // This is the ratio between failures and total requests. The ratio is calculated using
            // the requests received within the given rolling window.
            failureThreshold: 0.2,

            // The period (in seconds) to wait before attempting to make another request to the upstream service.
            resetTime: 10,

            // HTTP response status codes that are considered as failures
            statusCodes: [400, 404, 500]

        }
    });

    // Build lobby information
    Lobby lobby = {
        opponent: opponent,
        you: player
    };

    http:Request request = new;
    request.method = http:POST;
    request.setPayload(lobby.toJson());
    http:Response playerStart = checkpanic findstarterClient->post("/findstarter", request);
    json resp = check playerStart.getJsonPayload();
    io:println("Match info: " + resp.toString());
    Match gameConf = check resp.cloneWithType();
    return gameConf;
}

isolated function FindOpponent(http:Caller caller, Player player) returns Player|error {
    // Create client websocket with matchmaking server
    websocket:Client matchmakingClient = check new ("ws://matchmaking-server:8080/matchmaking",{
        handShakeTimeout: 350,
        // Set the maximum retry count to 5 so that it will try 5 times with the interval of
        // 5 second in between the retry attempts.
        retryConfig: {
            maxCount: 5,
            interval: 5
        }
    });
    
    check matchmakingClient->writeMessage(player.toJson());

    // Lettura dei messaggi dal server
    string playerResponse = "";
    while (true) {
        string|error message = matchmakingClient->readMessage();
        if (message is websocket:ConnectionClosureError) {
            io:println("Connection closed for: " + player.toString());
            break;
        }else if(message is error){
            io:println("Error: " + message.toString());
            break;
        }else{
            io:println("Response: " + message + "FOR: " + player.username);
            playerResponse = message;
            break;
        }
    }

    string rawData = playerResponse;
    // Get the `json` value from the string.
    json j = check rawData.fromJsonString();
    Player opponent = check j.cloneWithType();
    return opponent;
}
        

service /orchestrator/game on new websocket:Listener(9095) {
    resource function get .(http:Request req) returns websocket:Service|websocket:UpgradeError {
            return new WsGameManage();
    }
}

service class WsGameManage {
    *websocket:Service;

    remote isolated function onOpen(websocket:Caller caller) returns error? {
        io:println("Opened a WebSocket connection");
    }

    remote function onTextMessage(websocket:Caller caller, string text) returns error? {
        io:println("Received msg: " + text);

        json j = check text.fromJsonString();
        InitialMessage | error im = j.cloneWithType();
        
        if(im is InitialMessage){
            caller.setAttribute("username", im.username);
            matchesMap[im.matchId][im.username] = caller;

            io:println("Get Initial Message from player " + im.username); 
            
            // Syncronized the clients
            map<websocket:Caller> gameMap = matchesMap[im.matchId] ?: {};
            if(gameMap.length() == 2){
                foreach var item in gameMap {
                    check item->writeMessage("OK");
                }
            }
        }else{
            Move | error m = j.cloneWithType();
            if(m is Move){
                io:println("Mossa da: " + m.p.username + " in: " + m.pos.toString());

                // Create http client with game server
                http:Client makemoveClient = check new ("game-server-ct:9090");
                http:Request request = new;
                request.method = http:POST;
                request.setPayload(m.toJson());

                http:Response response = checkpanic makemoveClient->post("/addmove", request);
                string resp = check response.getTextPayload();
                io:println("Received msg from game server: " + resp);
                map<websocket:Caller> gameMap = matchesMap[m.gameKey] ?: {};
                if(resp.equalsIgnoreCaseAscii("change turn")){ 
                    foreach var item in gameMap {
                        if(item.isOpen()){
                            if(item.getConnectionId() == caller.getConnectionId()){
                                check item->writeMessage("change turn");
                            }else{
                                check item->writeMessage(m);
                            }
                        }
                    }
                }else{
                    json r = check resp.fromJsonString();
                    Player | error w = r.cloneWithType();

                    if(w is Player){
                        foreach var item in gameMap {
                            if(item.isOpen()){
                                check item->writeMessage("winner-"+w.toString());
                            }
                        }
                    }else{
                        io:println(w);
                    }
                }

            }
        }

    }

    remote function onIdleTimeout(websocket:Caller caller) {
        io:println("Connection timed out");
    }

    remote function onClose(websocket:Caller caller, int statusCode, string reason) {
        io:println(string `Client closed connection with ${statusCode} because of ${reason}`);
        // Remove the element matching the match from the nested map
        foreach var matchKey in matchesMap.keys() {
            map<websocket:Caller> gameMap = matchesMap[matchKey] ?: {};
            foreach var user in gameMap.keys() {
                if (user == caller.getAttribute("username")) {
                    var eliminatedMatch = matchesMap.remove(matchKey);
                    io:println("Removed match: " + eliminatedMatch.toString());
                    break;
                }
            }
        }
    }
}
