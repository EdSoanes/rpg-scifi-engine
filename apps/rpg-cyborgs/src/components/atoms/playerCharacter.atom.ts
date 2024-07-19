import { atom } from 'jotai'
import { PlayerCharacter } from '../../lib/rpg-api/types'
import { graphStateAtom } from './graphState.atom'

export const playerCharacterAtom = atom<PlayerCharacter | null>((get) => {
  const graphState = get(graphStateAtom)
  return graphState?.entities.find(
    (x) => x.id === graphState.contextId
  ) as PlayerCharacter
})
