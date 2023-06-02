# microservices-orchestration-with-ballerina-for-videogame-PoC
![schermata gioco](https://github.com/MarcoNarde/microservices-orchestration-with-ballerina-for-videogame-PoC/assets/56313280/8b3055f8-f9f3-4c79-b876-cd89e8fd153c)

The repository holds a PoC (Proof of Concept) aim to show how a Cloud-Native language, in this case [Ballerina](https://ballerina.io), can be used to help the orchestration of microservices, in the particular case in which this context is applied to the development of online multiplayer video games, mainly focusing on the backend part of the entire application. In the repository you can find:
- The orchestrator implementation with Ballerina, to manage the interaction between the microservices and the game built in Unity. There are two different implementations, one used for local deployment (also with the help of Kubernetes) in the folder *OrchestratorKubernetes* and one adapted for the Cloud in the folder *OrchestratorCloud*.
- The matchmaking service, written in [GO](https://golang.org) that allows two players to be paired with each other. You can find it in the folder *match-making*.
- The service that manages the individual games created, created with [C#](https://learn.microsoft.com/it-it/dotnet/csharp/) within the [.Net](https://learn.microsoft.com/it-it/dotnet/) environment. It allows you to keep track of the games, to make updates to the games in progress thus managing the entire logic of the game. You can find it in the folder *game-server*.
- The implementation of the TicTacToe game, made for the PoC in the folder *TictacToe*.
- The executables of the game i.e. the APK file for Android and the .exe for Windows in the folder *game-executables*.
- Some test files used to test the performance of the orchestrator, located in the folder *test-scripts*.
