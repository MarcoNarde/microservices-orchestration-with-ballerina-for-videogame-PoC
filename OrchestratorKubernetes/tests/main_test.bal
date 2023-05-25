import ballerina/test;
import ballerina/http;
import ballerina/random;

http:Client testClient = check new ("http://localhost:9092/orchestrator/find");

@test:Config{}
public function testDefaultService() returns error? {
  http:Response response = check testClient->get("");
  test:assertEquals(response.statusCode, http:STATUS_NOT_FOUND);
}


@test:Config {}
public function testGetOpponent() returns error? {
    http:Client matchmakingEndpoint = test:mock(http:Client);

    test:prepare(matchmakingEndpoint).when("get").thenReturn(getMockResponseMatchmaking());

    http:Response result = check matchmakingEndpoint->get("");
    json payload = check result.getJsonPayload();
    Player opponenet = {id: "127.0.0.1", username: "TestPlayer"};
    test:assertEquals(payload, opponenet);    
}

function getMockResponseMatchmaking() returns http:Response {
    http:Response mockResponse = new;
    Player opponenet = {id: "127.0.0.1", username: "TestPlayer"};
    mockResponse.setPayload(opponenet);
    return mockResponse;
}

@test:Config {}
public function testGetMatch() returns error? {
    http:Client matchInfoEndpoint = test:mock(http:Client);

    Player player1 = {id: "127.0.0.1", username: "TestPlayer1"};
    Player player2 = {id: "127.0.0.1", username: "TestPlayer2"};

    test:prepare(matchInfoEndpoint).when("get").thenReturn(getMockResponseMatchInfo(player1, player2));

    http:Response result = check matchInfoEndpoint->get("");
    json payload = check result.getJsonPayload();
    Match myMatch = check payload.cloneWithType();

    Match matchInfo = {playerO: player1, playerX: player2, starter: player1, matchId: "12345"};

    // Assert the result is of type `Match`
    test:assertTrue(myMatch is Match, "Expected a Match record");

    // Assert the matchId is not empty
    test:assertTrue(matchInfo.matchId != "", "Match ID should not be empty");

    // Assert the starter is either playerX or playerO
    test:assertTrue(matchInfo.starter == player1 || matchInfo.starter == player2,
            "Starter should be either playerX or playerO");

    // Assert the returned value equals to expected value
    test:assertEquals(payload, matchInfo);    
}

function testFindOpponent() returns error?{
    http:Client findOpponentClient = test:mock(http:Client);

    Player player = {id: "123", username: "alice"};

    test:prepare(findOpponentClient).when("get").thenReturn(getMockResponseOpponent(player));

    http:Response result = check findOpponentClient->get("");
    json payload = check result.getJsonPayload();

    // Assert the result is of type `Player`
    test:assertTrue(payload.ensureType(Player) is Player, "Expected a Player record");

    // Assert the opponent's username is not empty
    test:assertTrue(payload.username != "", "Opponent's username should not be empty");

    // Assert the opponent's ID is not empty
    test:assertTrue(payload.id != "", "Opponent's ID should not be empty");
}

function getMockResponseOpponent(Player p1) returns http:Response|error {
    http:Response mockResponse = new;

    // Genera un numero casuale compreso tra un valore minimo (incluso) e un valore massimo (escluso)
    int randomInteger = check random:createIntInRange(1, 100);

    Player opponent = {id: randomInteger.toString(), username: "username" + randomInteger.toString()};
    mockResponse.setPayload(opponent.toJson());
    return mockResponse;
}

function getMockResponseMatchInfo(Player p1, Player p2) returns http:Response {
    http:Response mockResponse = new;
    Match matchInfo = {playerO: p1, playerX: p2, starter: p1, matchId: "12345"};
    mockResponse.setPayload(matchInfo.toJson());
    return mockResponse;
}