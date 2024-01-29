import { Row, Container, Col } from "react-bootstrap";
import PolarChart from "./components/polar-chart/polar-chart";
import BarChart from "./components/bar-chart/bar-chart";
import DonutProgress from "./components/donut-progress/donut-progress";

export default function MyApp() {
  return (
    <Container>
      <Row>
        <Col>
          <PolarChart></PolarChart>
        </Col>
        <Col>
          <BarChart></BarChart>
        </Col>
        <Col>
          <DonutProgress></DonutProgress>
        </Col>
      </Row>
    </Container>
  );
}
