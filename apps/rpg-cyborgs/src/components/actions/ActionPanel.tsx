import { Atom, useAtom } from 'jotai'
import React from 'react'
import { Action } from '../../lib/rpg-api/types'
import { Box } from '@chakra-ui/react'

import { playerCharacterAtom } from '../atoms/playerCharacter.atom'
import { graphStateAtom } from '../atoms/graphState.atom'

export declare interface ActionPanelProps {
  actionAtom: Atom<Action>
}

function ActionPanel(props: ActionPanelProps) {
  const [playerCharacter] = useAtom(playerCharacterAtom)
  const [action] = useAtom(props.actionAtom)
  const [graphState, setGraphState] = useAtom(graphStateAtom)

  // const onChangeState = async () => {
  //   const res = await postSetState(
  //     playerCharacter!.id,
  //     state.name,
  //     !state.isOn,
  //     graphState!
  //   )

  //   setGraphState(res)
  // }

  return <Box>{action.name}</Box>
}

export default ActionPanel
