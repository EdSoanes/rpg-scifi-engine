//The same steps as the browser spike, run on the desktop, to compare timings
var runner = new Rpg.BrowserSpike.SpikeRunner();
runner.Run();

foreach (var step in runner.Steps)
    Console.WriteLine($"{(step.Ok ? "ok    " : "FAILED")} {step.Ms,9:0.0} ms  {step.Name}: {step.Detail}");

Console.WriteLine($"Total {runner.Steps.Sum(x => x.Ms):0} ms. {(runner.AllOk ? "ALL STEPS PASSED" : "SOME STEPS FAILED")}");
