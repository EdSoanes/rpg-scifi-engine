export default function PolarChart(chartName) {
  const polarCtx = document.getElementById(chartName);

  const polarData = {
    labels: ["Red", "Green", "Yellow", "Grey", "Blue"],
    datasets: [
      {
        label: "My First Dataset",
        data: [12, 34, 16, 44, 3],
        backgroundColor: [
          "rgb(255, 99, 132)",
          "rgb(75, 192, 192)",
          "rgb(255, 205, 86)",
          "rgb(201, 203, 207)",
          "rgb(54, 162, 235)",
        ],
        borderWidth: 0,
      },
    ],
  };

  const polarConfig = {
    type: "polarArea",
    data: polarData,
    options: {},
  };

  new Chart(polarCtx, polarConfig);

  return <canvas id="{chartName}"></canvas>;
}
