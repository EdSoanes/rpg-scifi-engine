import React from 'react'
import { ActionInstance, RpgArg } from '../../lib/rpg-api/types'
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

export declare interface ActionInstancePanelProps {
  actionInstance: ActionInstance
}

function ActionInstancePanel(props: ActionInstancePanelProps) {
  const { actionInstance } = props

  const onSubmit = (argValues: {
    [key: string]: string | null | undefined
  }) => {}

  return (
    <Box>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        {actionInstance?.actionName}
      </Heading>
      <ArgForm
        argSet={Object.values(actionInstance!.args) as RpgArg[]}
        onSubmit={onSubmit}
      />
      <Code>{JSON.stringify(actionInstance, undefined, 2)}</Code>
    </Box>
  )
}

export default ActionInstancePanel
