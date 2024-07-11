import { components } from "./rpgtypes";

export type RpgGraphState = Pick<
  components["schemas"]["Rpg.ModObjects.RpgGraphState"],
  keyof components["schemas"]["Rpg.ModObjects.RpgGraphState"]
>;
