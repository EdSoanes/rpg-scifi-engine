import { Atom, useAtom } from 'jotai'
import React from 'react'
import { State } from '../../lib/rpg-api/types'
import { Button } from '@chakra-ui/react'
import {
  postSetState,
  graphStateAtom,
  playerCharacterAtom,
} from '../../lib/rpg-api/fetcher'

export declare interface StateButtonProps {
  stateAtom: Atom<State>
}

function StateButton(props: StateButtonProps) {
  const [playerCharacter] = useAtom(playerCharacterAtom)
  const [state] = useAtom(props.stateAtom)
  const [graphState, setGraphState] = useAtom(graphStateAtom)
  const variant = state.isOn ? 'outline' : 'solid'

  const onChangeState = async () => {
    const res = await postSetState(
      playerCharacter!.id,
      state.name,
      !state.isOn,
      graphState!
    )

    setGraphState(res)
  }

  return (
    <Button variant={variant} onClick={onChangeState}>
      {state.name}
    </Button>
  )
}

export default StateButton
