import { ActionTemplate } from '@lib/rpg-api/types'
import { Grid, GridItem, Heading } from '@chakra-ui/react'
import { PiCheckCircleFill, PiCheckCircleLight } from 'react-icons/pi'
import BoxButton from '@components/ui/box-button'

export declare interface ActionTemplatePanelProps {
  onActionTemplate: (actionTemplate: ActionTemplate) => Promise<void>
  actionTemplate: ActionTemplate
}

function ActionTemplatePanel(props: ActionTemplatePanelProps) {
  const { actionTemplate, onActionTemplate } = props
  return (
    <BoxButton
      width={'100%'}
      state={actionTemplate.isPerformable ? 'on' : 'off'}
      onClick={() => onActionTemplate(actionTemplate)}
    >
      <Grid templateColumns="repeat(6, 1fr)" gap={4} width={'100%'}>
        <GridItem colSpan={1} h="10">
          {actionTemplate.isPerformable ? (
            <PiCheckCircleFill size={'28px'} />
          ) : (
            <PiCheckCircleLight size={'28px'} />
          )}
        </GridItem>
        <GridItem colSpan={5} h="10">
          <Heading as={'h3'} size={'sm'}>
            {actionTemplate.name}
          </Heading>
        </GridItem>
      </Grid>
    </BoxButton>
  )
}

export default ActionTemplatePanel
