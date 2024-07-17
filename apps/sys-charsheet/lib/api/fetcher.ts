import createClient from "openapi-fetch";

import { paths } from "./rpgtypes";
import { PlayerCharacter, RpgGraphState, State, Action } from "./types";
import { atom, useAtom, useAtomValue } from "jotai";
import { selectAtom, splitAtom } from "jotai/utils";

export const playerCharacterAtom = atom<PlayerCharacter | null>((get) => {
  const graphState = get(graphStateAtom)
  return graphState?.entities.find(
    (x) => x.id == graphState.contextId
  ) as PlayerCharacter})

const statesAtom = atom<State[]>((get) => {
  const dict = get(playerCharacterAtom)?.states ?? {}
  return Object.entries(dict).map((pair) => pair[1] as State);
}, (get, set) => {

})

const actionsAtom = atom<Action[]>((get) => {
  const dict = get(playerCharacterAtom)?.actions ?? {}
  const actions = Object.entries(dict).map((pair) => pair[1] as Action);

  return actions
})

export const stateAtomsAtom = splitAtom(statesAtom)
export const actionAtomsAtom = splitAtom(actionsAtom)
export const graphStateAtom = atom<RpgGraphState | null>(null)
export const graphFetchAtom = atom(null, async (get, set, payload) => {
  const graphState = await getGraphState('Benny')
  set(graphStateAtom, graphState)

  var pc = 
  
  set(playerCharacterAtom, pc)
})

const client = createClient<paths>({
  baseUrl: process.env.NEXT_PUBLIC_RPG_API_HOST,
});

const getGraphState = async (id: string): Promise<RpgGraphState | null> => {
  const response = await client.GET("/api/v1/rpg/{system}/{archetype}/{id}", {
    params: {
      path: {
        system: process.env.RPG_SYSTEM ?? "Cyborgs",
        archetype: process.env.RPG_PC_ARCHETYPE ?? "PlayerCharacter",
        id: id,
      },
    },
  });

  console.log("GS", response)
  return !response.error
    ? response.data
    : null
};
