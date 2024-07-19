import { Atom, useAtom } from 'jotai'
import React from 'react'
import { Action } from '../../lib/rpg-api/types'
import { Button } from '@chakra-ui/react'
import { ArrowForwardIcon } from '@chakra-ui/icons'

export declare interface ActionButtonProps {
  onAction: (action: Action) => void
  actionAtom: Atom<Action>
}

function ActionButton(props: ActionButtonProps) {
  const [action] = useAtom(props.actionAtom)

  return (
    <Button
      leftIcon={<ArrowForwardIcon />}
      variant={'solid'}
      size={'lg'}
      onClick={() => props.onAction(action)}
    >
      {action.name}
    </Button>
  )
}

export default ActionButton
