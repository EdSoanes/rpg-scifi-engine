import React from 'react'
import { ActionTemplate } from '../../lib/rpg-api/types'
import { Button } from '@chakra-ui/react'
import { ArrowForwardIcon } from '@chakra-ui/icons'

export declare interface ActionButtonProps {
  onActionTemplate: (actionTemplate: ActionTemplate) => void
  actionTemplate: ActionTemplate
}

function ActionButton(props: ActionButtonProps) {

  return (
    <Button
      leftIcon={<ArrowForwardIcon />}
      variant={'solid'}
      size={'lg'}
      onClick={() => props.onActionTemplate(props.actionTemplate)}
    >
      {props.actionTemplate.name}
    </Button>
  )
}

export default ActionButton
