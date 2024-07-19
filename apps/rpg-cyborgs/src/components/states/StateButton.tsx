import { Atom, useAtom } from 'jotai'
import React from 'react'
import { State } from '../../lib/rpg-api/types'
import { Button } from '@chakra-ui/react'
import { postSetState } from '../../lib/rpg-api/fetcher'
import { playerCharacterAtom } from '../atoms/playerCharacter.atom'
import { graphStateAtom } from '../atoms/graphState.atom'
import { CheckCircleIcon, SmallCloseIcon } from '@chakra-ui/icons'

export declare interface StateButtonProps {
  stateAtom: Atom<State>
}

function StateButton(props: StateButtonProps) {
  const [playerCharacter] = useAtom(playerCharacterAtom)
  const [state] = useAtom(props.stateAtom)
  const [graphState, setGraphState] = useAtom(graphStateAtom)
  const variant = state.isOn ? 'solid' : 'outline'

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
    <Button
      leftIcon={state.isOn ? <CheckCircleIcon /> : <SmallCloseIcon />}
      variant={variant}
      size={'lg'}
      onClick={onChangeState}
    >
      {state.name}
    </Button>
  )
}

export default StateButton
