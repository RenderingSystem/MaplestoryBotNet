using MaplestoryRuneSolver.RuneSolving;


new RuneSolvingServerFacade(
    args.Length >= 1 ? args[0] : "",
    args.Length >= 2 ? args[1] : "",
    args.Length >= 3 ? args[2] : "",
    args.Length >= 4 ? args[3] : "",
    args.Length >= 5 ? args[4] : ""
).Launch();
