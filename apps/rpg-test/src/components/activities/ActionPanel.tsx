import { Action } from '@lib/rpg-api/types'
import {
  Box,
  Code,
  Heading,
  // Tab,
  // TabList,
  // TabPanel,
  // TabPanels,
  // Tabs,
} from '@chakra-ui/react'
import ArgForm from './forms/ArgForm'

export declare interface ActionPanelProps {
  action: Action
}

function ActionPanel(props: ActionPanelProps) {
  const { action } = props

  const onSubmit = (argValues: Record<string, unknown>) => {
    console.log('argValues', argValues)
  }

  return (
    <Box>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        {action.name}
      </Heading>
      <ArgForm argSet={action.actionArgs} onSubmit={onSubmit} />
      <Code>{JSON.stringify(action, undefined, 2)}</Code>
    </Box>
  )
}

export default ActionPanel
