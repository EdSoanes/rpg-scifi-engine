import styles from "./donut-progress.module.css";

export default function DonutProgress() {
  return (
    <div className="svg-item">
      <svg width="100%" height="100%" viewBox="0 0 40 40" class="donut">
        <circle
          className="donut-hole"
          cx="20"
          cy="20"
          r="15.91549430918954"
          fill="#fff"
        ></circle>
        <circle
          className="donut-ring"
          cx="20"
          cy="20"
          r="15.91549430918954"
          fill="transparent"
          stroke-width="3.5"
        ></circle>
        <circle
          className="donut-segment"
          cx="20"
          cy="20"
          r="15.91549430918954"
          fill="transparent"
          stroke-width="3.5"
          stroke-dasharray="22 78"
          stroke-dashoffset="25"
        ></circle>
        <g className="donut-text">
          <text y="50%" transform="translate(0, 2)">
            <tspan x="50%" text-anchor="middle" class="donut-percent">
              22%
            </tspan>
          </text>
          <text y="60%" transform="translate(0, 2)">
            <tspan x="50%" text-anchor="middle" class="donut-data">
              3450 widgets
            </tspan>
          </text>
        </g>
      </svg>
    </div>
  );
}
