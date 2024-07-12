import { components } from "./rpgtypes";

export type RpgGraphState = Pick<
  components["schemas"]["Rpg.ModObjects.RpgGraphState"],
  keyof components["schemas"]["Rpg.ModObjects.RpgGraphState"]
>;

export type RpgContent = Pick<
  components["schemas"]["RpgContent"],
  keyof components["schemas"]["RpgContent"]
>;

export type PlayerCharacter = Pick<
  components["schemas"]["Rpg.Cyborgs.PlayerCharacter"],
  keyof components["schemas"]["Rpg.Cyborgs.PlayerCharacter"]
>;
