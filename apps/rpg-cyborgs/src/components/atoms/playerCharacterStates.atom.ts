import { atom } from 'jotai'
import { State } from '../../lib/rpg-api/types'
import { playerCharacterAtom } from './playerCharacter.atom'

export const playerCharacterStatesAtom = atom<State[]>((get) => {
  const dict = get(playerCharacterAtom)?.states ?? {}
  return Object.entries(dict).map((pair) => pair[1] as State)
})
