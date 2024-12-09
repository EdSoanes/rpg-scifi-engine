import { ActionTemplate } from '@lib/rpg-api/types'
import {
  Grid,
  GridItem,
  Heading,
  HoverCardArrow,
  HoverCardContent,
  HoverCardRoot,
  HoverCardTrigger,
} from '@chakra-ui/react'
import {
  PiCheckCircleFill,
  PiCheckCircleLight,
  PiQuestionLight,
} from 'react-icons/pi'
import BoxButton from '@components/ui/box-button'
import { useState } from 'react'

export declare interface ActionTemplatePanelProps {
  onActionTemplate: (actionTemplate: ActionTemplate) => Promise<void>
  actionTemplate: ActionTemplate
}

function ActionTemplatePanel(props: ActionTemplatePanelProps) {
  const { actionTemplate, onActionTemplate } = props

  const [open] = useState(false)

  return (
    <>
      <BoxButton
        width={'100%'}
        state={actionTemplate.isPerformable ? 'on' : 'off'}
        onClick={() => onActionTemplate(actionTemplate)}
      >
        <Grid templateColumns="repeat(6, 1fr)" gap={4} width={'100%'}>
          <GridItem colSpan={5} h="10">
            <Heading as={'h3'} size={'sm'}>
              {actionTemplate.name}
            </Heading>
          </GridItem>
          <GridItem colStart={6} h="10">
            {actionTemplate.isPerformable ? (
              <PiCheckCircleFill size={'28px'} />
            ) : (
              <PiCheckCircleLight size={'28px'} />
            )}
          </GridItem>
        </Grid>
      </BoxButton>

      <HoverCardRoot size="sm" open={open}>
        <HoverCardTrigger asChild>
          <PiQuestionLight />
        </HoverCardTrigger>

        <HoverCardContent>
          <HoverCardArrow />
          Action description to come...
        </HoverCardContent>
      </HoverCardRoot>
    </>
  )
}

export default ActionTemplatePanel
