import { ActionTemplate } from '../../lib/rpg-api/types'
import { IconButton } from '@chakra-ui/react'
import { PiArrowRight } from 'react-icons/pi'

export declare interface ActionButtonProps {
  onActionTemplate: (actionTemplate: ActionTemplate) => void
  actionTemplate: ActionTemplate
}

function ActionButton(props: ActionButtonProps) {
  const { actionTemplate } = props

  return (
    <IconButton
      variant={'solid'}
      size={'lg'}
      onClick={() => props.onActionTemplate(actionTemplate)}
    >
      <PiArrowRight />
      <span>{actionTemplate.name}</span>
      <span>Performable: {String(actionTemplate.isPerformable)}</span>

      {actionTemplate.actionArgs
        .filter((arg) => arg.value)
        .map((arg) => (
          <span key={arg.name}>{arg.name + ' ' + String(arg.value)}</span>
        ))}
    </IconButton>
  )
}

export default ActionButton
