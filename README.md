# microservices-orchestration-with-ballerina-for-videogame-PoC
![schermata gioco](https://github.com/MarcoNarde/microservices-orchestration-with-ballerina-for-videogame-PoC/assets/56313280/8b3055f8-f9f3-4c79-b876-cd89e8fd153c)

The repository holds a PoC (Proof of Concept) aim to show how a Cloud-Native language, in this case [Ballerina](https://ballerina.io), can be used to help the orchestration of microservices, in the particular case in which this context is applied to the development of online multiplayer video games, mainly focusing on the backend part of the entire application. In the repository you can find:
- The orchestrator implementation with Ballerina, to manage the interaction between the microservices and the game built in Unity. There are two different implementations, one used for local deployment (also with the help of Kubernetes) in the folder *OrchestratorKubernetes* and one adapted for the Cloud in the folder *OrchestratorCloud*.
- The matchmaking service, written in [GO](https://golang.org) that allows two players to be paired with each other. You can find it in the folder *match-making*.
- The service that manages the individual games created, created with [C#](https://learn.microsoft.com/it-it/dotnet/csharp/) within the [.Net](https://learn.microsoft.com/it-it/dotnet/) environment. It allows you to keep track of the games, to make updates to the games in progress thus managing the entire logic of the game. You can find it in the folder *game-server*.
- The implementation of the TicTacToe game, made for the PoC in the folder *TictacToe*.
- The executables of the game i.e. the APK file for Android and the .exe for Windows in the folder *game-executables*.
- Some test files used to test the performance of the orchestrator, located in the folder *test-scripts*.
---
## Setup Local Environment
The game was tested on Windows 10 computers and smartphones running Android 9-10 while the microservices were tested with docker desktop and Kubernetes for Windows and in a virtual machine running Linux. To be able to use the entire application locally, only with docker, just carry out the steps described below:
1. Set up [Docker and Docker Desktop](https://docs.docker.com/get-started/) for you pc.
2. Run ``docker-compose -f ~/Other/docker-compose.yml up -d`` using the file docker-compose.yaml in the *Other* folder.
    1. Alternatively, if you don't want to use the docker images already present in docker.hub, you can build each service image using the dockerfiles contained in each service folder, using the command ``docker build -t image-name .``.
4. Get the executable used for local use, the executable for this are in the folder *game-executables/local*.
5. Run the application in 2 instances and try it.
