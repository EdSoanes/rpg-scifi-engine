import React from 'react'
import { ActionTemplate } from '../../lib/rpg-api/types'
import { Button } from '@chakra-ui/react'
import { ArrowForwardIcon } from '@chakra-ui/icons'

export declare interface ActionButtonProps {
  onActionTemplate: (actionTemplate: ActionTemplate) => void
  actionTemplate: ActionTemplate
}

function ActionButton(props: ActionButtonProps) {
  const {actionTemplate} = props

  return (
    <Button
      leftIcon={<ArrowForwardIcon />}
      variant={'solid'}
      size={'lg'}
      onClick={() => props.onActionTemplate(actionTemplate)}
    >
      <span>
        {actionTemplate.name}
      </span>
      <span>Performable: {String(actionTemplate.isPerformable)}</span>
      
      {actionTemplate.actionArgs.filter((arg) => arg.value).map((arg) => (<span key={arg.name}>{arg.name + ' ' + arg.value}</span>))}
      
    </Button>
  )
}

export default ActionButton
