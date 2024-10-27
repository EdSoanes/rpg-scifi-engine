import React from 'react'
import { Action } from '../../lib/rpg-api/types'
import { Button } from '@chakra-ui/react'
import { ArrowForwardIcon } from '@chakra-ui/icons'

export declare interface ActionButtonProps {
  onAction: (action: Action) => void
  action: Action
}

function ActionButton(props: ActionButtonProps) {

  return (
    <Button
      leftIcon={<ArrowForwardIcon />}
      variant={'solid'}
      size={'lg'}
      onClick={() => props.onAction(props.action)}
    >
      {props.action.name}
    </Button>
  )
}

export default ActionButton
